using System;
using System.Collections.Generic;
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
    internal sealed partial class UserEditorForm : Form
    {
        private readonly int userId;
        private readonly bool isCreate;
        private readonly bool allowLoginEdit;
        private readonly bool allowDisplayEdit;
        private readonly bool allowAssignRoles;
        private readonly bool allowPasswordReset;
        private readonly bool allowStatusChange;
        private readonly List<AccessRoleOption> availableRoles;
        private readonly HashSet<int> selectedRoleIds;
        private bool isActive;
        private bool mustChangePassword;
        private bool busy;

        public UserEditorForm()
            : this(0, string.Empty, string.Empty, true, false, true, true, true, false, false,
                new List<AccessRoleOption>(), new List<int>())
        {
        }

        public UserEditorForm(int userId, string userName, string displayName, bool isActive, bool mustChangePassword,
            bool allowLoginEdit, bool allowDisplayEdit, bool allowAssignRoles, bool allowPasswordReset,
            bool allowStatusChange, IList<AccessRoleOption> availableRoles, IEnumerable<int> selectedRoleIds)
        {
            AppFontManager.EnsureVazirmatnLoaded();
            InitializeComponent();
            this.userId = userId;
            isCreate = userId == 0;
            this.isActive = isActive;
            this.mustChangePassword = mustChangePassword;
            this.allowLoginEdit = allowLoginEdit;
            this.allowDisplayEdit = allowDisplayEdit;
            this.allowAssignRoles = allowAssignRoles;
            this.allowPasswordReset = allowPasswordReset;
            this.allowStatusChange = allowStatusChange;
            this.availableRoles = (availableRoles ?? new List<AccessRoleOption>()).ToList();
            this.selectedRoleIds = new HashSet<int>(selectedRoleIds ?? Enumerable.Empty<int>());

            txtUserName.Text = userName ?? string.Empty;
            txtDisplayName.Text = displayName ?? string.Empty;
            txtUserName.ReadOnly = !allowLoginEdit;
            txtUserName.Visible = allowLoginEdit || isCreate;
            lblUserName.Visible = txtUserName.Visible;
            txtDisplayName.Visible = allowDisplayEdit || isCreate;
            lblDisplayName.Visible = txtDisplayName.Visible;
            rolesCard.Visible = allowAssignRoles;
            accountActionsCard.Visible = !isCreate && (allowPasswordReset || allowStatusChange);
            btnResetPassword.Visible = !isCreate && allowPasswordReset;
            btnToggleStatus.Visible = !isCreate && allowStatusChange;
            btnSave.Text = isCreate ? "ساخت حساب" : "ذخیره تغییرها";
            btnSave.Visible = isCreate || allowDisplayEdit || allowAssignRoles;
            lblEditorTitle.Text = isCreate ? "ساخت حساب کاربری" : "مدیریت حساب کاربری";
            lblEditorSubtitle.Text = isCreate
                ? "برای کاربر نام ورود و گذرواژهٔ موقت ساخته می‌شود؛ او باید در نخستین ورود گذرواژه را عوض کند."
                : "مشخصات، نقش‌ها و وضعیت حساب را از همین صفحه مدیریت کنید.";
            lblPasswordState.Visible = !isCreate && (allowPasswordReset || mustChangePassword);
            RefreshAccountState();
            RefreshRoleChoices();
            UpdateFormLayout();
        }

        public bool Changed { get; private set; }

        private async void Save_Click(object sender, EventArgs e)
        {
            if (busy) return;
            if (isCreate && string.IsNullOrWhiteSpace(txtUserName.Text))
            {
                MessageBox.Show(this, "نام ورود را وارد کنید.", "نام ورود لازم است", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtUserName.Focus();
                return;
            }
            if ((isCreate || allowDisplayEdit) && string.IsNullOrWhiteSpace(txtDisplayName.Text))
            {
                MessageBox.Show(this, "نام نمایشی را وارد کنید.", "نام نمایشی لازم است", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtDisplayName.Focus();
                return;
            }

            try
            {
                string userName = txtUserName.Text.Trim();
                string displayName = txtDisplayName.Text.Trim();
                List<int> roleIds = allowAssignRoles ? selectedRoleIds.ToList() : new List<int>();
                SetBusy(true, isCreate ? "در حال ساخت حساب کاربری…" : "در حال ذخیره تغییرهای حساب…", AccessManagementVisualAssets.Save);
                if (isCreate)
                {
                    AuthorizationGuard.Require("users.create");
                    var credential = await Task.Run(() => AccessControlRepository.CreateUser(userName, displayName, roleIds));
                    using (var dialog = new InitialCredentialDialog(credential.UserName, credential.Password)) dialog.ShowDialog(this);
                }
                else
                {
                    if (allowDisplayEdit) AuthorizationGuard.Require("users.update");
                    if (allowAssignRoles) AuthorizationGuard.Require("users.assign_roles");
                    await Task.Run(() =>
                    {
                        if (allowDisplayEdit) AccessControlRepository.UpdateUserDisplayName(userId, displayName);
                        if (allowAssignRoles) AccessControlRepository.SetUserRoles(userId, roleIds);
                    });
                }

                Changed = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseManager.SafeMessage(ex), isCreate ? "ساخت حساب انجام نشد" : "ذخیره تغییرها انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally { SetBusy(false, null, null); }
        }

        private async void ResetPassword_Click(object sender, EventArgs e)
        {
            if (busy || !allowPasswordReset || userId == 0) return;
            if (MessageBox.Show(this, "گذرواژهٔ موقت تازه‌ای برای این کاربر ساخته شود؟ گذرواژهٔ فعلی دیگر کار نمی‌کند.",
                "صدور گذرواژهٔ تازه", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                AuthorizationGuard.Require("users.reset_password");
                SetBusy(true, "در حال ساخت گذرواژهٔ موقت…", AccessManagementVisualAssets.PasswordReset);
                string password = await Task.Run(() => AccessControlRepository.ResetPassword(userId));
                mustChangePassword = true;
                RefreshAccountState();
                using (var dialog = new InitialCredentialDialog(txtUserName.Text, password)) dialog.ShowDialog(this);
                Changed = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "صدور گذرواژه انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally { SetBusy(false, null, null); }
        }

        private async void ToggleStatus_Click(object sender, EventArgs e)
        {
            if (busy || !allowStatusChange || userId == 0) return;
            string action = isActive ? "غیرفعال" : "فعال";
            string question = isActive
                ? "این کاربر دیگر نمی‌تواند وارد نرم‌افزار شود. حساب غیرفعال شود؟"
                : "امکان ورود این کاربر دوباره برقرار شود؟";
            if (MessageBox.Show(this, question, "تغییر وضعیت حساب", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                AuthorizationGuard.Require("users.deactivate");
                SetBusy(true, "در حال تغییر وضعیت حساب…", AccessManagementVisualAssets.UserStatus);
                bool nextState = !isActive;
                await Task.Run(() => AccessControlRepository.SetUserActive(userId, nextState));
                isActive = nextState;
                Changed = true;
                RefreshAccountState();
                MessageBox.Show(this, "حساب " + action + " شد.", "وضعیت حساب به‌روز شد", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "تغییر وضعیت حساب انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally { SetBusy(false, null, null); }
        }

        private void RefreshRoleChoices()
        {
            if (!allowAssignRoles) return;
            string search = (txtRoleSearch.Text ?? string.Empty).Trim();
            var matches = availableRoles.Where(role => search.Length == 0 || role.Name.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0).ToList();
            clbRoles.BeginUpdate();
            clbRoles.Items.Clear();
            foreach (AccessRoleOption role in matches)
                clbRoles.Items.Add(role, selectedRoleIds.Contains(role.Id));
            clbRoles.EndUpdate();
            lblRoleCount.Text = selectedRoleIds.Count.ToString("N0") + " نقش انتخاب شده";
            lblNoRoles.Visible = availableRoles.Count == 0;
            if (availableRoles.Count == 0) lblNoRoles.Text = "نقشی که بتوانید به این کاربر واگذار کنید موجود نیست.";
            else if (matches.Count == 0) { lblNoRoles.Visible = true; lblNoRoles.Text = "نقشی با این جست‌وجو پیدا نشد."; }
        }

        private void Roles_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var role = e.Index >= 0 && e.Index < clbRoles.Items.Count ? clbRoles.Items[e.Index] as AccessRoleOption : null;
            if (role == null) return;
            if (e.NewValue == CheckState.Checked) selectedRoleIds.Add(role.Id);
            else selectedRoleIds.Remove(role.Id);
            BeginInvoke(new Action(() => lblRoleCount.Text = selectedRoleIds.Count.ToString("N0") + " نقش انتخاب شده"));
        }

        private void RoleSearch_TextChanged(object sender, EventArgs e) { RefreshRoleChoices(); }
        private void Cancel_Click(object sender, EventArgs e) { DialogResult = DialogResult.Cancel; Close(); }

        private void RefreshAccountState()
        {
            lblAccountState.Text = isCreate ? "حساب پس از ساخت فعال خواهد بود." : (isActive ? "وضعیت حساب: فعال" : "وضعیت حساب: غیرفعال");
            lblPasswordState.Text = mustChangePassword ? "کاربر باید در ورود بعدی گذرواژه را عوض کند." : "برای این حساب تغییر گذرواژه اجباری ثبت نشده است.";
            btnToggleStatus.Text = isActive ? "غیرفعال‌کردن حساب" : "فعال‌کردن حساب";
        }

        private void SetBusy(bool value, string message, System.Drawing.Image icon)
        {
            busy = value;
            if (value) loadingIndicator.BeginLoading(message, icon);
            else loadingIndicator.EndLoading();
            btnSave.Enabled = !value;
            btnCancel.Enabled = !value;
            btnResetPassword.Enabled = !value;
            btnToggleStatus.Enabled = !value;
            txtUserName.Enabled = !value;
            txtDisplayName.Enabled = !value && allowDisplayEdit;
            clbRoles.Enabled = !value && allowAssignRoles;
            txtRoleSearch.Enabled = !value && allowAssignRoles;
        }

        private void UpdateFormLayout()
        {
            headerIcon.Image = isCreate ? AccessManagementVisualAssets.UserAdd : AccessManagementVisualAssets.UserEdit;
        }
    }
}
