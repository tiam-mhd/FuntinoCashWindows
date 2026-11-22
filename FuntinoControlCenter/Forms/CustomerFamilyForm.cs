using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Security;
using FuntinoControlCenter.Services;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;

namespace FuntinoControlCenter.Forms
{
    public partial class CustomerFamilyForm : Form
    {
        private const int PageSize = 50;
        private readonly List<string> customerCursors = new List<string>();
        private int currentPage = 1;
        private int pageCount = 1;
        private bool currentPageHasNext;
        private string nextCustomerCursor;
        private bool currentRowsCentral;
        private bool currentRowsStale;
        private bool snapshotFresh;
        private bool canViewFamily;
        private bool canPairOperator;
        private bool customerLoadBusy;
        private bool bindingCustomers;
        private bool pairingBusy;
        private int familyRequestVersion;
        private CancellationTokenSource pairingCancellation;
        private string pendingPairingId;
        private string pendingPollSecret;

        private sealed class CustomerPageResult
        {
            public DataTable Rows { get; set; }
            public int Total { get; set; }
            public bool IsCentral { get; set; }
            public bool IsStale { get; set; }
            public string NextCursor { get; set; }
        }

        private sealed class FamilyPageResult
        {
            public DataTable Members { get; set; }
            public DataTable Cards { get; set; }
            public string ProfileSummary { get; set; }
            public bool CardsAreSnapshot { get; set; }
        }

        public CustomerFamilyForm()
        {
            InitializeComponent();
        }

        private async void CustomerFamilyForm_Load(object sender, EventArgs e)
        {
            try
            {
                AuthorizationGuard.Require("customers.view");
                canViewFamily = AuthorizationGuard.Has("families.view");
                canPairOperator = AuthorizationGuard.Has("edge.operator.pair");
                pnlFamily.Visible = canViewFamily;
                tlpPairing.Visible = canPairOperator;
                AuthorizationMonitor.Attach(this, delegate
                {
                    return AuthorizationGuard.Has("customers.view") &&
                        (!canViewFamily || AuthorizationGuard.Has("families.view")) &&
                        (!canPairOperator || AuthorizationGuard.Has("edge.operator.pair"));
                });
                customerCursors.Add(null);
                RefreshPairingStatus();
                tmrOperatorExpiry.Start();
                await LoadSnapshotStatusAsync();
                await LoadCustomersAsync(false);
            }
            catch (UnauthorizedAccessException)
            {
                if (IsDisposed || Disposing) return;
                MessageBox.ShowWarning(this, "دسترسی مشاهدهٔ مشتریان شعبه برای این حساب فعال نیست.", "دسترسی محدود است");
                Close();
            }
            catch (Exception)
            {
                if (IsDisposed || Disposing) return;
                MessageBox.ShowError(this, "اطلاعات مشتریان بارگذاری نشد. وضعیت پایگاه عملیاتی را بررسی کنید.", "بارگذاری اطلاعات");
            }
        }

        private async Task LoadSnapshotStatusAsync()
        {
            EdgeSnapshotStatus snapshot = await Task.Run(delegate { return EdgeIntegrationRepository.GetCustomerSnapshotStatus(); });
            if (IsDisposed || Disposing) return;
            snapshotFresh = snapshot.IsFresh;
            string freshness = snapshot.IsFresh ? "تازه" : "منقضی یا دریافت‌نشده";
            string version = String.IsNullOrWhiteSpace(snapshot.Version) ? "دریافت نشده" : snapshot.Version;
            string expiry = snapshot.ExpiresAtUtc.HasValue
                ? PersianDigits(DateTime.SpecifyKind(snapshot.ExpiresAtUtc.Value, DateTimeKind.Utc).ToLocalTime().ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture))
                : "ثبت نشده";
            lblSnapshot.Text = "حافظهٔ فقط‌خواندنی شعبه «" + snapshot.BranchId + "»؛ نسخهٔ " + version +
                "؛ پایان اعتبار " + expiry + "؛ وضعیت: " + freshness;
        }

