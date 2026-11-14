using System;
using System.Collections.Generic;
using System.Linq;
using FuntinoControlCenter.Database;

namespace FuntinoControlCenter
{
    internal sealed class AppSession
    {
        private readonly object authorizationLock = new object();
        private HashSet<string> permissions;
        private HashSet<string> roleCodes;
        private DateTime lastAuthorizationCheckUtc;

        public int ApplicationUserId { get; private set; }
        public string UserName { get; private set; }
        public bool MustChangePassword { get; private set; }
        public int AuthorizationVersion { get; private set; }
        public static AppSession Current { get; private set; }
        public static event Action AuthorizationStateChanged;

        public AppSession(int applicationUserId, string userName, bool mustChangePassword, int authorizationVersion, IEnumerable<string> permissionCodes, IEnumerable<string> assignedRoleCodes)
        {
            ApplicationUserId = applicationUserId;
            UserName = userName;
            MustChangePassword = mustChangePassword;
            AuthorizationVersion = authorizationVersion;
            permissions = new HashSet<string>(permissionCodes ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            roleCodes = new HashSet<string>(assignedRoleCodes ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            lastAuthorizationCheckUtc = DateTime.UtcNow;
            Current = this;
        }

        public bool HasPermission(string permission)
        {
            RefreshAuthorization(false);
            return HasPermissionCached(permission);
        }

        public bool HasAnyPermission(params string[] permissionCodes)
        {
            RefreshAuthorization(false);
            return permissionCodes != null && permissionCodes.Any(HasPermissionCached);
        }

        public bool HasPermissionCached(string permission)
        {
            lock (authorizationLock) return !MustChangePassword && permissions.Contains(permission ?? "");
        }

        public void RefreshAuthorization(bool force)
        {
            if (!force && DateTime.UtcNow - lastAuthorizationCheckUtc < TimeSpan.FromSeconds(5)) return;
            lock (authorizationLock)
            {
                if (!force && DateTime.UtcNow - lastAuthorizationCheckUtc < TimeSpan.FromSeconds(5)) return;
                AccessControlRepository.RefreshSession(this);
                lastAuthorizationCheckUtc = DateTime.UtcNow;
            }
        }

        internal void ReplaceAuthorization(int version, IEnumerable<string> permissionCodes, IEnumerable<string> assignedRoleCodes)
        {
            bool changed;
            lock (authorizationLock)
            {
                var replacement = new HashSet<string>(permissionCodes ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
                changed = !permissions.SetEquals(replacement);
                AuthorizationVersion = version;
                permissions = replacement;
                roleCodes = new HashSet<string>(assignedRoleCodes ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            }
            if (changed && AuthorizationStateChanged != null) AuthorizationStateChanged();
        }

        public bool HasRole(string roleCode)
        {
            RefreshAuthorization(false);
            lock (authorizationLock) return roleCodes.Contains(roleCode ?? "");
        }

        public void CompletePasswordChange() { SetMustChangePassword(false); }

        internal void SetMustChangePassword(bool required)
        {
            bool changed;
            lock (authorizationLock)
            {
                changed = MustChangePassword != required;
                MustChangePassword = required;
            }
            if (changed && AuthorizationStateChanged != null) AuthorizationStateChanged();
        }
        public static void Clear()
        {
            if (Current == null) return;
            Current = null;
            if (AuthorizationStateChanged != null) AuthorizationStateChanged();
        }
    }
}
