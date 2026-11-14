using System;
using System.Collections.Generic;
using System.Linq;

namespace FuntinoControlCenter.Security
{
    internal sealed class PermissionDefinition
    {
        public string Code { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    internal static class PermissionCatalog
    {
        private static readonly string[] BusinessSchemas =
        {
            "finance", "pricing", "payments", "ticket", "reference", "device", "security", "configuration", "business"
        };
        private static readonly List<PermissionDefinition> Definitions = Build();

        public static IList<PermissionDefinition> All { get { return Definitions.AsReadOnly(); } }
        public static IEnumerable<string> Codes { get { return Definitions.Select(p => p.Code); } }
        public static IEnumerable<string> DataSchemas { get { return BusinessSchemas; } }

        public static string TableCode(string schema, string action)
        {
            return "data." + (schema ?? "").Trim().ToLowerInvariant() + "." + (action ?? "").Trim().ToLowerInvariant();
        }

        public static string SchemaCaption(string schema)
        {
            switch ((schema ?? "").ToLowerInvariant())
            {
                case "finance": return "تراکنش‌های مالی";
                case "pricing": return "قیمت‌گذاری و تعرفه";
                case "payments": return "پرداخت و پایانه";
                case "ticket": return "بلیت و ورود";
                case "reference": return "اطلاعات پایه";
                case "device": return "دستگاه‌ها";
                case "security": return "امنیت و حساب‌ها";
                case "configuration": return "پیکربندی";
                case "business": return "مدیریت کسب‌وکار";
                default: return "داده‌های " + (schema ?? "");
            }
        }

        private static List<PermissionDefinition> Build()
        {
            var result = new List<PermissionDefinition>();
            int order = 10;
            Add(result, "داشبورد", "dashboard.view", "مشاهده‌ی داشبورد", "نمایش خلاصه‌ی وضعیت و آمار کلی برنامه.", order++);
            Add(result, "گزارش‌ها", "reports.view", "مشاهده‌ی گزارش‌ها", "بازکردن گزارش‌ها و مرور داده‌هایی که مجوز مشاهده‌شان را دارید.", order++);
            Add(result, "گزارش‌ها", "reports.export", "گرفتن خروجی گزارش", "ذخیره‌ی گزارش‌های مجاز در قالب Excel یا PDF.", order++);
            Add(result, "دستگاه‌ها", "devices.view", "مشاهده‌ی وضعیت دستگاه‌ها", "نمایش وضعیت درگاه‌های سخت‌افزاری و ارتباط پایگاه‌ها.", order++);
            Add(result, "دستگاه‌ها", "devices.test", "آزمایش دستگاه‌ها", "اجرای آزمایش اتصال کارت‌خوان، درگاه و SDK پایانه.", order++);
            Add(result, "دستگاه‌ها", "devices.events.view", "مشاهده‌ی سوابق آزمایش", "نمایش نتیجه‌ی آزمایش‌های قبلی دستگاه‌ها.", order++);
            Add(result, "عملیات کارت", "cards.details.view", "خواندن اطلاعات کارت", "خواندن شماره و مشخصات کارت با درخواست کاربر.", order++);
            Add(result, "عملیات کارت", "cards.balance.view", "مشاهده‌ی موجودی کارت", "خواندن موجودی کارت با درخواست کاربر.", order++);
            Add(result, "عملیات کارت", "cards.remainder.view", "مشاهده‌ی باقیمانده‌ی آخرین شارژ", "خواندن مبلغ باقیمانده‌ی آخرین شارژ ذخیره‌شده روی کارت.", order++);
            Add(result, "عملیات کارت", "cards.history.view", "مشاهده‌ی سوابق کارت", "خواندن سوابق روی کارت و تراکنش‌های شارژ ثبت‌شده.", order++);
            Add(result, "عملیات کارت", "cards.recharge", "شارژ کارت", "افزایش موجودی کارت و ثبت تراکنش شارژ.", order++);
            Add(result, "تنظیمات", "settings.database.view", "مشاهده‌ی تنظیمات پایگاه", "مشاهده‌ی نام سرور و روش اتصال به پایگاه داده.", order++);
            Add(result, "تنظیمات", "settings.database.update", "تغییر تنظیمات پایگاه", "تغییر روش و نشانی اتصال پایگاه داده.", order++);
            Add(result, "تنظیمات", "settings.database.test", "آزمایش اتصال پایگاه", "آزمایش اتصال به هر دو پایگاه عملیاتی.", order++);
            Add(result, "تنظیمات", "settings.hardware.view", "مشاهده‌ی تنظیمات سخت‌افزار", "مشاهده‌ی درگاه و سرعت کارت‌خوان و پایانه.", order++);
            Add(result, "تنظیمات", "settings.hardware.update", "تغییر تنظیمات سخت‌افزار", "تغییر درگاه و سرعت ارتباط دستگاه‌ها.", order++);
            Add(result, "کاربران", "users.view", "مشاهده‌ی کاربران", "دیدن فهرست حساب‌ها و وضعیت آن‌ها.", order++);
            Add(result, "کاربران", "users.create", "ساخت کاربر", "ساخت حساب تازه با گذرواژه‌ی موقت و الزام تغییر آن در ورود نخست.", order++);
            Add(result, "کاربران", "users.update", "ویرایش مشخصات کاربر", "تغییر نام نمایشی حساب کاربری.", order++);
            Add(result, "کاربران", "users.reset_password", "صدور گذرواژه‌ی موقت", "ساخت گذرواژه‌ی موقت تازه و الزام تغییر آن در ورود بعدی.", order++);
            Add(result, "کاربران", "users.assign_roles", "تخصیص نقش به کاربر", "تغییر نقش‌های دسترسی کاربر.", order++);
            Add(result, "کاربران", "users.deactivate", "فعال یا غیرفعال‌کردن کاربر", "قطع یا برقراری امکان ورود حساب کاربری.", order++);
            Add(result, "نقش‌ها", "roles.view", "مشاهده‌ی نقش‌ها", "دیدن نقش‌ها و مجوزهای اختصاص‌یافته به آن‌ها.", order++);
            Add(result, "نقش‌ها", "roles.create", "ساخت نقش", "تعریف نقش تازه برای تخصیص به کاربران.", order++);
            Add(result, "نقش‌ها", "roles.update", "ویرایش مشخصات نقش", "تغییر نام و توضیح نقش‌های قابل‌ویرایش.", order++);
            Add(result, "نقش‌ها", "roles.assign_permissions", "تخصیص مجوز به نقش", "تعیین دقیق بخش‌ها و عملیات مجاز برای نقش.", order++);
            Add(result, "نقش‌ها", "roles.delete", "حذف نقش", "حذف نقش سفارشی‌ای که به هیچ کاربری تخصیص داده نشده است.", order++);

            string[,] schemas =
            {
                { "finance", "تراکنش‌های مالی" }, { "pricing", "قیمت‌گذاری و تعرفه" },
                { "payments", "پرداخت و پایانه" }, { "ticket", "بلیت و ورود" },
                { "reference", "اطلاعات پایه" }, { "device", "داده‌های دستگاه‌ها" },
                { "security", "امنیت و حساب‌های قدیمی" }, { "configuration", "پیکربندی" },
                { "business", "مدیریت کسب‌وکار" }
            };
            string[] actions = { "view", "create", "update", "delete" };
            string[] names = { "مشاهده", "ایجاد", "ویرایش", "حذف" };
            string[] descriptions =
            {
                "دیدن جدول‌ها و ردیف‌های این حوزه.",
                "افزودن ردیف تازه در جدول‌های این حوزه.",
                "تغییر ردیف‌های موجود در این حوزه.",
                "حذف ردیف از جدول‌های این حوزه."
            };
            for (int i = 0; i < schemas.GetLength(0); i++)
                for (int j = 0; j < actions.Length; j++)
                    Add(result, schemas[i, 1], TableCode(schemas[i, 0], actions[j]), names[j] + " | " + schemas[i, 1], descriptions[j], order++);
            return result;
        }

        private static void Add(List<PermissionDefinition> list, string category, string code, string name, string description, int order)
        {
            list.Add(new PermissionDefinition { Category = category, Code = code, Name = name, Description = description, DisplayOrder = order });
        }
    }
}