        private async Task LoadCustomersAsync(bool resetPaging)
        {
            if (customerLoadBusy || IsDisposed || Disposing) return;
            if (resetPaging)
            {
                customerCursors.Clear();
                customerCursors.Add(null);
                currentPage = 1;
            }
            if (customerCursors.Count == 0) customerCursors.Add(null);

            customerLoadBusy = true;
            familyRequestVersion++;
            currentPage = Math.Max(1, Math.Min(currentPage, customerCursors.Count));
            btnSearch.Enabled = false;
            btnPrevious.Enabled = false;
            btnNext.Enabled = false;
            gridCustomers.Enabled = false;
            loadingCustomers.BeginLoading("در حال جست‌وجوی مشتریان شعبه…", global::FuntinoControlCenter.Properties.Resources.Reports);
            try
            {
                string term = (txtSearch.Text ?? String.Empty).Trim();
                string branchId = AppSettings.Current.EdgeBranchId;
                string operatorToken = null;
                bool activeOperatorSession = EdgeOperatorSession.IsActive(branchId);
                bool canSearchCentrally = activeOperatorSession &&
                    EdgeOperatorSession.TryGetToken(branchId, "customer.lookup.limited", out operatorToken);

                if (activeOperatorSession && !canSearchCentrally)
                {
                    ClearCustomerData();
                    lblCustomerCount.Text = "مجوز جست‌وجوی مرکزی برای این اتصال صادر نشده است.";
                    lblCustomerPage.Text = String.Empty;
                    lblPairingStatus.Text = "برای دریافت دسترسی درست، اتصال امن را دوباره آغاز کنید.";
                    return;
                }
                if (activeOperatorSession && term.Length == 0)
                {
                    ClearCustomerData();
                    lblCustomerCount.Text = "برای جست‌وجوی مرکزی، نام یا شناسه را وارد کنید.";
                    lblCustomerPage.Text = String.Empty;
                    return;
                }

                CustomerPageResult result;
                if (canSearchCentrally)
                {
                    string cursor = customerCursors[currentPage - 1];
                    try
                    {
                        result = await Task.Run(delegate
                        {
                            using (var client = new EdgeApiClient(AppSettings.Current))
                            {
                                EdgeCustomerSearchPage page = client.SearchCustomers(term, cursor, PageSize, operatorToken);
                                if (page.NextCursor != null && customerCursors.Contains(page.NextCursor, StringComparer.Ordinal))
                                    throw new EdgeApiException("invalid-response", false);
                                return new CustomerPageResult
                                {
                                    Rows = ToCustomerTable(page.Items),
                                    Total = page.Items.Count,
                                    IsCentral = true,
                                    IsStale = false,
                                    NextCursor = page.NextCursor
                                };
                            }
                        });
                    }
                    catch (EdgeApiException exception)
                    {
                        if (exception.HttpStatusCode == 401)
                        {
                            EdgeOperatorSession.Clear();
                            ClearCustomerData();
                            lblCustomerCount.Text = "مجوز مرکزی پایان یافته است؛ برای ادامه اتصال امن تازه بگیرید.";
                            RefreshPairingStatus();
                            return;
                        }
                        if (exception.HttpStatusCode == 403)
                        {
                            ClearCustomerData();
                            lblCustomerCount.Text = "مجوز یا شعبهٔ این اتصال اجازهٔ دیدن این اطلاعات را نمی‌دهد.";
                            return;
                        }
                        if (exception.HttpStatusCode == 429)
                        {
                            ClearCustomerData();
                            lblCustomerCount.Text = "درخواست‌ها موقتاً محدود شده‌اند؛ کمی بعد دوباره تلاش کنید.";
                            return;
                        }
                        if (exception.HttpStatusCode < 500 && exception.SafeCode != "network-unavailable") throw;
                        result = await LoadLocalCustomersAsync(term, currentPage, true);
                    }
                }
                else
                {
                    result = await LoadLocalCustomersAsync(term, currentPage, !snapshotFresh);
                }

                if (IsDisposed || Disposing) return;
                bindingCustomers = true;
                gridCustomers.DataSource = result.Rows;
                currentRowsCentral = result.IsCentral;
                currentRowsStale = result.IsStale;
                currentPageHasNext = result.IsCentral && !String.IsNullOrWhiteSpace(result.NextCursor);
                nextCustomerCursor = result.NextCursor;
                if (!result.IsCentral) pageCount = Math.Max(1, (int)Math.Ceiling(result.Total / (double)PageSize));
                lblCustomerPage.Text = result.IsCentral
                    ? "صفحهٔ " + PersianDigits(currentPage.ToString(CultureInfo.InvariantCulture)) + " | سامانهٔ مرکزی"
                    : "صفحهٔ " + PersianDigits(currentPage.ToString(CultureInfo.InvariantCulture)) + " از " + PersianDigits(pageCount.ToString(CultureInfo.InvariantCulture));
                lblCustomerCount.Text = result.IsCentral
                    ? PersianDigits(result.Total.ToString("N0", CultureInfo.InvariantCulture)) + " نتیجه در این صفحه"
                    : PersianDigits(result.Total.ToString("N0", CultureInfo.InvariantCulture)) + " مشتری در حافظهٔ شعبه";
                btnPrevious.Enabled = currentPage > 1;
                btnNext.Enabled = result.IsCentral ? currentPageHasNext : currentPage < pageCount;
                if (result.IsStale)
                    lblPairingStatus.Text = "نمایش فقط‌خواندنی از تصویر محلی؛ ارتباط مرکزی در دسترس نیست.";
                else if (result.IsCentral)
                    RefreshPairingStatus();
                else
                    lblPairingStatus.Text = snapshotFresh ? "نمایش فقط‌خواندنی از تصویر تازهٔ شعبه." : "تصویر محلی منقضی است؛ اطلاعات ممکن است قدیمی باشد.";

                if (gridCustomers.Rows.Count > 0)
                {
                    gridCustomers.ClearSelection();
                    gridCustomers.Rows[0].Selected = true;
                    gridCustomers.CurrentCell = gridCustomers.Rows[0].Cells[0];
                }
                else
                {
                    gridFamily.DataSource = null;
                    gridCards.DataSource = null;
                    lblFamilyCustomer.Text = canViewFamily ? "برای دیدن اطلاعات خانواده، یک مشتری را انتخاب کنید." : String.Empty;
                }
                bindingCustomers = false;
                if (gridCustomers.Rows.Count > 0 && canViewFamily)
                    CustomerSelectionChanged(this, EventArgs.Empty);
            }
            catch (UnauthorizedAccessException)
            {
                if (!IsDisposed && !Disposing)
                {
                    MessageBox.ShowWarning(this, "دسترسی مشاهدهٔ مشتریان تغییر کرده است. پنجره بسته می‌شود.", "دسترسی محدود است");
                    Close();
                }
            }
            catch (EdgeApiException)
            {
                if (!IsDisposed && !Disposing)
                {
                    ClearCustomerData();
                    lblCustomerCount.Text = "پاسخ سامانهٔ مرکزی معتبر نبود؛ اطلاعات نمایش داده نشد.";
                }
            }
            catch (Exception)
            {
                if (!IsDisposed && !Disposing)
                {
                    ClearCustomerData();
                    lblCustomerCount.Text = "فهرست مشتریان بارگذاری نشد؛ اتصال و تصویر شعبه را بررسی کنید.";
                }
            }
            finally
            {
                bindingCustomers = false;
                customerLoadBusy = false;
                if (!IsDisposed && !Disposing)
                {
                    loadingCustomers.EndLoading();
                    btnSearch.Enabled = true;
                    gridCustomers.Enabled = true;
                    btnPrevious.Enabled = currentPage > 1;
                    btnNext.Enabled = currentRowsCentral ? currentPageHasNext : currentPage < pageCount;
                }
            }
        }

