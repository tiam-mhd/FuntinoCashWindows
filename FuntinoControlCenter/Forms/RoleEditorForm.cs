using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Properties;
using FuntinoControlCenter.Security;
using FuntinoControlCenter.UI;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;

namespace FuntinoControlCenter.Forms
{
    internal sealed partial class RoleEditorForm : Form
    {
        private const int PageSize = 16;
        private int roleId;
        private bool isCreate;
        private bool createdMetadataSaved;
        private readonly bool isSystem;
        private readonly bool canView;
        private readonly bool canEditMetadata;
        private readonly bool canAssignPermissions;
        private readonly bool canDelete;
        private readonly HashSet<string> selectedCodes;
        private readonly List<AccessPermissionDefinition> definitions;
        private bool busy;
        private int pageIndex;

        public RoleEditorForm()
            : this(0, string.Empty, string.Empty, false, true, true, true, false, new List<string>())
        {
        }

        public RoleEditorForm(int roleId, string roleName, string roleDescription, bool isSystem, bool canView,
            bool canEditMetadata, bool canAssignPermissions, bool canDelete, IEnumerable<string> selectedPermissionCodes)
        {
            AppFontManager.EnsureVazirmatnLoaded();
            InitializeComponent();
            this.roleId = roleId;
            isCreate = roleId == 0;
            this.isSystem = isSystem;
            this.canView = canView;
            this.canEditMetadata = canEditMetadata;
            this.canAssignPermissions = canAssignPermissions && !isSystem;
            this.canDelete = canDelete && !isSystem;
            selectedCodes = new HashSet<string>(selectedPermissionCodes ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            definitions = PermissionCatalog.All.OrderBy(item => item.DisplayOrder).Select(item =>
                new AccessPermissionDefinition(item.Code, item.Category, item.Name, item.Description)).ToList();

            txtRoleName.Text = roleName ?? string.Empty;
            txtRoleDescription.Text = roleDescription ?? string.Empty;
            txtRoleName.Visible = isCreate || this.canEditMetadata;
            lblRoleName.Visible = txtRoleName.Visible;
            txtRoleDescription.Visible = isCreate || this.canEditMetadata;
            lblRoleDescription.Visible = txtRoleDescription.Visible;
            metadataCard.Visible = isCreate || this.canEditMetadata || canView;
            lblRoleReadOnly.Visible = !isCreate && !this.canEditMetadata && canView;
            permissionsCard.Visible = canView || this.canAssignPermissions;
            permissionsGrid.ReadOnly = !this.canAssignPermissions;
            permissionsGrid.Columns["Granted"].ReadOnly = !this.canAssignPermissions;
            btnGrantResults.Visible = this.canAssignPermissions;
            btnClearResults.Visible = this.canAssignPermissions;
            btnDeleteRole.Visible = this.canDelete;
            btnSave.Visible = isCreate || this.canEditMetadata || this.canAssignPermissions;
            btnSave.Text = isCreate ? "ساخت نقش" : "ذخیره تغییرها";
            lblRoleEditorTitle.Text = isCreate ? "ساخت نقش تازه" : "مدیریت نقش و دسترسی‌ها";
            if (!isCreate) lblRoleEditorTitle.Text = "دسترسی‌های نقش «" + (roleName ?? string.Empty) + "»";
            lblRoleEditorSubtitle.Text = isSystem
                ? "این نقش پایه برای حفاظت از دسترسی‌های سامانه قابل تغییر یا حذف نیست."
                : "مشخصات نقش و مجوزهای دقیق آن را از همین صفحه تنظیم کنید.";
            lblRoleReadOnly.Text = "نقش: " + (roleName ?? string.Empty) + (String.IsNullOrWhiteSpace(roleDescription) ? string.Empty : "   |   " + roleDescription);
            ConfigurePermissionFilters();
            ApplyPermissionFilter();
        }

        private void ConfigurePermissionFilters()
        {
            cmbPermissionCategory.BeginUpdate();
            cmbPermissionCategory.Items.Clear();
            cmbPermissionCategory.Items.Add("همه بخش‌ها");
            foreach (string category in definitions.Select(item => item.Category).Distinct(StringComparer.CurrentCulture).OrderBy(value => value))
                cmbPermissionCategory.Items.Add(category);
            cmbPermissionCategory.SelectedIndex = 0;
            cmbPermissionCategory.EndUpdate();
        }

        private void ApplyPermissionFilter()
        {
            if (definitions == null) return;
            var filtered = definitions.AsEnumerable();
            string search = (txtPermissionSearch.Text ?? string.Empty).Trim();
            if (search.Length > 0)
                filtered = filtered.Where(item => item.Name.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                    item.Description.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                    item.Category.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0);
            if (cmbPermissionCategory.SelectedIndex > 0)
            {
                string category = Convert.ToString(cmbPermissionCategory.SelectedItem);
                filtered = filtered.Where(item => String.Equals(item.Category, category, StringComparison.CurrentCulture));
            }

            List<AccessPermissionDefinition> items = filtered.ToList();
            int pageCount = Math.Max(1, (int)Math.Ceiling(items.Count / (double)PageSize));
            pageIndex = Math.Max(0, Math.Min(pageIndex, pageCount - 1));
            var page = items.Skip(pageIndex * PageSize).Take(PageSize)
                .Select(item => new AccessPermissionRow(item.Code, item.Category, item.Name, item.Description, selectedCodes.Contains(item.Code)))
                .ToList();

            permissionsGrid.DataSource = new BindingList<AccessPermissionRow>(page);
            lblPermissionsEmpty.Visible = items.Count == 0;
            lblPermissionsEmpty.Text = "مجوزی با این جست‌وجو یا فیلتر پیدا نشد.";
            lblPermissionPage.Text = "صفحه " + (pageIndex + 1).ToString("N0") + " از " + pageCount.ToString("N0");
            lblFilteredCount.Text = items.Count.ToString("N0") + " مجوز";
            lblSelectedCount.Text = selectedCodes.Count.ToString("N0") + " مجوز انتخاب شده";
            btnPermissionPrevious.Enabled = pageIndex > 0;
            btnPermissionNext.Enabled = pageIndex + 1 < pageCount;
        }

        private void PermissionsGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (permissionsGrid.IsCurrentCellDirty && permissionsGrid.CurrentCell is DataGridViewCheckBoxCell)
                permissionsGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void PermissionsGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!canAssignPermissions || e.RowIndex < 0 || e.ColumnIndex < 0 || permissionsGrid.Columns[e.ColumnIndex].Name != "Granted") return;
            var item = permissionsGrid.Rows[e.RowIndex].DataBoundItem as AccessPermissionRow;
            if (item == null) return;
            if (item.Granted) selectedCodes.Add(item.Code);
            else selectedCodes.Remove(item.Code);
            lblSelectedCount.Text = selectedCodes.Count.ToString("N0") + " مجوز انتخاب شده";
        }

