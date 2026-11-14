using System;
using System.Linq;

namespace FuntinoControlCenter.Security
{
    internal static class AuthorizationGuard
    {
        public static bool Has(string permission)
        {
            var session = AppSession.Current;
            return session != null && session.HasPermission(permission);
        }

        public static bool HasAny(params string[] permissions)
        {
            var session = AppSession.Current;
            return session != null && session.HasAnyPermission(permissions);
        }

        public static void Require(string permission)
        {
            var session = AppSession.Current;
            if (session == null) throw new UnauthorizedAccessException("برای انجام این کار باید وارد حساب کاربری شوید.");
            session.RefreshAuthorization(true);
            if (session.MustChangePassword) throw new UnauthorizedAccessException("پیش از ادامه باید گذرواژه‌ی موقت را تغییر دهید.");
            if (!session.HasPermissionCached(permission))
            {
                var definition = PermissionCatalog.All.FirstOrDefault(p => String.Equals(p.Code, permission, StringComparison.OrdinalIgnoreCase));
                throw new UnauthorizedAccessException("مجوز لازم برای این کار را ندارید" + (definition == null ? "." : ": " + definition.Name + "."));
            }
        }

        public static void RequireAny(params string[] permissions)
        {
            if (permissions == null || permissions.Length == 0) throw new UnauthorizedAccessException("مجوز لازم برای این کار تعریف نشده است.");
            var session = AppSession.Current;
            if (session == null) throw new UnauthorizedAccessException("برای انجام این کار باید وارد حساب کاربری شوید.");
            session.RefreshAuthorization(true);
            if (session.MustChangePassword) throw new UnauthorizedAccessException("پیش از ادامه باید گذرواژه‌ی موقت را تغییر دهید.");
            if (!permissions.Any(session.HasPermissionCached))
                throw new UnauthorizedAccessException("مجوز لازم برای این بخش را ندارید.");
        }

        public static void RequireTable(string schema, string action)
        {
            Require(PermissionCatalog.TableCode(schema, action));
        }
    }
}