        private static async Task<CustomerPageResult> LoadLocalCustomersAsync(string term, int page, bool stale)
        {
            return await Task.Run(delegate
            {
                int total;
                DataTable rows = EdgeIntegrationRepository.SearchCustomers(term, page, PageSize, out total);
                if (!rows.Columns.Contains("MaskedPhone")) rows.Columns.Add("MaskedPhone", typeof(string));
                if (!rows.Columns.Contains("MemberId")) rows.Columns.Add("MemberId", typeof(string));
                return new CustomerPageResult { Rows = rows, Total = total, IsCentral = false, IsStale = stale };
            });
        }

        private static DataTable ToCustomerTable(IList<EdgeCustomerSearchItem> items)
        {
            var table = new DataTable();
            table.Columns.Add("CustomerId", typeof(string));
            table.Columns.Add("DisplayName", typeof(string));
            table.Columns.Add("MaskedPhone", typeof(string));
            table.Columns.Add("HouseholdId", typeof(string));
            table.Columns.Add("MemberId", typeof(string));
            table.Columns.Add("SnapshotVersion", typeof(string));
            foreach (EdgeCustomerSearchItem item in items)
                table.Rows.Add(item.CustomerId, item.DisplayName, item.MaskedPhone, (object)item.HouseholdId ?? DBNull.Value,
                    (object)item.MemberId ?? DBNull.Value, item.RecordVersion.ToString(CultureInfo.InvariantCulture));
            return table;
        }

