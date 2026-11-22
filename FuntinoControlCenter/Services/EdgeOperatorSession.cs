using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;

namespace FuntinoControlCenter.Services
{
    internal static class EdgeOperatorSession
    {
        private static readonly object Sync = new object();
        private static readonly HashSet<string> KnownPermissions = new HashSet<string>(StringComparer.Ordinal)
        {
            "card.inventory.read", "card.inventory.manage", "card.assign", "card.transfer", "card.unassign",
            "customer.lookup.limited", "customer.provisional.create", "customer.family.read",
            "customer.family.manage", "customer.profile.manage"
        };
        private static byte[] tokenBytes;
        private static string branchId;
        private static HashSet<string> permissions;
        private static DateTime expiresAtUtc;
        private static Timer expirationTimer;

        static EdgeOperatorSession()
        {
            AppSession.AuthorizationStateChanged += Clear;
        }

        public static bool IsActive(string expectedBranchId)
        {
            lock (Sync)
            {
                ClearIfExpiredUnsafe();
                return tokenBytes != null && String.Equals(branchId, expectedBranchId, StringComparison.OrdinalIgnoreCase);
            }
        }

        public static DateTime? ExpiresAtUtc
        {
            get
            {
                lock (Sync)
                {
                    ClearIfExpiredUnsafe();
                    return tokenBytes == null ? (DateTime?)null : expiresAtUtc;
                }
            }
        }

        public static bool HasPermission(string expectedBranchId, string permission)
        {
            lock (Sync)
            {
                ClearIfExpiredUnsafe();
                return tokenBytes != null && String.Equals(branchId, expectedBranchId, StringComparison.OrdinalIgnoreCase) &&
                    permissions != null && permissions.Contains(permission ?? String.Empty);
            }
        }

        public static bool TryGetToken(string expectedBranchId, string permission, out string token)
        {
            lock (Sync)
            {
                ClearIfExpiredUnsafe();
                if (tokenBytes == null || !String.Equals(branchId, expectedBranchId, StringComparison.OrdinalIgnoreCase) ||
                    (permission != null && (permissions == null || !permissions.Contains(permission))))
                {
                    token = null;
                    return false;
                }

                token = Encoding.UTF8.GetString(tokenBytes);
                return token.Length >= 32 && token.Length <= 256;
            }
        }

        public static void Store(EdgeOperatorTokenDto value, string expectedOrganizationId, string expectedBranchId)
        {
            if (value == null || String.IsNullOrWhiteSpace(value.Token) || value.Token.Length < 32 || value.Token.Length > 256)
                throw new SecurityException("پاسخ مجوز مرکزی معتبر نیست.");
            Guid tokenId, organizationId, branch;
            DateTimeOffset expiration;
            if (!Guid.TryParse(value.TokenId, out tokenId) || !Guid.TryParse(value.OrganizationId, out organizationId) ||
                !Guid.TryParse(value.BranchId, out branch) || !Guid.TryParse(expectedOrganizationId, out Guid expectedOrg) ||
                !Guid.TryParse(expectedBranchId, out Guid expectedBranch) || organizationId != expectedOrg || branch != expectedBranch ||
                !DateTimeOffset.TryParse(value.ExpiresAt, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out expiration))
                throw new SecurityException("محدودهٔ مجوز مرکزی با این نصب یکسان نیست.");

            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (expiration <= now || expiration > now.AddMinutes(5).AddSeconds(30) || value.PolicyVersion < 1 || value.CatalogVersion < 1 ||
                value.PermissionKeys == null || value.PermissionKeys.Count == 0 || value.PermissionKeys.Count > KnownPermissions.Count ||
                value.PermissionKeys.Any(permission => !KnownPermissions.Contains(permission)) ||
                value.PermissionKeys.Distinct(StringComparer.Ordinal).Count() != value.PermissionKeys.Count)
                throw new SecurityException("زمان یا دسترسی‌های مجوز مرکزی معتبر نیست.");

            byte[] replacement = Encoding.UTF8.GetBytes(value.Token);
            lock (Sync)
            {
                ClearUnsafe();
                tokenBytes = replacement;
                branchId = expectedBranch.ToString("D");
                permissions = new HashSet<string>(value.PermissionKeys, StringComparer.Ordinal);
                expiresAtUtc = expiration.UtcDateTime;
                TimeSpan remaining = expiresAtUtc - DateTime.UtcNow;
                expirationTimer = new Timer(ClearIfExpired, null, remaining, Timeout.InfiniteTimeSpan);
            }
        }

        public static void Clear()
        {
            lock (Sync) ClearUnsafe();
        }

        private static void ClearIfExpired(object state)
        {
            lock (Sync) ClearIfExpiredUnsafe();
        }

        private static void ClearIfExpiredUnsafe()
        {
            if (tokenBytes != null && DateTime.UtcNow >= expiresAtUtc) ClearUnsafe();
        }

        private static void ClearUnsafe()
        {
            if (expirationTimer != null)
            {
                expirationTimer.Dispose();
                expirationTimer = null;
            }
            if (tokenBytes != null) Array.Clear(tokenBytes, 0, tokenBytes.Length);
            tokenBytes = null;
            branchId = null;
            permissions = null;
            expiresAtUtc = DateTime.MinValue;
        }
    }
}
