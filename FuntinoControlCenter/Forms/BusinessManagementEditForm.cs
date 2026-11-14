using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Security;

namespace FuntinoControlCenter.Forms
{
    public partial class BusinessManagementEditForm : Form
    {
        private readonly BusinessManagementArea area;
        private readonly ManagementEditMode mode;
        private readonly BusinessManagementRecord original;

        internal BusinessManagementEditForm(BusinessManagementArea area, ManagementEditMode mode, BusinessManagementRecord record)
        {
            this.area = area;
            this.mode = mode;
            this.original = record;
            InitializeComponent();
        }

        private void BusinessManagementEditForm_Load(object sender, EventArgs e)
        {
            try
            {
                string action = mode == ManagementEditMode.Create ? "create" : mode == ManagementEditMode.Update ? "update" : "delete";
                BusinessManagementRepository.RequireAccess(area, action);
                ConfigureFields();
                LoadValues();
                if (mode == ManagementEditMode.Update) btnSave.Image = global::FuntinoControlCenter.Properties.Resources.ActionEdit;
                btnSave.Visible = mode != ManagementEditMode.Deactivate;
                btnDeactivate.Visible = mode == ManagementEditMode.Deactivate;
                lblArchiveNote.Visible = mode == ManagementEditMode.Deactivate;
                if (mode == ManagementEditMode.Deactivate)
                {
                    SetInputsReadOnly();
                    chkActive.Visible = chkHalfPrice.Visible = false;
                }
                AuthorizationMonitor.Attach(this, delegate { return BusinessManagementRepository.HasAccess(area, action); });
            }
            catch (Exception ex)
            {
                AppMessageService.ShowError(this, DatabaseManager.SafeMessage(ex), "دسترسی به فرم تغییر اطلاعات");
                Close();
            }
        }

        private void ConfigureFields()
        {
            Control[] allFields = { lblCode, pnlCode, lblName, pnlName, lblAmount, pnlAmount, lblPercent, pnlPercent,
                lblStartsAt, dtpStartsAt, lblEndsAt, dtpEndsAt, lblTier, cmbValue, lblWeekday, cmbWeekday,
                lblMinimum, pnlMinimum, lblMaximum, pnlMaximum, lblPricingType, lblDuration, pnlDuration,
                lblDevice, cmbDevice, lblPromotion, cmbPromotion, chkActive, chkHalfPrice };
            foreach (Control control in allFields) control.Visible = false;

            lblTitle.Text = area == BusinessManagementArea.RechargeLimit && mode == ManagementEditMode.Update
                ? "ثبت سقف تازه‌ی شارژ"
                : (mode == ManagementEditMode.Create ? "افزودن " : mode == ManagementEditMode.Deactivate ? "غیرفعال‌سازی " : "ویرایش ") + AreaTitle(area);
            if (area == BusinessManagementArea.RechargeLimit && mode != ManagementEditMode.Deactivate)
                btnSave.Text = "ثبت سقف تازه";

            switch (area)
            {
                case BusinessManagementArea.Attractions:
                    Show(lblCode, pnlCode, lblName, pnlName, lblAmount, pnlAmount, chkActive);
                    break;
                case BusinessManagementArea.ControllerDevices:
                    Show(lblCode, pnlCode, lblName, pnlName, lblAmount, pnlAmount, lblPricingType, cmbValue, lblDuration, pnlDuration, chkActive, chkHalfPrice);
                    break;
                case BusinessManagementArea.Promotions:
                    Show(lblCode, pnlCode, lblName, pnlName, lblPercent, pnlPercent, lblStartsAt, dtpStartsAt, lblEndsAt, dtpEndsAt);
                    break;
                case BusinessManagementArea.DiscountPolicies:
                    Show(lblTier, cmbValue, lblWeekday, cmbWeekday, lblMinimum, pnlMinimum, lblMaximum, pnlMaximum, chkActive);
                    break;
                case BusinessManagementArea.DiscountRates:
                    Show(lblCode, pnlCode, lblPercent, pnlPercent);
                    txtCode.ReadOnly = true;
                    break;
                case BusinessManagementArea.Deposits:
                    Show(lblAmount, pnlAmount);
                    break;
                case BusinessManagementArea.RechargeLimit:
                    Show(lblAmount, pnlAmount);
                    break;
                case BusinessManagementArea.HourlyPlans:
                    Show(lblName, pnlName, lblAmount, pnlAmount, chkActive); txtName.ReadOnly = true;
                    break;
                case BusinessManagementArea.Schedules:
                    Show(lblStartsAt, dtpStartsAt, chkActive); lblStartsAt.Text = "زمان فعال‌سازی شارژ";
                    break;
                case BusinessManagementArea.Tickets:
                    Show(lblAmount, pnlAmount, chkActive);
                    break;
                case BusinessManagementArea.DevicePromotions:
                    Show(lblDevice, cmbDevice, lblPromotion, cmbPromotion, chkActive);
                    break;
            }

            if (mode == ManagementEditMode.Deactivate) lblArchiveNote.Visible = true;
            if (area == BusinessManagementArea.DiscountPolicies) LoadDiscountTiers();
            if (area == BusinessManagementArea.DevicePromotions) LoadAssignmentChoices();
        }