        private void ClearCustomerData()
        {
            bindingCustomers = true;
            gridCustomers.DataSource = null;
            gridFamily.DataSource = null;
            gridCards.DataSource = null;
            currentRowsCentral = false;
            currentRowsStale = true;
            currentPageHasNext = false;
            nextCustomerCursor = null;
            bindingCustomers = false;
        }

        private async void CustomerSelectionChanged(object sender, EventArgs e)
        {
            if (bindingCustomers || !canViewFamily || gridCustomers.SelectedRows.Count == 0 || IsDisposed || Disposing) return;
            DataGridViewRow row = gridCustomers.SelectedRows[0];
            string customerName = Convert.ToString(row.Cells["colCustomerName"].Value, CultureInfo.InvariantCulture);
            string customerId = Convert.ToString(row.Cells["colCustomerId"].Value, CultureInfo.InvariantCulture);
            object householdValue = row.Cells["colHouseholdId"].Value;
            string householdId = householdValue == null || householdValue == DBNull.Value
                ? null : Convert.ToString(householdValue, CultureInfo.InvariantCulture);
            int requestVersion = ++familyRequestVersion;
            if (String.IsNullOrWhiteSpace(householdId))
            {
                gridFamily.DataSource = null;
                gridCards.DataSource = null;
                lblFamilyCustomer.Text = customerName + "؛ در اطلاعات در دسترس این شعبه خانواده‌ای ثبت نشده است.";
                return;
            }
            await LoadFamilyAsync(customerName, customerId, householdId, requestVersion);
        }

