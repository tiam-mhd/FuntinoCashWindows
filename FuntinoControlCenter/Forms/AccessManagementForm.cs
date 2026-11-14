using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Security;

namespace FuntinoControlCenter.Forms
{
    internal sealed partial class AccessManagementForm : Form
    {
        private readonly Dictionary<string, CheckedListBox> permissionLists = new Dictionary<string, CheckedListBox>(StringComparer.OrdinalIgnoreCase);
        private readonly List<RoleChoice> assignableRoles = new List<RoleChoice>();
        private DataTable rolesTable;
        private bool loading;
        private int selectedUserId;
        private int selectedRoleId;
        private bool selectedUserIsActive;

        public AccessManagementForm()
        {
            InitializeComponent();
            InitializePermissionLists();
        }

        private void InitializePermissionLists()
        {
            permissionLists.Add("داشبورد", clbDashboardPermissions);
            permissionLists.Add("گزارش‌ها", clbReportPermissions);
            permissionLists.Add("دستگاه‌ها", clbDevicePermissions);
            permissionLists.Add("تنظیمات", clbSettingsPermissions);
            permissionLists.Add("کاربران", clbUserPermissions);
            permissionLists.Add("نقش‌ها", clbRolePermissions);
            permissionLists.Add("تراکنش‌های مالی", clbFinancePermissions);
            permissionLists.Add("قیمت‌گذاری و تعرفه", clbPricingPermissions);
            permissionLists.Add("پرداخت و پایانه", clbPaymentsPermissions);
            permissionLists.Add("بلیت و ورود", clbTicketPermissions);
            permissionLists.Add("اطلاعات پایه", clbReferencePermissions);
            permissionLists.Add("داده‌های دستگاه‌ها", clbDeviceDataPermissions);
            permissionLists.Add("امنیت و حساب‌های قدیمی", clbSecurityDataPermissions);
            permissionLists.Add("پیکربندی", clbConfigurationPermissions);
            permissionLists.Add("مدیریت کسب‌وکار", clbBusinessPermissions);

            foreach (var group in PermissionCatalog.All.GroupBy(p => p.Category))
            {
                CheckedListBox list;
                if (!permissionLists.TryGetValue(group.Key, out list))
                    throw new InvalidOperationException("گروه مجوز «" + group.Key + "» در فرم طراحی نشده است.");
                foreach (var permission in group.OrderBy(p => p.DisplayOrder))
                    list.Items.Add(new PermissionChoice(permission.Code, permission.Name + " — " + permission.Description), false);
            }
        }

        private void UsersGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.ColumnIndex >= usersGrid.Columns.Count) return;
            string columnName = usersGrid.Columns[e.ColumnIndex].Name;
            if ((columnName == "IsActive" || columnName == "MustChangePassword") && e.Value != null && e.Value != DBNull.Value)
            {
                bool value = Convert.ToBoolean(e.Value);
                if (usersGrid.Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
                    e.Value = value;
                else
                    e.Value = value ? "بله" : "خیر";
                e.FormattingApplied = true;
            }
        }