        private void LoadValues()
        {
            var record = original ?? new BusinessManagementRecord
            {
                IsActive = true,
                StartsAt = DateTime.Now,
                EndsAt = DateTime.Now.AddDays(7),
                Weekday = 1,
                Maximum = Int32.MaxValue,
                PricingType = 1
            };

            txtCode.Text = record.Code.ToString(CultureInfo.InvariantCulture);
            txtName.Text = record.Name ?? String.Empty;
            txtAmount.Text = record.Amount.ToString("N0", CultureInfo.GetCultureInfo("fa-IR"));
            txtPercent.Text = record.Percent.ToString(CultureInfo.InvariantCulture);
            txtMinimum.Text = record.Minimum.ToString("N0", CultureInfo.GetCultureInfo("fa-IR"));
            txtMaximum.Text = record.Maximum.ToString("N0", CultureInfo.GetCultureInfo("fa-IR"));
            txtDuration.Text = record.Duration.ToString(CultureInfo.InvariantCulture);
            dtpStartsAt.Value = SafeDate(record.StartsAt, DateTime.Now);
            dtpEndsAt.Value = SafeDate(record.EndsAt, DateTime.Now.AddDays(7));
            cmbWeekday.SelectedIndex = Math.Max(0, Math.Min(6, record.Weekday - 1));
            chkActive.Checked = record.IsActive;
            chkHalfPrice.Checked = record.IsHalfPrice;

            if (area == BusinessManagementArea.ControllerDevices)
                cmbValue.SelectedIndex = record.PricingType == 1 ? 0 : 1;
            else if (area == BusinessManagementArea.DiscountPolicies && cmbValue.DataSource != null)
                SelectComboValue(cmbValue, record.Code);
            else if (area == BusinessManagementArea.DevicePromotions)
            {
                SelectComboValue(cmbDevice, record.Extra1);
                SelectComboValue(cmbPromotion, record.Extra2);
            }
        }

        private void LoadDiscountTiers()
        {
            DataTable tiers = BusinessManagementRepository.Load(BusinessManagementArea.DiscountRates);
            cmbValue.Items.Clear();
            cmbValue.DisplayMember = "Name";
            cmbValue.ValueMember = "RecordId";
            cmbValue.DataSource = tiers;
            if (tiers.Rows.Count == 0) throw new InvalidOperationException("هیچ سطح تخفیفی تعریف نشده است؛ ابتدا درصدهای سطح تخفیف را ثبت کنید.");
        }

        private void LoadAssignmentChoices()
        {
            DataTable devices = ActiveOnly(BusinessManagementRepository.Load(BusinessManagementArea.ControllerDevices), false);
            DataTable promotions = ActiveOnly(BusinessManagementRepository.Load(BusinessManagementArea.Promotions), true);
            if (devices.Rows.Count == 0) throw new InvalidOperationException("هیچ دستگاه فعالی برای تخصیص جشنواره وجود ندارد.");
            if (promotions.Rows.Count == 0) throw new InvalidOperationException("جشنواره‌ای که هنوز پایان نیافته باشد پیدا نشد.");
            cmbDevice.DisplayMember = "Name"; cmbDevice.ValueMember = "RecordId"; cmbDevice.DataSource = devices;
            cmbPromotion.DisplayMember = "Name"; cmbPromotion.ValueMember = "Code"; cmbPromotion.DataSource = promotions;
        }

