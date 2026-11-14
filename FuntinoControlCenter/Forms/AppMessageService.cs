using System;
using System.Windows.Forms;

namespace FuntinoControlCenter.Forms
{
    internal enum AppMessageKind
    {
        Information,
        Success,
        Warning,
        Error,
        Confirmation
    }

    internal static class AppMessageService
    {
        public static DialogResult Show(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return Show(null, message, caption, buttons, icon, MessageBoxDefaultButton.Button1);
        }

        public static DialogResult Show(IWin32Window owner, string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return Show(owner, message, caption, buttons, icon, MessageBoxDefaultButton.Button1);
        }

        public static DialogResult Show(IWin32Window owner, string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            AppMessageKind kind = GetMessageKind(caption, buttons, icon);
            bool canUseToast = buttons == MessageBoxButtons.OK && (kind == AppMessageKind.Information || kind == AppMessageKind.Success);
            if (canUseToast && owner != null)
            {
                var toast = new AppToastForm(caption, message, kind);
                toast.ShowToast(owner);
                return DialogResult.OK;
            }

            using (var dialog = new AppMessageDialog(caption, message, kind, buttons, defaultButton))
                return dialog.ShowDialog(owner);
        }

        public static DialogResult ShowError(IWin32Window owner, string message, string caption)
        {
            return Show(owner, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static DialogResult ShowWarning(IWin32Window owner, string message, string caption)
        {
            return Show(owner, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ShowSuccess(IWin32Window owner, string message, string caption)
        {
            Show(owner, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static AppMessageKind GetMessageKind(string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            if (buttons != MessageBoxButtons.OK) return AppMessageKind.Confirmation;
            if (icon == MessageBoxIcon.Error || icon == MessageBoxIcon.Hand || icon == MessageBoxIcon.Stop)
                return AppMessageKind.Error;
            if (icon == MessageBoxIcon.Warning || icon == MessageBoxIcon.Exclamation)
                return AppMessageKind.Warning;
            if (icon == MessageBoxIcon.Information || icon == MessageBoxIcon.Asterisk)
                return IsSuccessCaption(caption) ? AppMessageKind.Success : AppMessageKind.Information;
            return AppMessageKind.Information;
        }

        private static bool IsSuccessCaption(string caption)
        {
            if (String.IsNullOrWhiteSpace(caption)) return false;
            return caption.IndexOf("موفق", StringComparison.Ordinal) >= 0 ||
                caption.IndexOf("آماده", StringComparison.Ordinal) >= 0 ||
                caption.IndexOf("ذخیره شد", StringComparison.Ordinal) >= 0 ||
                caption.IndexOf("برقرار است", StringComparison.Ordinal) >= 0 ||
                caption.IndexOf("خروجی آماده", StringComparison.Ordinal) >= 0 ||
                caption.IndexOf("انجام شد", StringComparison.Ordinal) >= 0;
        }
    }
}