        private async Task LoadFamilyAsync(string customerName, string customerId, string householdId, int requestVersion)
        {
            loadingFamily.BeginLoading("در حال دریافت پرونده و اعضای خانواده…", global::FuntinoControlCenter.Properties.Resources.CardHistory);
            try
            {
                string branchId = AppSettings.Current.EdgeBranchId;
                string lookupToken = null;
                string familyToken = null;
                bool operatorActive = EdgeOperatorSession.IsActive(branchId);
                if (currentRowsCentral && !operatorActive)
                {
                    ClearCustomerData();
                    lblFamilyCustomer.Text = "مجوز مرکزی پایان یافته است؛ برای مشاهدهٔ دوباره اتصال امن بگیرید.";
                    RefreshPairingStatus();
                    return;
                }
                bool centralLookup = currentRowsCentral && operatorActive &&
                    EdgeOperatorSession.TryGetToken(branchId, "customer.lookup.limited", out lookupToken);
                bool centralFamily = centralLookup &&
                    EdgeOperatorSession.TryGetToken(branchId, "customer.family.read", out familyToken);
                FamilyPageResult result;

                if (currentRowsCentral && operatorActive && !centralFamily)
                {
                    gridFamily.DataSource = null;
                    gridCards.DataSource = null;
                    lblFamilyCustomer.Text = "مجوز خواندن خانواده برای این اتصال صادر نشده است.";
                    return;
                }

                if (centralFamily)
                {
                    try
                    {
                        result = await Task.Run(delegate
                        {
                            using (var client = new EdgeApiClient(AppSettings.Current))
                            {
                                EdgeCustomerProfile profile = client.ReadCustomerProfile(customerId, lookupToken);
                                EdgeHouseholdRead household = client.ReadHousehold(householdId, familyToken);
                                if (!String.Equals(profile.CustomerId, customerId, StringComparison.OrdinalIgnoreCase) ||
                                    !String.Equals(household.Id, householdId, StringComparison.OrdinalIgnoreCase))
                                    throw new EdgeApiException("invalid-response", false);
                                return new FamilyPageResult
                                {
                                    Members = ToHouseholdTable(household),
                                    Cards = EdgeIntegrationRepository.GetHouseholdCards(householdId),
                                    ProfileSummary = "تلفن " + profile.MaskedPhone + "؛ نسخهٔ پرونده " + profile.RecordVersion.ToString(CultureInfo.InvariantCulture),
                                    CardsAreSnapshot = true
                                };
                            }
                        });
                    }
                    catch (EdgeApiException exception)
                    {
                        if (exception.HttpStatusCode == 401)
                        {
                            if (requestVersion != familyRequestVersion) return;
                            EdgeOperatorSession.Clear();
                            gridFamily.DataSource = null;
                            gridCards.DataSource = null;
                            lblFamilyCustomer.Text = "مجوز مرکزی پایان یافته است؛ دوباره اتصال امن بگیرید.";
                            RefreshPairingStatus();
                            return;
                        }
                        if (exception.HttpStatusCode == 403)
                        {
                            if (requestVersion != familyRequestVersion) return;
                            gridFamily.DataSource = null;
                            gridCards.DataSource = null;
                            lblFamilyCustomer.Text = "مجوز یا شعبه اجازهٔ مشاهدهٔ این خانواده را نمی‌دهد.";
                            return;
                        }
                        if (exception.HttpStatusCode == 429)
                        {
                            if (requestVersion != familyRequestVersion) return;
                            gridFamily.DataSource = null;
                            gridCards.DataSource = null;
                            lblFamilyCustomer.Text = "درخواست‌ها موقتاً محدود شده‌اند؛ کمی بعد دوباره تلاش کنید.";
                            return;
                        }
                        if (exception.HttpStatusCode < 500 && exception.SafeCode != "network-unavailable") throw;
                        result = await LoadLocalFamilyAsync(householdId);
                        result.ProfileSummary = "ارتباط مرکزی در دسترس نیست؛ جزئیات از تصویر محلی خوانده شد.";
                        result.CardsAreSnapshot = true;
                    }
                }
                else
                {
                    result = await LoadLocalFamilyAsync(householdId);
                    result.ProfileSummary = currentRowsStale
                        ? "اطلاعات از تصویر محلی و احتمالاً منقضی شعبه خوانده شد."
                        : "اطلاعات از تصویر فقط‌خواندنی شعبه خوانده شد.";
                    result.CardsAreSnapshot = true;
                }

                if (IsDisposed || Disposing || requestVersion != familyRequestVersion) return;
                gridFamily.DataSource = result.Members;
                gridCards.DataSource = result.Cards;
                SetFamilyGridCaptions();
                lblFamilyCustomer.Text = customerName + "؛ " + result.ProfileSummary +
                    (result.CardsAreSnapshot ? "؛ کارت‌ها از تصویر محلی شعبه‌اند." : String.Empty);
            }
            catch (UnauthorizedAccessException)
            {
                if (!IsDisposed && !Disposing)
                {
                    MessageBox.ShowWarning(this, "مجوز مشاهدهٔ خانواده برای این حساب تغییر کرده است.", "دسترسی محدود است");
                    pnlFamily.Visible = false;
                    canViewFamily = false;
                }
            }
            catch (EdgeApiException)
            {
                if (!IsDisposed && !Disposing && requestVersion == familyRequestVersion)
                {
                    gridFamily.DataSource = null;
                    gridCards.DataSource = null;
                    lblFamilyCustomer.Text = "پاسخ مرکزی معتبر نبود؛ اطلاعات خانواده نمایش داده نشد.";
                }
            }
            catch (Exception)
            {
                if (!IsDisposed && !Disposing && requestVersion == familyRequestVersion)
                {
                    gridFamily.DataSource = null;
                    gridCards.DataSource = null;
                    lblFamilyCustomer.Text = "اطلاعات خانواده بارگذاری نشد؛ اتصال یا تصویر شعبه را بررسی کنید.";
                }
            }
            finally
            {
                if (!IsDisposed && !Disposing && requestVersion == familyRequestVersion) loadingFamily.EndLoading();
            }
        }