        private static DataTable ActiveOnly(DataTable source, bool includeFuturePromotions)
        {
            DataTable result = source.Clone();
            foreach (DataRow row in source.Rows)
            {
                if (includeFuturePromotions && source.Columns.Contains("Synchronized") &&
                    !Convert.ToBoolean(row["Synchronized"], CultureInfo.InvariantCulture)) continue;
                bool active = row["Active"] != DBNull.Value && Convert.ToBoolean(row["Active"], CultureInfo.InvariantCulture);
                if (active || (includeFuturePromotions && row["EndsAt"] != DBNull.Value && Convert.ToDateTime(row["EndsAt"], CultureInfo.InvariantCulture) >= DateTime.Now))
                    result.ImportRow(row);
            }
            return result;
        }

        private static void SelectComboValue(ComboBox combo, int value)
        {
            if (combo.DataSource == null) return;
            try { combo.SelectedValue = value; }
            catch { combo.SelectedIndex = combo.Items.Count == 0 ? -1 : 0; }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            try
            {
                BusinessManagementRecord record = ReadRecord();
                BusinessManagementRepository.Save(area, record, mode == ManagementEditMode.Create);
                AppMessageService.ShowSuccess(this, "اطلاعات با موفقیت ذخیره شد.", "ذخیره شد");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                AppMessageService.ShowError(this, DatabaseManager.SafeMessage(ex), "ذخیره‌ی اطلاعات انجام نشد");
            }
        }

