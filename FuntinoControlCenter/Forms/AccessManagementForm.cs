using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
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
    internal sealed partial class AccessManagementForm : Form
    {
        private const int DefaultPageSize = 20;
        private DataTable usersTable;
        private DataTable rolesTable;
        private bool loading;
        private bool usersBusy;
        private bool rolesBusy;
        private int usersPageIndex;
        private int rolesPageIndex;
        private int selectedUserId;
        private int selectedRoleId;
        private bool selectedUserIsActive;
        private bool selectedRoleIsSystem;

        public AccessManagementForm()
        {
            AppFontManager.EnsureVazirmatnLoaded();
            InitializeComponent();
        }

        private async void AccessManagementForm_Load(object sender, EventArgs e)
        {
            try
            {
                bool canSeeUsers = AuthorizationGuard.HasAny("users.view", "users.create", "users.update", "users.reset_password", "users.assign_roles", "users.deactivate");
                bool canSeeRoles = AuthorizationGuard.HasAny("roles.view", "roles.create", "roles.update", "roles.assign_permissions", "roles.delete");
                bool canBrowseUsers = AuthorizationGuard.HasAny("users.view", "users.update", "users.reset_password", "users.assign_roles", "users.deactivate");
                bool canBrowseRoles = AuthorizationGuard.HasAny("roles.view", "roles.update", "roles.assign_permissions", "roles.delete");
                tabs.TabPages.Clear();
                if (canSeeUsers) tabs.TabPages.Add(usersPage);
                if (canSeeRoles) tabs.TabPages.Add(rolesPage);
                if (tabs.TabPages.Count == 0)
                {
                    MessageBox.Show(this, "برای مدیریت حساب‌ها یا نقش‌ها مجوز لازم را ندارید.", "دسترسی مجاز نیست", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Close();
                    return;
                }

                AuthorizationMonitor.Attach(this, delegate
                {
                    return AuthorizationGuard.HasAny("users.view", "users.create", "users.update", "users.reset_password", "users.assign_roles", "users.deactivate", "roles.view", "roles.create", "roles.update", "roles.assign_permissions", "roles.delete");
                });

                btnCreateUser.Visible = AuthorizationGuard.Has("users.create");
                cmbUserStatus.Visible = AuthorizationGuard.Has("users.view");
                btnCreateRole.Visible = AuthorizationGuard.Has("roles.create");
                lblUserSearch.Visible = canBrowseUsers;
                userSearchHost.Visible = canBrowseUsers;
                usersGridHost.Visible = canBrowseUsers;
                usersFooter.Visible = canBrowseUsers;
                userDetailsHost.Visible = canBrowseUsers;
                lblRoleSearch.Visible = canBrowseRoles;
                roleSearchHost.Visible = canBrowseRoles;
                cmbRoleType.Visible = canBrowseRoles;
                rolesContentLayout.Visible = canBrowseRoles;
                rolesFooter.Visible = canBrowseRoles;

                var loads = new List<Task>();
                if (canSeeUsers && AuthorizationGuard.HasAny("users.view", "users.update", "users.reset_password", "users.assign_roles", "users.deactivate"))
                    loads.Add(LoadUsersAsync());
                if (canSeeRoles && AuthorizationGuard.HasAny("roles.view", "roles.update", "roles.assign_permissions", "roles.delete"))
                    loads.Add(LoadRolesAsync());
                await Task.WhenAll(loads);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "باز کردن مدیریت دسترسی‌ها انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private async Task LoadUsersAsync()
        {
            if (usersBusy) return;
            usersBusy = true;
            loading = true;
            usersGrid.Enabled = false;
            btnCreateUser.Enabled = false;
            usersLoading.BeginLoading("در حال دریافت فهرست حساب‌ها…", AccessManagementVisualAssets.UserAdd);
            try
            {
                usersTable = await Task.Run(() => AccessControlRepository.GetUsers());
                usersPageIndex = 0;
                ApplyUserFilter();
            }
            catch (Exception ex)
            {
                usersTable = null;
                usersGrid.DataSource = null;
                lblUsersEmpty.Text = "فهرست حساب‌ها بارگذاری نشد. دوباره تلاش کنید.";
                lblUsersEmpty.Visible = true;
                MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "دریافت فهرست حساب‌ها انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                usersLoading.EndLoading();
                usersGrid.Enabled = true;
                btnCreateUser.Enabled = AuthorizationGuard.Has("users.create");
                usersBusy = false;
                loading = false;
            }
        }

        private async Task LoadRolesAsync()
        {
            if (rolesBusy) return;
            rolesBusy = true;
            loading = true;
            rolesGrid.Enabled = false;
            btnCreateRole.Enabled = false;
            rolesLoading.BeginLoading("در حال دریافت فهرست نقش‌ها…", AccessManagementVisualAssets.Role);
            try
            {
                rolesTable = await Task.Run(() => AccessControlRepository.GetRoles());
                rolesPageIndex = 0;
                ApplyRoleFilter();
            }
            catch (Exception ex)
            {
                rolesTable = null;
                rolesGrid.DataSource = null;
                lblRolesEmpty.Text = "فهرست نقش‌ها بارگذاری نشد. دوباره تلاش کنید.";
                lblRolesEmpty.Visible = true;
                MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "دریافت فهرست نقش‌ها انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                rolesLoading.EndLoading();
                rolesGrid.Enabled = true;
                btnCreateRole.Enabled = AuthorizationGuard.Has("roles.create");
                rolesBusy = false;
                loading = false;
            }
        }

        private void ApplyUserFilter()
        {
            if (usersTable == null) return;
            IEnumerable<DataRow> rows = usersTable.Rows.Cast<DataRow>();
            string search = (txtUserSearch.Text ?? string.Empty).Trim();
            if (search.Length > 0)
            {
                rows = rows.Where(row => Contains(row, "UserName", search) || Contains(row, "DisplayName", search) || Contains(row, "RoleNames", search));
            }
            if (AuthorizationGuard.Has("users.view") && cmbUserStatus.SelectedIndex == 1)
                rows = rows.Where(row => !row.IsNull("IsActive") && Convert.ToBoolean(row["IsActive"]));
            else if (AuthorizationGuard.Has("users.view") && cmbUserStatus.SelectedIndex == 2)
                rows = rows.Where(row => !row.IsNull("IsActive") && !Convert.ToBoolean(row["IsActive"]));

            List<DataRow> filtered = rows.ToList();
            int pageSize = SelectedPageSize(cmbUserPageSize, DefaultPageSize);
            int pageCount = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)pageSize));
            usersPageIndex = Math.Max(0, Math.Min(usersPageIndex, pageCount - 1));
            DataTable page = usersTable.Clone();
            foreach (DataRow row in filtered.Skip(usersPageIndex * pageSize).Take(pageSize)) page.ImportRow(row);

            loading = true;
            usersGrid.DataSource = page;
            ConfigureUsersGrid();
            usersGrid.ClearSelection();
            selectedUserId = 0;
            lblUsersEmpty.Visible = filtered.Count == 0;
            if (filtered.Count == 0)
                lblUsersEmpty.Text = usersTable.Rows.Count == 0 ? "هنوز حسابی ساخته نشده است." : "حسابی با این جست‌وجو یا فیلتر پیدا نشد.";
            lblUserCount.Text = filtered.Count.ToString("N0") + " حساب";
            lblUsersPage.Text = "صفحه " + (usersPageIndex + 1).ToString("N0") + " از " + pageCount.ToString("N0");
            btnUsersPrevious.Enabled = usersPageIndex > 0;
            btnUsersNext.Enabled = usersPageIndex + 1 < pageCount;
            SetUserDetails(null);
            loading = false;
        }

        private void ConfigureUsersGrid()
        {
            HideColumn(usersGrid, "ApplicationUserId", "AuthorizationVersion");
            SetHeader(usersGrid, "UserName", "نام ورود");
            SetHeader(usersGrid, "DisplayName", "نام نمایشی");
            SetHeader(usersGrid, "IsActive", "وضعیت حساب");
            SetHeader(usersGrid, "MustChangePassword", "تغییر گذرواژه در ورود بعدی");
            SetHeader(usersGrid, "CreatedAt", "تاریخ ساخت");
            SetHeader(usersGrid, "LastLoginAt", "آخرین ورود");
            SetHeader(usersGrid, "RoleNames", "نقش‌ها");
            if (usersGrid.Columns.Contains("IsActive")) usersGrid.Columns["IsActive"].FillWeight = 65;
            if (usersGrid.Columns.Contains("UserName")) usersGrid.Columns["UserName"].FillWeight = 90;
            if (usersGrid.Columns.Contains("DisplayName")) usersGrid.Columns["DisplayName"].FillWeight = 110;
            if (usersGrid.Columns.Contains("RoleNames")) usersGrid.Columns["RoleNames"].FillWeight = 130;
            if (usersGrid.Columns.Contains("CreatedAt")) usersGrid.Columns["CreatedAt"].FillWeight = 95;
            if (usersGrid.Columns.Contains("LastLoginAt")) usersGrid.Columns["LastLoginAt"].FillWeight = 95;
        }

        private void UsersGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            string column = usersGrid.Columns[e.ColumnIndex].Name;
            if (column == "MustChangePassword" && e.Value != null && e.Value != DBNull.Value)
            {
                e.Value = Convert.ToBoolean(e.Value);
                e.FormattingApplied = true;
            }
            else if ((column == "CreatedAt" || column == "LastLoginAt") && e.Value is DateTime)
            {
                e.Value = ((DateTime)e.Value).ToString("yyyy/MM/dd  HH:mm");
                e.FormattingApplied = true;
            }
        }

        private void UsersGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (loading || usersGrid.CurrentRow == null || usersGrid.CurrentRow.DataBoundItem == null) return;
            var row = usersGrid.CurrentRow.DataBoundItem as DataRowView;
            if (row == null) return;
            try
            {
                selectedUserId = Convert.ToInt32(row["ApplicationUserId"]);
                selectedUserIsActive = Convert.ToBoolean(row["IsActive"]);
                SetUserDetails(row.Row);
            }
            catch (Exception ex)
            {
                selectedUserId = 0;
                SetUserDetails(null);
                MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "نمایش حساب انتخاب‌شده انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetUserDetails(DataRow row)
        {
            bool hasSelection = row != null && selectedUserId != 0;
            userDetailsPanel.Visible = hasSelection;
            userDetailsEmpty.Visible = !hasSelection;
            btnEditUser.Visible = hasSelection && AuthorizationGuard.HasAny("users.update", "users.assign_roles", "users.reset_password", "users.deactivate");
            if (!hasSelection) return;

            string userName = Convert.ToString(row["UserName"]);
            string displayName = row.Table.Columns.Contains("DisplayName") ? Convert.ToString(row["DisplayName"]) : string.Empty;
            bool mustChange = row.Table.Columns.Contains("MustChangePassword") && !row.IsNull("MustChangePassword") && Convert.ToBoolean(row["MustChangePassword"]);
            string roles = row.Table.Columns.Contains("RoleNames") ? Convert.ToString(row["RoleNames"]) : "";
            lblSelectedUserTitle.Text = String.IsNullOrWhiteSpace(displayName) ? userName : displayName;
            lblSelectedUserSummary.Text = "نام ورود: " + userName + "   |   " + (selectedUserIsActive ? "حساب فعال" : "حساب غیرفعال") +
                (mustChange ? "   |   تغییر گذرواژه در ورود بعدی لازم است" : "");
            lblSelectedUserRoles.Text = roles.Length == 0 ? "نقش‌ها: اطلاعاتی برای نمایش در دسترس نیست." : "نقش‌ها: " + roles;
        }

        private void ApplyRoleFilter()
        {
            if (rolesTable == null) return;
            IEnumerable<DataRow> rows = rolesTable.Rows.Cast<DataRow>();
            string search = (txtRoleSearch.Text ?? string.Empty).Trim();
            if (search.Length > 0)
                rows = rows.Where(row => Contains(row, "DisplayName", search) || Contains(row, "Description", search));
            if (cmbRoleType.SelectedIndex == 1)
                rows = rows.Where(row => Convert.ToBoolean(row["IsSystem"]));
            else if (cmbRoleType.SelectedIndex == 2)
                rows = rows.Where(row => !Convert.ToBoolean(row["IsSystem"]));

            List<DataRow> filtered = rows.ToList();
            const int pageSize = DefaultPageSize;
            int pageCount = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)pageSize));
            rolesPageIndex = Math.Max(0, Math.Min(rolesPageIndex, pageCount - 1));
            DataTable page = rolesTable.Clone();
            foreach (DataRow row in filtered.Skip(rolesPageIndex * pageSize).Take(pageSize)) page.ImportRow(row);

            loading = true;
            rolesGrid.DataSource = page;
            HideColumn(rolesGrid, "RoleId");
            SetHeader(rolesGrid, "DisplayName", "نام نقش");
            SetHeader(rolesGrid, "IsSystem", "نقش پایه");
            SetHeader(rolesGrid, "Description", "توضیح کوتاه");
            rolesGrid.ClearSelection();
            selectedRoleId = 0;
            lblRolesEmpty.Visible = filtered.Count == 0;
            if (filtered.Count == 0)
                lblRolesEmpty.Text = rolesTable.Rows.Count == 0 ? "هنوز نقشی ساخته نشده است." : "نقشی با این جست‌وجو یا فیلتر پیدا نشد.";
            lblRoleCount.Text = filtered.Count.ToString("N0") + " نقش";
            lblRolesPage.Text = "صفحه " + (rolesPageIndex + 1).ToString("N0") + " از " + pageCount.ToString("N0");
            btnRolesPrevious.Enabled = rolesPageIndex > 0;
            btnRolesNext.Enabled = rolesPageIndex + 1 < pageCount;
            SetRoleDetails(null, null);
            loading = false;
        }

        private async void RolesGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (loading || rolesBusy || rolesGrid.CurrentRow == null || rolesGrid.CurrentRow.DataBoundItem == null) return;
            var row = rolesGrid.CurrentRow.DataBoundItem as DataRowView;
            if (row == null) return;
            try
            {
                selectedRoleId = Convert.ToInt32(row["RoleId"]);
                int requestedRoleId = selectedRoleId;
                selectedRoleIsSystem = Convert.ToBoolean(row["IsSystem"]);
                SetRoleDetails(row.Row, null);
                if (!AuthorizationGuard.HasAny("roles.view", "roles.assign_permissions")) return;
                rolesBusy = true;
                rolesLoading.BeginLoading("در حال دریافت دسترسی‌های این نقش…", AccessManagementVisualAssets.Permission);
                rolesGrid.Enabled = false;
                List<string> codes = await Task.Run(() => AccessControlRepository.GetRolePermissionCodes(requestedRoleId));
                if (selectedRoleId != requestedRoleId) return;
                SetRoleDetails(row.Row, codes);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "نمایش دسترسی‌های نقش انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                rolesLoading.EndLoading();
                rolesGrid.Enabled = true;
                rolesBusy = false;
            }
        }

        private void SetRoleDetails(DataRow row, IList<string> permissionCodes)
        {
            bool hasSelection = row != null && selectedRoleId != 0;
            roleDetailsPanel.Visible = hasSelection;
            roleDetailsEmpty.Visible = !hasSelection;
            btnEditRole.Visible = hasSelection && AuthorizationGuard.HasAny("roles.view", "roles.update", "roles.assign_permissions", "roles.delete");
            if (!hasSelection) return;

            string name = Convert.ToString(row["DisplayName"]);
            string description = row.Table.Columns.Contains("Description") ? Convert.ToString(row["Description"]) : "";
            lblSelectedRoleTitle.Text = name;
            lblSelectedRoleKind.Text = selectedRoleIsSystem ? "نقش پایهٔ سامانه · محافظت‌شده" : "نقش اختصاصی";
            lblSelectedRoleDescription.Text = String.IsNullOrWhiteSpace(description) ? "برای این نقش توضیحی ثبت نشده است." : description;
            lstRolePermissions.Items.Clear();
            bool canReadPermissions = AuthorizationGuard.HasAny("roles.view", "roles.assign_permissions");
            lstRolePermissions.Visible = canReadPermissions;
            lblRolePermissionCount.Visible = canReadPermissions;
            if (!canReadPermissions) return;
            if (permissionCodes == null)
            {
                lblRolePermissionCount.Text = "در حال آماده‌سازی فهرست دسترسی‌ها…";
                return;
            }
            var selected = new HashSet<string>(permissionCodes, StringComparer.OrdinalIgnoreCase);
            var definitions = PermissionCatalog.All.Where(p => selected.Contains(p.Code)).OrderBy(p => p.DisplayOrder).ToList();
            lblRolePermissionCount.Text = definitions.Count.ToString("N0") + " دسترسی برای این نقش";
            if (definitions.Count == 0) lstRolePermissions.Items.Add("برای این نقش دسترسی‌ای ثبت نشده است.");
            foreach (var permission in definitions)
                lstRolePermissions.Items.Add(permission.Category + "  ·  " + permission.Name);
        }

        private async void CreateUser_Click(object sender, EventArgs e) { await OpenUserEditorAsync(true); }
        private async void EditUser_Click(object sender, EventArgs e) { await OpenUserEditorAsync(false); }
        private async void CreateRole_Click(object sender, EventArgs e) { await OpenRoleEditorAsync(true); }
        private async void EditRole_Click(object sender, EventArgs e) { await OpenRoleEditorAsync(false); }

        private async Task OpenUserEditorAsync(bool create)
        {
            if (!create && selectedUserId == 0) return;
            bool canAssign = AuthorizationGuard.Has("users.assign_roles");
            if (create) AuthorizationGuard.Require("users.create");
            else AuthorizationGuard.RequireAny("users.update", "users.assign_roles", "users.reset_password", "users.deactivate");

            int userId = create ? 0 : selectedUserId;
            string userName = create ? "" : GetSelectedUserValue("UserName");
            string displayName = create ? "" : GetSelectedUserValue("DisplayName");
            bool active = create || selectedUserIsActive;
            bool isCurrentUser = !create && AppSession.Current != null && userId == AppSession.Current.ApplicationUserId;
            SetUsersOperationBusy(true, "در حال آماده‌کردن صفحهٔ حساب…", AccessManagementVisualAssets.UserEdit);
            try
            {
                var roleOptions = new List<AccessRoleOption>();
                List<int> selectedRoles = new List<int>();
                if (canAssign)
                {
                    var data = await Task.Run(() => AccessControlRepository.GetAssignableRoles());
                    foreach (DataRow row in data.Rows)
                        roleOptions.Add(new AccessRoleOption(Convert.ToInt32(row["RoleId"]), Convert.ToString(row["DisplayName"])));
                    if (!create) selectedRoles = await Task.Run(() => AccessControlRepository.GetUserRoleIds(userId));
                }

                bool mustChange = !create && GetSelectedUserBoolean("MustChangePassword");
                using (var dialog = new UserEditorForm(userId, userName, displayName, active, mustChange,
                    create, create || AuthorizationGuard.Has("users.update"), canAssign,
                    AuthorizationGuard.Has("users.reset_password") && !isCurrentUser,
                    AuthorizationGuard.Has("users.deactivate") && !isCurrentUser, roleOptions, selectedRoles))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK) await LoadUsersAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "باز کردن صفحهٔ حساب انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally { SetUsersOperationBusy(false, null, null); }
        }

        private async Task OpenRoleEditorAsync(bool create)
        {
            if (!create && selectedRoleId == 0) return;
            if (create) AuthorizationGuard.Require("roles.create");
            else AuthorizationGuard.RequireAny("roles.view", "roles.update", "roles.assign_permissions", "roles.delete");

            int roleId = create ? 0 : selectedRoleId;
            DataRow row = create ? null : FindRoleRow(roleId);
            if (!create && row == null) return;
            string name = create ? "" : Convert.ToString(row["DisplayName"]);
            string description = !create && row.Table.Columns.Contains("Description") ? Convert.ToString(row["Description"]) : "";
            bool isSystem = !create && Convert.ToBoolean(row["IsSystem"]);
            bool canView = AuthorizationGuard.Has("roles.view");
            bool canEditMetadata = !isSystem && (create ? AuthorizationGuard.Has("roles.create") : AuthorizationGuard.Has("roles.update"));
            bool canAssign = !isSystem && AuthorizationGuard.Has("roles.assign_permissions");
            bool canDelete = !create && !isSystem && AuthorizationGuard.Has("roles.delete");

            SetRolesOperationBusy(true, "در حال آماده‌کردن صفحهٔ نقش…", AccessManagementVisualAssets.Permission);
            try
            {
                List<string> codes = new List<string>();
                if (!create && AuthorizationGuard.HasAny("roles.view", "roles.assign_permissions"))
                    codes = await Task.Run(() => AccessControlRepository.GetRolePermissionCodes(roleId));

                using (var dialog = new RoleEditorForm(roleId, name, description, isSystem, canView, canEditMetadata,
                    canAssign, canDelete, codes))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK) await LoadRolesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "باز کردن صفحهٔ نقش انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally { SetRolesOperationBusy(false, null, null); }
        }

        private void SetUsersOperationBusy(bool busy, string message, Image icon)
        {
            if (busy) usersLoading.BeginLoading(message, icon);
            else usersLoading.EndLoading();
            txtUserSearch.Enabled = !busy;
            cmbUserStatus.Enabled = !busy;
            btnCreateUser.Enabled = !busy && AuthorizationGuard.Has("users.create");
            usersGrid.Enabled = !busy;
            btnEditUser.Enabled = !busy;
        }

        private void SetRolesOperationBusy(bool busy, string message, Image icon)
        {
            if (busy) rolesLoading.BeginLoading(message, icon);
            else rolesLoading.EndLoading();
            txtRoleSearch.Enabled = !busy;
            cmbRoleType.Enabled = !busy;
            btnCreateRole.Enabled = !busy && AuthorizationGuard.Has("roles.create");
            rolesGrid.Enabled = !busy;
            btnEditRole.Enabled = !busy;
        }

        private string GetSelectedUserValue(string column)
        {
            if (usersGrid.CurrentRow == null || !usersGrid.Columns.Contains(column)) return string.Empty;
            return Convert.ToString(usersGrid.CurrentRow.Cells[column].Value);
        }

        private bool GetSelectedUserBoolean(string column)
        {
            if (usersGrid.CurrentRow == null || !usersGrid.Columns.Contains(column)) return false;
            object value = usersGrid.CurrentRow.Cells[column].Value;
            return value != null && value != DBNull.Value && Convert.ToBoolean(value);
        }

        private DataRow FindRoleRow(int roleId)
        {
            if (rolesTable == null) return null;
            foreach (DataRow row in rolesTable.Rows)
                if (Convert.ToInt32(row["RoleId"]) == roleId) return row;
            return null;
        }

        private void UserSearch_TextChanged(object sender, EventArgs e) { usersPageIndex = 0; ApplyUserFilter(); }
        private void UserStatus_SelectedIndexChanged(object sender, EventArgs e) { usersPageIndex = 0; ApplyUserFilter(); }
        private void UserPageSize_SelectedIndexChanged(object sender, EventArgs e) { usersPageIndex = 0; ApplyUserFilter(); }
        private void UsersPrevious_Click(object sender, EventArgs e) { usersPageIndex--; ApplyUserFilter(); }
        private void UsersNext_Click(object sender, EventArgs e) { usersPageIndex++; ApplyUserFilter(); }
        private void RoleSearch_TextChanged(object sender, EventArgs e) { rolesPageIndex = 0; ApplyRoleFilter(); }
        private void RoleType_SelectedIndexChanged(object sender, EventArgs e) { rolesPageIndex = 0; ApplyRoleFilter(); }
        private void RolesPrevious_Click(object sender, EventArgs e) { rolesPageIndex--; ApplyRoleFilter(); }
        private void RolesNext_Click(object sender, EventArgs e) { rolesPageIndex++; ApplyRoleFilter(); }
        private void Close_Click(object sender, EventArgs e) { Close(); }

        private static int SelectedPageSize(ComboBox combo, int fallback)
        {
            int value;
            return combo != null && Int32.TryParse(Convert.ToString(combo.SelectedItem), out value) && value > 0 ? value : fallback;
        }

        private static bool Contains(DataRow row, string column, string search)
        {
            return row.Table.Columns.Contains(column) && !row.IsNull(column) &&
                Convert.ToString(row[column]).IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private static void HideColumn(DataGridView grid, params string[] names)
        {
            foreach (string name in names)
                if (grid.Columns.Contains(name)) grid.Columns[name].Visible = false;
        }

        private static void SetHeader(DataGridView grid, string name, string header)
        {
            if (grid.Columns.Contains(name)) grid.Columns[name].HeaderText = header;
        }
    }

    internal sealed class AccessRoleOption
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public AccessRoleOption(int id, string name)
        {
            Id = id;
            Name = name ?? string.Empty;
        }

        public override string ToString() { return Name; }
    }
}