        private static async Task<FamilyPageResult> LoadLocalFamilyAsync(string householdId)
        {
            return await Task.Run(delegate
            {
                return new FamilyPageResult
                {
                    Members = EdgeIntegrationRepository.GetHousehold(householdId),
                    Cards = EdgeIntegrationRepository.GetHouseholdCards(householdId)
                };
            });
        }

        private static DataTable ToHouseholdTable(EdgeHouseholdRead household)
        {
            var table = new DataTable();
            table.Columns.Add("MemberType", typeof(string));
            table.Columns.Add("MemberId", typeof(string));
            table.Columns.Add("DisplayName", typeof(string));
            table.Columns.Add("RelatedCustomerId", typeof(string));
            table.Rows.Add("خانواده", household.Id, household.Name + " | " + household.Status + " | نسخهٔ " + household.RecordVersion, DBNull.Value);
            foreach (EdgeHouseholdReadMember member in household.Members)
                table.Rows.Add(member.Relationship + " | " + member.Status, member.Id, member.DisplayName, DBNull.Value);
            return table;
        }

        private void SetFamilyGridCaptions()
        {
            SetHeader(gridFamily, "MemberType", "نقش");
            SetHeader(gridFamily, "MemberId", "شناسه");
            SetHeader(gridFamily, "DisplayName", "نام");
            SetHeader(gridFamily, "RelatedCustomerId", "شناسهٔ مشتری مرتبط");
            SetHeader(gridCards, "CardId", "شناسهٔ کارت");
            SetHeader(gridCards, "HouseholdId", "شناسهٔ خانواده");
            SetHeader(gridCards, "MemberId", "شناسهٔ عضو");
            SetHeader(gridCards, "SnapshotVersion", "نسخهٔ تصویر");
        }

        private static void SetHeader(DataGridView grid, string columnName, string caption)
        {
            if (grid.Columns.Contains(columnName)) grid.Columns[columnName].HeaderText = caption;
        }