        private void Deactivate_Click(object sender, EventArgs e)
        {
            if (original == null) return;
            if (AppMessageService.Show(this, "این مورد غیرفعال می‌شود و سوابق ثبت‌شده‌ی قبلی باقی می‌ماند. ادامه می‌دهید؟", "تأیید غیرفعال‌سازی", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;
            try
            {
                BusinessManagementRepository.Deactivate(area, original);
                AppMessageService.ShowSuccess(this, "مورد از استفاده‌ی بعدی خارج شد.", "غیرفعال‌سازی انجام شد");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                AppMessageService.ShowError(this, DatabaseManager.SafeMessage(ex), "غیرفعال‌سازی انجام نشد");
            }
        }

        private BusinessManagementRecord ReadRecord()
        {
            BusinessManagementRecord record = original ?? new BusinessManagementRecord();
            switch (area)
            {
                case BusinessManagementArea.Attractions:
                    record.Code = ParseInt(txtCode, "کد دستگاه"); record.Name = txtName.Text.Trim(); record.Amount = ParseLong(txtAmount, "قیمت پایه"); record.IsActive = chkActive.Checked;
                    break;
                case BusinessManagementArea.ControllerDevices:
                    record.Code = ParseInt(txtCode, "کد دستگاه"); record.Name = txtName.Text.Trim(); record.Amount = ParseLong(txtAmount, "قیمت پایه");
                    record.PricingType = cmbValue.SelectedIndex == 0 ? 1 : 0; record.IsHalfPrice = chkHalfPrice.Checked; record.Duration = String.IsNullOrWhiteSpace(txtDuration.Text) ? 0 : ParseInt(txtDuration, "زمان دستگاه"); record.IsActive = chkActive.Checked;
                    break;
                case BusinessManagementArea.Promotions:
                    record.Code = ParseInt(txtCode, "کد جشنواره"); record.Name = txtName.Text.Trim(); record.Percent = ParseInt(txtPercent, "درصد تخفیف"); record.StartsAt = dtpStartsAt.Value; record.EndsAt = dtpEndsAt.Value;
                    break;
                case BusinessManagementArea.DiscountPolicies:
                    if (cmbValue.SelectedValue == null) throw new InvalidOperationException("یک سطح تخفیف انتخاب کنید.");
                    record.Code = Convert.ToInt32(cmbValue.SelectedValue, CultureInfo.InvariantCulture); record.Weekday = cmbWeekday.SelectedIndex + 1;
                    record.Minimum = ParseLong(txtMinimum, "حداقل مبلغ"); record.Maximum = ParseLong(txtMaximum, "حداکثر مبلغ"); record.IsActive = chkActive.Checked;
                    break;
                case BusinessManagementArea.DiscountRates:
                    record.Id = ParseInt(txtCode, "شماره‌ی سطح"); record.Percent = ParseInt(txtPercent, "درصد تخفیف");
                    break;
                case BusinessManagementArea.Deposits:
                case BusinessManagementArea.RechargeLimit:
                case BusinessManagementArea.Tickets:
                    record.Amount = ParseLong(txtAmount, "مبلغ"); record.IsActive = chkActive.Checked;
                    break;
                case BusinessManagementArea.HourlyPlans:
                    record.Name = txtName.Text; record.Amount = ParseLong(txtAmount, "مبلغ شارژ ساعتی"); record.IsActive = chkActive.Checked;
                    break;
                case BusinessManagementArea.Schedules:
                    record.StartsAt = dtpStartsAt.Value; record.IsActive = chkActive.Checked;
                    break;
                case BusinessManagementArea.DevicePromotions:
                    if (cmbDevice.SelectedValue == null || cmbPromotion.SelectedValue == null) throw new InvalidOperationException("یک دستگاه و یک جشنواره انتخاب کنید.");
                    record.Extra1 = Convert.ToInt32(cmbDevice.SelectedValue, CultureInfo.InvariantCulture);
                    record.Extra2 = Convert.ToInt32(cmbPromotion.SelectedValue, CultureInfo.InvariantCulture);
                    record.IsActive = chkActive.Checked;
                    break;
            }
            return record;
        }

        private void SetInputsReadOnly()
        {
            foreach (TextBox control in new TextBox[] { txtCode, txtName, txtAmount, txtPercent, txtMinimum, txtMaximum, txtDuration })
                control.ReadOnly = true;
            foreach (Control control in new Control[] { dtpStartsAt, dtpEndsAt, cmbWeekday, cmbValue, cmbDevice, cmbPromotion })
                control.Enabled = false;
        }

        private static void Show(params Control[] controls)
        {
            foreach (Control control in controls) control.Visible = true;
        }

        private static DateTime SafeDate(DateTime value, DateTime fallback)
        {
            if (value == DateTime.MinValue || value < DateTimePicker.MinimumDateTime || value > DateTimePicker.MaximumDateTime) return fallback;
            return value;
        }

        private static long ParseLong(TextBox box, string fieldName)
        {
            string digits = NormalizeDigits(box.Text ?? String.Empty);
            digits = digits.Replace(",", String.Empty).Replace("٬", String.Empty).Replace(" ", String.Empty).Replace(" ", String.Empty);
            long value;
            if (!Int64.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out value))
                throw new ArgumentException("مقدار «" + fieldName + "» را فقط با رقم وارد کنید.");
            return value;
        }

        private static int ParseInt(TextBox box, string fieldName)
        {
            long value = ParseLong(box, fieldName);
            if (value > Int32.MaxValue) throw new ArgumentOutOfRangeException(fieldName, "مقدار «" + fieldName + "» بیش از اندازه‌ی مجاز است.");
            return (int)value;
        }

        private static string NormalizeDigits(string value)
        {
            char[] result = value.ToCharArray();
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] >= '۰' && result[i] <= '۹') result[i] = (char)('0' + result[i] - '۰');
                else if (result[i] >= '٠' && result[i] <= '٩') result[i] = (char)('0' + result[i] - '٠');
            }
            return new string(result);
        }

        private static string AreaTitle(BusinessManagementArea item)
        {
            switch (item)
            {
                case BusinessManagementArea.Attractions: return "دستگاه تفریحی";
                case BusinessManagementArea.ControllerDevices: return "دستگاه کنترل‌شونده";
                case BusinessManagementArea.Promotions: return "جشنواره";
                case BusinessManagementArea.DevicePromotions: return "تخصیص جشنواره";
                case BusinessManagementArea.DiscountPolicies: return "قانون تخفیف";
                case BusinessManagementArea.DiscountRates: return "درصد تخفیف";
                case BusinessManagementArea.Deposits: return "بیعانه";
                case BusinessManagementArea.RechargeLimit: return "سقف شارژ";
                case BusinessManagementArea.HourlyPlans: return "شارژ ساعتی";
                case BusinessManagementArea.Schedules: return "زمان شارژ";
                case BusinessManagementArea.Tickets: return "بلیت";
                default: return "اطلاعات";
            }
        }

        private void Cancel_Click(object sender, EventArgs e) { DialogResult = DialogResult.Cancel; Close(); }
    }
}