        private void AccessManagementForm_Load(object sender, EventArgs e)
        {
            try
            {
                bool canSeeUsers = AuthorizationGuard.HasAny("users.view", "users.create", "users.update", "users.reset_password", "users.assign_roles", "users.deactivate");
                bool canSeeRoles = AuthorizationGuard.HasAny("roles.view", "roles.create", "roles.update", "roles.assign_permissions", "roles.delete");
                tabs.TabPages.Clear();
                if (canSeeUsers) tabs.TabPages.Add(usersPage);
                if (canSeeRoles) tabs.TabPages.Add(rolesPage);
                if (tabs.TabPages.Count == 0)
                {
                    MessageBox.Show(this, "برای مدیریت حساب‌ها یا نقش‌ها مجوز ندارید.", "دسترسی مجاز نیست", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Close();
                    return;
                }
                AuthorizationMonitor.Attach(this, delegate
                {
                    return AuthorizationGuard.HasAny("users.view", "users.create", "users.update", "users.reset_password", "users.assign_roles", "users.deactivate", "roles.view", "roles.create", "roles.update", "roles.assign_permissions", "roles.delete");
                });
                if (canSeeUsers)
                {
                    createGroup.Visible = AuthorizationGuard.Has("users.create");
                    selectedGroup.Visible = AuthorizationGuard.HasAny("users.update", "users.assign_roles", "users.reset_password", "users.deactivate");
                    usersGrid.Visible = AuthorizationGuard.HasAny("users.view", "users.update", "users.reset_password", "users.assign_roles", "users.deactivate");
                    if (AuthorizationGuard.Has("users.view") || AuthorizationGuard.HasAny("users.update", "users.reset_password", "users.assign_roles", "users.deactivate"))
                        LoadUsers();
                    bool canAssign = AuthorizationGuard.Has("users.assign_roles");
                    clbNewUserRoles.Visible = canAssign;
                    clbUserRoles.Visible = canAssign;
                    SetNamedControlVisible(clbNewUserRoles.Parent, "lblNewUserRoles", canAssign);
                    SetNamedControlVisible(clbUserRoles.Parent, "lblSelectedUserRoles", canAssign);
                    if (canAssign) LoadAssignableRoles();
                    btnCreateUser.Visible = AuthorizationGuard.Has("users.create");
                    btnSaveDisplayName.Visible = AuthorizationGuard.Has("users.update");
                    btnAssignUserRoles.Visible = canAssign;
                    btnResetPassword.Visible = AuthorizationGuard.Has("users.reset_password");
                    btnToggleUser.Visible = AuthorizationGuard.Has("users.deactivate");
                    bool showDisplayName = AuthorizationGuard.Has("users.update");
                    txtDisplayName.Visible = showDisplayName;
                    SetNamedControlVisible(txtDisplayName.Parent, "lblSelectedDisplayName", showDisplayName);
                }
                btnCreateRole.Visible = AuthorizationGuard.Has("roles.create");
                roleMetadata.Visible = AuthorizationGuard.Has("roles.create");
                rolesList.Visible = canSeeRoles && AuthorizationGuard.HasAny("roles.view", "roles.update", "roles.assign_permissions", "roles.delete");
                if (canSeeRoles && AuthorizationGuard.HasAny("roles.view", "roles.update", "roles.assign_permissions", "roles.delete"))
                    LoadRoles();
                permissionPanel.Visible = false;
                assignedPermissions.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "مدیریت دسترسی‌ها", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void LoadUsers()
        {
            loading = true;
            usersGrid.DataSource = AccessControlRepository.GetUsers();
            HideColumn(usersGrid, "ApplicationUserId", "AuthorizationVersion");
            SetHeader(usersGrid, "UserName", "نام ورود");
            SetHeader(usersGrid, "DisplayName", "نام نمایشی");
            SetHeader(usersGrid, "IsActive", "فعال");
            SetHeader(usersGrid, "MustChangePassword", "تغییر گذرواژه لازم است");
            SetHeader(usersGrid, "CreatedAt", "تاریخ ساخت");
            SetHeader(usersGrid, "LastLoginAt", "آخرین ورود");
            SetHeader(usersGrid, "RoleNames", "نقش‌ها");
            usersGrid.ClearSelection();
            selectedUserId = 0;
            txtDisplayName.Clear();
            clbUserRoles.Items.Clear();
            lblUserSelection.Text = "برای مدیریت، یک حساب را از فهرست انتخاب کنید.";
            btnToggleUser.Visible = btnResetPassword.Visible = btnAssignUserRoles.Visible = btnSaveDisplayName.Visible = false;
            loading = false;
        }

        private void LoadAssignableRoles()
        {
            assignableRoles.Clear();
            DataTable table = AccessControlRepository.GetAssignableRoles();
            foreach (DataRow row in table.Rows)
                assignableRoles.Add(new RoleChoice(Convert.ToInt32(row["RoleId"]), Convert.ToString(row["DisplayName"]), false));
            FillRoleChecklist(clbNewUserRoles, new int[0]);
            FillRoleChecklist(clbUserRoles, new int[0]);
        }

        private void FillRoleChecklist(CheckedListBox list, IEnumerable<int> selected)
        {
            var ids = new HashSet<int>(selected ?? Enumerable.Empty<int>());
            list.Items.Clear();
            foreach (RoleChoice role in assignableRoles)
                list.Items.Add(role, ids.Contains(role.Id));
        }

        private void UsersGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (loading || usersGrid.CurrentRow == null || !usersGrid.Columns.Contains("ApplicationUserId")) return;
            try
            {
                var row = usersGrid.CurrentRow;
                selectedUserId = Convert.ToInt32(row.Cells["ApplicationUserId"].Value);
                selectedUserIsActive = Convert.ToBoolean(row.Cells["IsActive"].Value);
                string name = Convert.ToString(row.Cells["UserName"].Value);
                string display = usersGrid.Columns.Contains("DisplayName") ? Convert.ToString(row.Cells["DisplayName"].Value) : "";
                bool mustChange = usersGrid.Columns.Contains("MustChangePassword") && Convert.ToBoolean(row.Cells["MustChangePassword"].Value);
                lblUserSelection.Text = "حساب انتخاب‌شده: " + name + (mustChange ? "  |  گذرواژه‌ی موقت باید تغییر کند" : "");
                txtDisplayName.Text = display;
                btnToggleUser.Text = selectedUserIsActive ? "غیرفعال‌کردن حساب" : "فعال‌کردن حساب";
                bool canChangeOtherUser = AppSession.Current != null && selectedUserId != AppSession.Current.ApplicationUserId;
                btnToggleUser.Visible = AuthorizationGuard.Has("users.deactivate") && canChangeOtherUser;
                btnResetPassword.Visible = AuthorizationGuard.Has("users.reset_password") && canChangeOtherUser;
                btnSaveDisplayName.Visible = AuthorizationGuard.Has("users.update");
                btnAssignUserRoles.Visible = AuthorizationGuard.Has("users.assign_roles");
                if (AuthorizationGuard.Has("users.assign_roles"))
                    FillRoleChecklist(clbUserRoles, AccessControlRepository.GetUserRoleIds(selectedUserId));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "انتخاب کاربر", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CreateUser_Click(object sender, EventArgs e)
        {
            try
            {
                var roles = AuthorizationGuard.Has("users.assign_roles") ? CheckedRoleIds(clbNewUserRoles) : new List<int>();
                GeneratedCredential credential = AccessControlRepository.CreateUser(txtNewUserName.Text, txtNewDisplayName.Text, roles);
                LoadUsersIfPermitted();
                using (var dialog = new InitialCredentialDialog(credential.UserName, credential.Password)) dialog.ShowDialog(this);
                txtNewUserName.Clear();
                txtNewDisplayName.Clear();
                FillRoleChecklist(clbNewUserRoles, new int[0]);
            }
            catch (Exception ex) { MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "ساخت کاربر انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void SaveUserDisplayName_Click(object sender, EventArgs e)
        {
            try { AccessControlRepository.UpdateUserDisplayName(selectedUserId, txtDisplayName.Text); LoadUsersIfPermitted(); }
            catch (Exception ex) { MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "ذخیره‌ی نام انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void AssignUserRoles_Click(object sender, EventArgs e)
        {
            try { AccessControlRepository.SetUserRoles(selectedUserId, CheckedRoleIds(clbUserRoles)); LoadUsersIfPermitted(); }
            catch (Exception ex) { MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "تخصیص نقش انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void ResetPassword_Click(object sender, EventArgs e)
        {
            try
            {
                string userName = Convert.ToString(usersGrid.CurrentRow.Cells["UserName"].Value);
                string password = AccessControlRepository.ResetPassword(selectedUserId);
                LoadUsersIfPermitted();
                using (var dialog = new InitialCredentialDialog(userName, password))
                    dialog.ShowDialog(this);
            }
            catch (Exception ex) { MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "صدور گذرواژه انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void ToggleUser_Click(object sender, EventArgs e)
        {
            try
            {
                AccessControlRepository.SetUserActive(selectedUserId, !selectedUserIsActive);
                LoadUsersIfPermitted();
            }
            catch (Exception ex) { MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "تغییر وضعیت انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void LoadUsersIfPermitted()
        {
            if (AuthorizationGuard.HasAny("users.view", "users.update", "users.reset_password", "users.assign_roles", "users.deactivate")) LoadUsers();
        }

        private void LoadRoles()
        {
            loading = true;
            rolesTable = AccessControlRepository.GetRoles();
            rolesList.Items.Clear();
            foreach (DataRow row in rolesTable.Rows)
                rolesList.Items.Add(new RoleChoice(Convert.ToInt32(row["RoleId"]), Convert.ToString(row["DisplayName"]), Convert.ToBoolean(row["IsSystem"])));
            selectedRoleId = 0;
            loading = false;
            ClearRoleEditor();
        }

        private void RolesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading) return;
            var role = rolesList.SelectedItem as RoleChoice;
            if (role == null) { selectedRoleId = 0; ClearRoleEditor(); return; }
            selectedRoleId = role.Id;
            DataRow row = FindRoleRow(role.Id);
            bool canView = AuthorizationGuard.Has("roles.view");
            bool canEditMetadata = AuthorizationGuard.Has("roles.update") && !role.IsSystem;
            bool canAssignPermissions = AuthorizationGuard.Has("roles.assign_permissions") && !role.IsSystem;
            bool canReadDetails = canView || canEditMetadata;
            roleMetadata.Visible = canEditMetadata || AuthorizationGuard.Has("roles.create");
            roleDetails.Visible = !canEditMetadata && canReadDetails;
            roleDetails.Text = role.Name + Environment.NewLine + (row == null || !row.Table.Columns.Contains("Description") || row.IsNull("Description") ? "" : Convert.ToString(row["Description"]));
            if (canEditMetadata)
            {
                txtRoleName.Text = row == null ? role.Name : Convert.ToString(row["DisplayName"]);
                txtRoleDescription.Text = row == null || row.IsNull("Description") ? "" : Convert.ToString(row["Description"]);
            }
            btnCreateRole.Visible = AuthorizationGuard.Has("roles.create");
            btnSaveRole.Visible = canEditMetadata;
            btnSavePermissions.Visible = canAssignPermissions;
            btnDeleteRole.Visible = !role.IsSystem && AuthorizationGuard.Has("roles.delete");
            var selected = AuthorizationGuard.HasAny("roles.view", "roles.assign_permissions")
                ? new HashSet<string>(AccessControlRepository.GetRolePermissionCodes(role.Id), StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            permissionPanel.Visible = canAssignPermissions;
            assignedPermissions.Visible = canView && !canAssignPermissions;
            assignedPermissions.Items.Clear();
            if (assignedPermissions.Visible)
            {
                var selectedDefinitions = PermissionCatalog.All.Where(p => selected.Contains(p.Code)).OrderBy(p => p.DisplayOrder).ToList();
                if (selectedDefinitions.Count == 0) assignedPermissions.Items.Add("برای این نقش مجوزی ثبت نشده است.");
                foreach (var permission in selectedDefinitions)
                    assignedPermissions.Items.Add(permission.Category + " | " + permission.Name + " — " + permission.Description);
            }
            foreach (var pair in permissionLists)
            {
                for (int i = 0; i < pair.Value.Items.Count; i++)
                {
                    var item = pair.Value.Items[i] as PermissionChoice;
                    pair.Value.SetItemChecked(i, canAssignPermissions && item != null && selected.Contains(item.Code));
                }
            }
        }

        private DataRow FindRoleRow(int roleId)
        {
            if (rolesTable == null) return null;
            foreach (DataRow row in rolesTable.Rows) if (Convert.ToInt32(row["RoleId"]) == roleId) return row;
            return null;
        }

        private void CreateRole_Click(object sender, EventArgs e)
        {
            try
            {
                int roleId = AccessControlRepository.CreateRole(txtRoleName.Text, txtRoleDescription.Text);
                if (AuthorizationGuard.HasAny("roles.view", "roles.update", "roles.assign_permissions", "roles.delete"))
                {
                    LoadRoles();
                    for (int i = 0; i < rolesList.Items.Count; i++)
                        if (((RoleChoice)rolesList.Items[i]).Id == roleId) { rolesList.SelectedIndex = i; break; }
                }
                else
                {
                    selectedRoleId = 0;
                    ClearRoleEditor();
                }
            }
            catch (Exception ex) { MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "ساخت نقش انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void SaveRole_Click(object sender, EventArgs e)
        {
            try
            {
                AccessControlRepository.UpdateRoleMetadata(selectedRoleId, txtRoleName.Text, txtRoleDescription.Text);
                if (AuthorizationGuard.Has("roles.view")) LoadRoles();
            }
            catch (Exception ex) { MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "ذخیره‌ی نقش انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void SaveRolePermissions_Click(object sender, EventArgs e)
        {
            try
            {
                var codes = new List<string>();
                foreach (var list in permissionLists.Values)
                    foreach (PermissionChoice choice in list.CheckedItems) codes.Add(choice.Code);
                AccessControlRepository.SetRolePermissions(selectedRoleId, codes);
                if (AuthorizationGuard.Has("roles.view")) LoadRoles();
                MessageBox.Show(this, "مجوزهای نقش ذخیره شد. نشست کاربران متأثر، مجوز تازه را پیش از عملیات بعدی دریافت می‌کند.", "ذخیره‌ی دسترسی‌ها", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "ذخیره‌ی مجوزها انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void DeleteRole_Click(object sender, EventArgs e)
        {
            if (selectedRoleId == 0 || MessageBox.Show(this, "نقش انتخاب‌شده حذف شود؟", "تأیید حذف نقش", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try { AccessControlRepository.DeleteRole(selectedRoleId); LoadRoles(); }
            catch (Exception ex) { MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "حذف نقش انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void ClearRoleEditor()
        {
            bool canCreate = AuthorizationGuard.Has("roles.create");
            txtRoleName.Clear();
            txtRoleDescription.Clear();
            roleMetadata.Visible = canCreate;
            roleDetails.Visible = false;
            roleDetails.Text = "";
            btnCreateRole.Visible = canCreate;
            btnSaveRole.Visible = false;
            btnSavePermissions.Visible = false;
            btnDeleteRole.Visible = false;
            permissionPanel.Visible = false;
            assignedPermissions.Visible = false;
            assignedPermissions.Items.Clear();
            foreach (var list in permissionLists.Values)
            {
                for (int i = 0; i < list.Items.Count; i++) list.SetItemChecked(i, false);
            }
        }

        private static List<int> CheckedRoleIds(CheckedListBox list)
        {
            var result = new List<int>();
            foreach (RoleChoice choice in list.CheckedItems) result.Add(choice.Id);
            return result;
        }

        private static void HideColumn(DataGridView grid, params string[] names)
        {
            foreach (string name in names) if (grid.Columns.Contains(name)) grid.Columns[name].Visible = false;
        }

        private static void SetHeader(DataGridView grid, string name, string header)
        {
            if (grid.Columns.Contains(name)) grid.Columns[name].HeaderText = header;
        }

        private static void SetNamedControlVisible(Control parent, string name, bool visible)
        {
            if (parent == null) return;
            foreach (Control control in parent.Controls)
            {
                if (String.Equals(control.Name, name, StringComparison.Ordinal)) control.Visible = visible;
                SetNamedControlVisible(control, name, visible);
            }
        }

        private sealed class RoleChoice
        {
            public int Id { get; private set; }
            public string Name { get; private set; }
            public bool IsSystem { get; private set; }
            public RoleChoice(int id, string name, bool isSystem) { Id = id; Name = name; IsSystem = isSystem; }
            public override string ToString() { return Name + (IsSystem ? " (سیستمی)" : ""); }
        }

        private sealed class PermissionChoice
        {
            public string Code { get; private set; }
            private readonly string display;
            public PermissionChoice(string code, string display) { Code = code; this.display = display; }
            public override string ToString() { return display; }
        }
    }
}