        private void GrantFiltered_Click(object sender, EventArgs e) { ChangeFilteredPermissions(true); }
        private void ClearFiltered_Click(object sender, EventArgs e) { ChangeFilteredPermissions(false); }

        private void ChangeFilteredPermissions(bool grant)
        {
            if (!canAssignPermissions || busy) return;
            foreach (AccessPermissionDefinition item in GetFilteredDefinitions())
            {
                if (grant) selectedCodes.Add(item.Code);
                else selectedCodes.Remove(item.Code);
            }
            ApplyPermissionFilter();
        }

        private IEnumerable<AccessPermissionDefinition> GetFilteredDefinitions()
        {
            IEnumerable<AccessPermissionDefinition> filtered = definitions;
            string search = (txtPermissionSearch.Text ?? string.Empty).Trim();
            if (search.Length > 0)
                filtered = filtered.Where(item => item.Name.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                    item.Description.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                    item.Category.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0);
            if (cmbPermissionCategory.SelectedIndex > 0)
            {
                string category = Convert.ToString(cmbPermissionCategory.SelectedItem);
                filtered = filtered.Where(item => String.Equals(item.Category, category, StringComparison.CurrentCulture));
            }
            return filtered;
        }

        private async void Save_Click(object sender, EventArgs e)
        {
            if (busy) return;
            string roleName = txtRoleName.Text.Trim();
            string roleDescription = txtRoleDescription.Text.Trim();
            List<string> permissionCodes = selectedCodes.ToList();
            if ((isCreate || canEditMetadata) && String.IsNullOrWhiteSpace(roleName))
            {
                MessageBox.Show(this, "نام نقش را وارد کنید.", "نام نقش لازم است", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtRoleName.Focus();
                return;
            }

            try
            {
                SetBusy(true, isCreate ? "در حال ساخت نقش…" : "در حال ذخیرهٔ نقش و دسترسی‌ها…", AccessManagementVisualAssets.Save);
                if (isCreate)
                {
                    AuthorizationGuard.Require("roles.create");
                    roleId = await Task.Run(() => AccessControlRepository.CreateRole(roleName, roleDescription));
                    isCreate = false;
                    createdMetadataSaved = true;
                    if (canAssignPermissions)
                    {
                        try
                        {
                            AuthorizationGuard.Require("roles.assign_permissions");
                            await Task.Run(() => AccessControlRepository.SetRolePermissions(roleId, permissionCodes));
                        }
                        catch (Exception ex)
                        {
                            lblOperationStatus.Text = "نقش ساخته شد. دسترسی‌ها هنوز ذخیره نشده‌اند؛ دوباره «ذخیره» را بزنید.";
                            lblOperationStatus.Visible = true;
                            MessageBox.Show(this, DatabaseManager.SafeMessage(ex) + Environment.NewLine + "نقش ساخته شده است؛ همین صفحه را برای تکمیل دسترسی‌ها باز نگه دارید.",
                                "ذخیرهٔ دسترسی‌ها انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }
                else
                {
                    if (canEditMetadata && !createdMetadataSaved)
                    {
                        AuthorizationGuard.Require("roles.update");
                        await Task.Run(() => AccessControlRepository.UpdateRoleMetadata(roleId, roleName, roleDescription));
                    }
                    if (canAssignPermissions)
                    {
                        AuthorizationGuard.Require("roles.assign_permissions");
                        await Task.Run(() => AccessControlRepository.SetRolePermissions(roleId, permissionCodes));
                    }
                }

                string message = canAssignPermissions
                    ? "تغییرهای نقش ذخیره شد. نشست‌های کاربران، مجوزهای تازه را پیش از عملیات بعدی دریافت می‌کنند."
                    : "تغییرهای نقش ذخیره شد.";
                MessageBox.Show(this, message, "ذخیره انجام شد", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "ذخیرهٔ نقش انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally { SetBusy(false, null, null); }
        }

        private async void DeleteRole_Click(object sender, EventArgs e)
        {
            if (busy || !canDelete || roleId == 0 || isSystem) return;
            if (MessageBox.Show(this, "این نقش از حساب‌های وابسته برداشته و حذف می‌شود. ادامه می‌دهید؟", "حذف نقش",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                AuthorizationGuard.Require("roles.delete");
                SetBusy(true, "در حال حذف نقش…", AccessManagementVisualAssets.Delete);
                await Task.Run(() => AccessControlRepository.DeleteRole(roleId));
                MessageBox.Show(this, "نقش حذف شد.", "حذف انجام شد", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "حذف نقش انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally { SetBusy(false, null, null); }
        }

        private void PermissionSearch_TextChanged(object sender, EventArgs e) { pageIndex = 0; ApplyPermissionFilter(); }
        private void PermissionCategory_SelectedIndexChanged(object sender, EventArgs e) { pageIndex = 0; ApplyPermissionFilter(); }
        private void PermissionPrevious_Click(object sender, EventArgs e) { pageIndex--; ApplyPermissionFilter(); }
        private void PermissionNext_Click(object sender, EventArgs e) { pageIndex++; ApplyPermissionFilter(); }
        private void Cancel_Click(object sender, EventArgs e) { DialogResult = DialogResult.Cancel; Close(); }

        private void SetBusy(bool value, string message, System.Drawing.Image icon)
        {
            busy = value;
            if (value) loadingIndicator.BeginLoading(message, icon);
            else loadingIndicator.EndLoading();
            btnSave.Enabled = !value;
            btnCancel.Enabled = !value;
            btnDeleteRole.Enabled = !value;
            btnGrantResults.Enabled = !value;
            btnClearResults.Enabled = !value;
            txtRoleName.Enabled = !value && canEditMetadata;
            txtRoleDescription.Enabled = !value && canEditMetadata;
            txtPermissionSearch.Enabled = !value;
            cmbPermissionCategory.Enabled = !value;
            permissionsGrid.Enabled = !value;
        }
    }

    internal sealed class AccessPermissionDefinition
    {
        public string Code { get; private set; }
        public string Category { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public AccessPermissionDefinition(string code, string category, string name, string description)
        {
            Code = code ?? string.Empty;
            Category = category ?? string.Empty;
            Name = name ?? string.Empty;
            Description = description ?? string.Empty;
        }
    }

    internal sealed class AccessPermissionRow : INotifyPropertyChanged
    {
        private bool granted;
        public string Code { get; private set; }
        public string Category { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool Granted
        {
            get { return granted; }
            set { if (granted == value) return; granted = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Granted")); }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public AccessPermissionRow(string code, string category, string name, string description, bool granted)
        {
            Code = code;
            Category = category;
            Name = name;
            Description = description;
            this.granted = granted;
        }
    }
}