        private async void PairOperator_Click(object sender, EventArgs e)
        {
            if (pairingBusy || !canPairOperator) return;
            try { AuthorizationGuard.Require("edge.operator.pair"); }
            catch (UnauthorizedAccessException) { return; }

            pairingBusy = true;
            btnPairOperator.Enabled = false;
            btnCancelPairing.Visible = true;
            btnCancelPairing.Enabled = true;
            txtPairingCode.Clear();
            loadingPairing.BeginLoading("در انتظار تأیید امن در پنل وب…", global::FuntinoControlCenter.Properties.Resources.Reports);
            lblPairingStatus.Text = "در حال ایجاد اتصال؛ اطلاعات موقت ذخیره نمی‌شود.";
            try
            {
                EdgeOperatorPairingStartResponse start = await Task.Run(delegate
                {
                    using (var client = new EdgeApiClient(AppSettings.Current)) return client.StartOperatorPairing();
                });
                if (IsDisposed || Disposing) return;

                pendingPairingId = start.PairingId;
                pendingPollSecret = start.PollSecret;
                start.PollSecret = null;
                txtPairingCode.Text = start.UserCode;
                DateTimeOffset expiry = DateTimeOffset.Parse(start.ExpiresAt, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                lblPairingStatus.Text = "کد را در پنل وب، بخش همگام‌سازی دستگاه‌ها وارد و دسترسی لازم را تأیید کنید. پایان: " +
                    PersianDigits(expiry.ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture));

                pairingCancellation = new CancellationTokenSource();
                await PollPairingAsync(start.PairingId, pendingPollSecret, expiry, start.PollIntervalSeconds, pairingCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                if (!IsDisposed && !Disposing) lblPairingStatus.Text = "درخواست اتصال لغو شد.";
            }
            catch (EdgeApiException exception)
            {
                if (!IsDisposed && !Disposing) lblPairingStatus.Text = PairingErrorMessage(exception);
            }
            catch (Exception)
            {
                if (!IsDisposed && !Disposing) lblPairingStatus.Text = "اتصال آغاز نشد؛ تنظیمات امن و ارتباط شبکه را بررسی کنید.";
            }
            finally
            {
                pairingBusy = false;
                pendingPairingId = null;
                pendingPollSecret = null;
                if (pairingCancellation != null)
                {
                    pairingCancellation.Dispose();
                    pairingCancellation = null;
                }
                if (!IsDisposed && !Disposing)
                {
                    loadingPairing.EndLoading();
                    btnPairOperator.Enabled = true;
                    btnCancelPairing.Visible = false;
                    btnCancelPairing.Enabled = false;
                    if (EdgeOperatorSession.IsActive(AppSettings.Current.EdgeBranchId))
                    {
                        RefreshPairingStatus();
                        if (!String.IsNullOrWhiteSpace(txtSearch.Text)) await LoadCustomersAsync(true);
                    }
                }
            }
        }

        private async Task PollPairingAsync(string pairingId, string pollSecret, DateTimeOffset expiresAt, int pollIntervalSeconds, CancellationToken cancellationToken)
        {
            int delaySeconds = Math.Max(3, pollIntervalSeconds);
            while (!cancellationToken.IsCancellationRequested && DateTimeOffset.UtcNow < expiresAt)
            {
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                EdgeOperatorPairingPollResponse response;
                try
                {
                    response = await Task.Run(delegate
                    {
                        using (var client = new EdgeApiClient(AppSettings.Current)) return client.PollOperatorPairing(pairingId, pollSecret);
                    });
                }
                catch (EdgeApiException exception)
                {
                    if (exception.HttpStatusCode == 429 || exception.HttpStatusCode >= 500 || exception.SafeCode == "network-unavailable")
                    {
                        delaySeconds = Math.Min(30, Math.Max(delaySeconds + 1, delaySeconds * 2));
                        lblPairingStatus.Text = "پاسخی دریافت نشد؛ تلاش امن با فاصلهٔ بیشتر ادامه دارد.";
                        continue;
                    }
                    lblPairingStatus.Text = PairingErrorMessage(exception);
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();
                delaySeconds = Math.Max(3, pollIntervalSeconds);
                switch (response.Status)
                {
                    case "pending":
                        lblPairingStatus.Text = "در انتظار تأیید کاربر مجاز در پنل وب…";
                        break;
                    case "approved":
                        lblPairingStatus.Text = "تأیید شد؛ در حال دریافت مجوز کوتاه‌مدت…";
                        break;
                    case "authorized":
                        EdgeOperatorSession.Store(response.OperatorToken, AppSettings.Current.EdgeOrganizationId, AppSettings.Current.EdgeBranchId);
                        response.OperatorToken.Token = null;
                        txtPairingCode.Clear();
                        lblPairingStatus.Text = "اتصال امن برقرار شد؛ دسترسی فقط تا پایان مهلت مجوز در حافظه می‌ماند.";
                        return;
                    case "expired":
                        txtPairingCode.Clear();
                        lblPairingStatus.Text = "مهلت کد اتصال تمام شد؛ اتصال تازه آغاز کنید.";
                        return;
                    case "cancelled":
                    case "restart_required":
                        txtPairingCode.Clear();
                        lblPairingStatus.Text = "این درخواست دیگر قابل استفاده نیست؛ اتصال تازه آغاز کنید.";
                        return;
                    default:
                        throw new EdgeApiException("invalid-pairing-response", false);
                }
            }
            if (!cancellationToken.IsCancellationRequested && !IsDisposed && !Disposing)
            {
                txtPairingCode.Clear();
                lblPairingStatus.Text = "مهلت کد اتصال تمام شد؛ اتصال تازه آغاز کنید.";
            }
        }

        private async void CancelPairing_Click(object sender, EventArgs e)
        {
            CancellationTokenSource cancellation = pairingCancellation;
            string pairingId = pendingPairingId;
            string pollSecret = pendingPollSecret;
            if (cancellation != null) cancellation.Cancel();
            btnCancelPairing.Enabled = false;
            lblPairingStatus.Text = "در حال لغو درخواست اتصال…";
            try
            {
                if (!String.IsNullOrWhiteSpace(pairingId) && !String.IsNullOrWhiteSpace(pollSecret))
                    await Task.Run(delegate
                    {
                        using (var client = new EdgeApiClient(AppSettings.Current)) client.CancelOperatorPairing(pairingId, pollSecret);
                    });
            }
            catch (Exception) { }
            if (!IsDisposed && !Disposing) txtPairingCode.Clear();
        }

        private void CustomerFamilyForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            tmrOperatorExpiry.Stop();
            if (pairingCancellation != null) pairingCancellation.Cancel();
            string pairingId = pendingPairingId;
            string pollSecret = pendingPollSecret;
            if (String.IsNullOrWhiteSpace(pairingId) || String.IsNullOrWhiteSpace(pollSecret)) return;
            Task.Run(delegate
            {
                try
                {
                    using (var client = new EdgeApiClient(AppSettings.Current)) client.CancelOperatorPairing(pairingId, pollSecret);
                }
                catch (Exception) { }
            });
        }

        private void OperatorSessionTimer_Tick(object sender, EventArgs e)
        {
            if (!currentRowsCentral || EdgeOperatorSession.IsActive(AppSettings.Current.EdgeBranchId)) return;
            ClearCustomerData();
            lblCustomerCount.Text = "مجوز مرکزی پایان یافته است؛ برای مشاهدهٔ دوباره اتصال امن بگیرید.";
            lblCustomerPage.Text = String.Empty;
            lblFamilyCustomer.Text = "پس از دریافت مجوز تازه، مشتری را دوباره جست‌وجو کنید.";
            lblPairingStatus.Text = "مجوز مرکزی پایان یافت؛ اتصال تازه لازم است.";
        }

        private static string PairingErrorMessage(EdgeApiException exception)
        {
            if (exception.HttpStatusCode == 401) return "نصب یا کلید ارتباطی پذیرفته نشد؛ تنظیمات Edge را بررسی کنید.";
            if (exception.HttpStatusCode == 403) return "مجوز محلی یا دسترسی شعبه برای اتصال کافی نیست.";
            if (exception.HttpStatusCode == 429) return "درخواست‌ها موقتاً محدود شده‌اند؛ بعداً دوباره تلاش کنید.";
            if (exception.HttpStatusCode >= 500 || exception.SafeCode == "network-unavailable") return "ارتباط امن با سرور برقرار نشد؛ دوباره تلاش کنید.";
            return "پاسخ اتصال معتبر نبود؛ درخواست تازه‌ای آغاز کنید.";
        }

        private void RefreshPairingStatus()
        {
            string branchId = AppSettings.Current.EdgeBranchId;
            DateTime? expires = EdgeOperatorSession.ExpiresAtUtc;
            if (EdgeOperatorSession.IsActive(branchId) && expires.HasValue)
            {
                lblPairingStatus.Text = "مجوز مرکزی تا " + PersianDigits(expires.Value.ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture)) + " معتبر است.";
                return;
            }
            if (!pairingBusy) lblPairingStatus.Text = "برای خواندن مرکزی، اتصال را آغاز کنید.";
        }

        private async void Search_Click(object sender, EventArgs e)
        {
            await LoadCustomersAsync(true);
        }

        private async void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            await LoadCustomersAsync(true);
        }

        private async void Previous_Click(object sender, EventArgs e)
        {
            if (currentPage <= 1 || customerLoadBusy) return;
            currentPage--;
            await LoadCustomersAsync(false);
        }

        private async void Next_Click(object sender, EventArgs e)
        {
            if (customerLoadBusy) return;
            if (currentRowsCentral && currentPageHasNext && !String.IsNullOrWhiteSpace(nextCustomerCursor))
            {
                customerCursors.Add(nextCustomerCursor);
                currentPage++;
                await LoadCustomersAsync(false);
            }
            else if (!currentRowsCentral && currentPage < pageCount)
            {
                currentPage++;
                await LoadCustomersAsync(false);
            }
        }

        private static string PersianDigits(string value)
        {
            if (value == null) return String.Empty;
            return value.Replace('0', '۰').Replace('1', '۱').Replace('2', '۲').Replace('3', '۳').Replace('4', '۴')
                .Replace('5', '۵').Replace('6', '۶').Replace('7', '۷').Replace('8', '۸').Replace('9', '۹');
        }

        private void Back_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
