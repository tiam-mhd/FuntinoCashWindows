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
            Add(result, "مدیریت دستگاه‌های تفریحی", "operations.attractions.view", "مشاهده‌ی دستگاه‌های تفریحی", "دیدن فهرست دستگاه‌ها و قیمت پایه‌ی هر دستگاه.", order++);
            Add(result, "مدیریت دستگاه‌های تفریحی", "operations.attractions.create", "افزودن دستگاه تفریحی", "ثبت دستگاه تفریحی تازه در اطلاعات پایه.", order++);
            Add(result, "مدیریت دستگاه‌های تفریحی", "operations.attractions.update", "ویرایش دستگاه تفریحی", "تغییر نام، کد، قیمت یا وضعیت دستگاه تفریحی.", order++);
            Add(result, "مدیریت دستگاه‌های تفریحی", "operations.attractions.delete", "بایگانی دستگاه تفریحی", "غیرفعال‌کردن دستگاه تفریحی برای استفاده‌های بعدی.", order++);
            Add(result, "مدیریت کنترل‌گرها", "operations.devices.view", "مشاهده‌ی دستگاه‌های کنترل‌شونده", "دیدن دستگاه‌های متصل به سامانه‌ی کنترل شهربازی.", order++);
            Add(result, "مدیریت کنترل‌گرها", "operations.devices.create", "افزودن دستگاه کنترل‌شونده", "ثبت دستگاه تازه در فهرست کنترل‌گرها.", order++);
            Add(result, "مدیریت کنترل‌گرها", "operations.devices.update", "ویرایش دستگاه کنترل‌شونده", "تغییر شناسه، نام، قیمت‌گذاری یا تنظیمات دستگاه.", order++);
            Add(result, "مدیریت کنترل‌گرها", "operations.devices.delete", "از دسترس خارج‌کردن دستگاه", "غیرفعال‌کردن دستگاه بدون پاک‌کردن سوابق آن.", order++);
            Add(result, "جشنواره‌ها و تخفیف‌ها", "operations.promotions.view", "مشاهده‌ی جشنواره‌ها", "دیدن جشنواره‌های فعال و زمان‌بندی‌شده.", order++);
            Add(result, "جشنواره‌ها و تخفیف‌ها", "operations.promotions.create", "ساخت جشنواره", "ثبت جشنواره و همگام‌سازی آن با سامانه‌ی دستگاه‌ها.", order++);
            Add(result, "جشنواره‌ها و تخفیف‌ها", "operations.promotions.update", "ویرایش جشنواره", "تغییر مشخصات جشنواره در هر دو پایگاه عملیاتی.", order++);
            Add(result, "جشنواره‌ها و تخفیف‌ها", "operations.promotions.delete", "پایان‌دادن به جشنواره", "پایان‌دادن به جشنواره بدون حذف سوابق ثبت‌شده.", order++);
            Add(result, "جشنواره‌ها و تخفیف‌ها", "operations.promotions.assign_devices", "تخصیص جشنواره به دستگاه", "فعال یا غیرفعال‌کردن جشنواره برای هر دستگاه تفریحی.", order++);
            Add(result, "قوانین تخفیف", "operations.discounts.view", "مشاهده‌ی قوانین تخفیف", "دیدن روزها، بازه‌های مبلغ و سطح تخفیف هر قانون.", order++);
            Add(result, "قوانین تخفیف", "operations.discounts.create", "افزودن قانون تخفیف", "تعریف قانون تازه بر پایه‌ی روز هفته و بازه‌ی مبلغ.", order++);
            Add(result, "قوانین تخفیف", "operations.discounts.update", "ویرایش قانون تخفیف", "تغییر روز، بازه‌ی مبلغ، سطح یا وضعیت قانون.", order++);
            Add(result, "قوانین تخفیف", "operations.discounts.delete", "غیرفعال‌کردن قانون تخفیف", "غیرفعال‌کردن قانون بدون پاک‌کردن سوابق آن.", order++);
            Add(result, "قوانین تخفیف", "operations.discount_rates.view", "مشاهده‌ی درصدهای تخفیف", "دیدن درصدهای تعریف‌شده برای ده سطح تخفیف.", order++);
            Add(result, "قوانین تخفیف", "operations.discount_rates.update", "تغییر درصد تخفیف", "تغییر درصد یکی از سطح‌های ثابت تخفیف.", order++);
            Add(result, "تنظیمات شارژ", "operations.deposit.view", "مشاهده‌ی مبلغ بیعانه", "دیدن مبلغ بیعانه‌ی ثبت‌شده برای شارژ.", order++);
            Add(result, "تنظیمات شارژ", "operations.deposit.update", "تغییر مبلغ بیعانه", "ثبت مبلغ بیعانه در همه‌ی ردیف‌های تنظیمات برنامه.", order++);
            Add(result, "تنظیمات شارژ", "operations.recharge_limit.view", "مشاهده‌ی سقف شارژ", "دیدن بیشترین مبلغ مجاز برای هر بار شارژ.", order++);
            Add(result, "تنظیمات شارژ", "operations.recharge_limit.create", "تعریف سقف آغازین شارژ", "ثبت سقف شارژ اولیه پیش از شروع عملیات کارت.", order++);
            Add(result, "تنظیمات شارژ", "operations.recharge_limit.update", "تغییر سقف شارژ", "تغییر بیشترین مبلغ مجاز و اعمال آن در عملیات کارت.", order++);
            Add(result, "تنظیمات شارژ", "operations.hourly_plans.view", "مشاهده‌ی شارژهای ساعتی", "دیدن مبلغ و وضعیت گزینه‌های شارژ ساعتی.", order++);
            Add(result, "تنظیمات شارژ", "operations.hourly_plans.update", "ویرایش شارژهای ساعتی", "تغییر مبلغ یا فعال‌بودن هر گزینه‌ی شارژ ساعتی.", order++);
            Add(result, "تنظیمات شارژ", "operations.schedules.view", "مشاهده‌ی زمان‌بندی شارژ", "دیدن ساعت‌های ثبت‌شده و زمان فعال.", order++);
            Add(result, "تنظیمات شارژ", "operations.schedules.create", "افزودن زمان شارژ", "ثبت یک زمان تازه برای برنامه‌ی شارژ.", order++);
            Add(result, "تنظیمات شارژ", "operations.schedules.update", "ویرایش زمان شارژ", "تغییر زمان یا وضعیت یکی از برنامه‌ها.", order++);
            Add(result, "تنظیمات شارژ", "operations.schedules.delete", "غیرفعال‌کردن زمان شارژ", "غیرفعال‌کردن زمان بدون پاک‌کردن سابقه.", order++);
            Add(result, "تنظیمات بلیت", "operations.tickets.view", "مشاهده‌ی تنظیمات بلیت", "دیدن مبلغ و وضعیت گزینه‌های بلیت.", order++);
            Add(result, "تنظیمات بلیت", "operations.tickets.create", "افزودن مبلغ بلیت", "تعریف یک مبلغ تازه برای فروش بلیت.", order++);
            Add(result, "تنظیمات بلیت", "operations.tickets.update", "ویرایش مبلغ بلیت", "تغییر مبلغ یک گزینه‌ی بلیت.", order++);
            Add(result, "تنظیمات بلیت", "operations.tickets.delete", "غیرفعال‌کردن مبلغ بلیت", "غیرفعال‌کردن گزینه بدون حذف سابقه‌ی فروش.", order++);
            Add(result, "عملیات کارت", "operations.recharge_terms.view", "مشاهده‌ی شرایط شارژ", "نمایش سقف شارژ، تخفیف روز و جشنواره‌ی قابل‌اعمال هنگام شارژ.", order++);
            Add(result, "عملیات کارت", "cards.details.view", "خواندن اطلاعات کارت", "خواندن شماره و مشخصات کارت با درخواست کاربر.", order++);
            Add(result, "عملیات کارت", "cards.balance.view", "مشاهده‌ی موجودی کارت", "خواندن موجودی کارت با درخواست کاربر.", order++);
            Add(result, "عملیات کارت", "cards.remainder.view", "مشاهده‌ی باقیمانده‌ی آخرین شارژ", "خواندن مبلغ باقیمانده‌ی آخرین شارژ ذخیره‌شده روی کارت.", order++);
            Add(result, "عملیات کارت", "cards.history.view", "مشاهده‌ی سوابق کارت", "خواندن سوابق روی کارت و تراکنش‌های شارژ ثبت‌شده.", order++);
            Add(result, "عملیات کارت", "cards.recharge", "شارژ کارت", "افزایش موجودی کارت و ثبت تراکنش شارژ.", order++);
            Add(result, "مشتری و خانواده", "customers.view", "مشاهده‌ی مشتریان شعبه", "جست‌وجوی مشتریانی که در تصویر تأییدشده‌ی همین شعبه در دسترس هستند.", order++);
            Add(result, "مشتری و خانواده", "families.view", "مشاهده‌ی خانواده و اعضا", "دیدن سرپرست، اعضا و کارت‌های خانوادگی مجاز برای همین شعبه.", order++);
            Add(result, "مشتری و خانواده", "customers.profile.update", "ویرایش پرونده‌ی مشتری", "تغییر پرونده‌ی مشتری از مسیر مجاز سامانه‌ی مرکزی.", order++);
            Add(result, "مشتری و خانواده", "families.update", "ویرایش خانواده", "تغییر نام خانواده با بررسی نسخه‌ی ثبت‌شده در سامانه‌ی مرکزی.", order++);
            Add(result, "مشتری و خانواده", "families.members.create", "افزودن عضو خانواده", "افزودن عضو با ثبت تکرارناپذیر در سامانه‌ی مرکزی.", order++);
            Add(result, "کارت مشتری", "cards.assign", "اتصال کارت به مشتری", "اتصال کارت شناخته‌شده به مشتری یا خانواده در شعبه‌ی فعال.", order++);
            Add(result, "کارت مشتری", "cards.replace", "تعویض کارت", "ثبت کارت جایگزین و نگه‌داشتن سابقه‌ی کارت پیشین.", order++);
            Add(result, "کارت مشتری", "cards.report_lost", "ثبت مفقودی کارت", "گزارش مفقودی کارت برای پیگیری و مسدودسازی مرکزی.", order++);
            Add(result, "کارت مشتری", "cards.block", "مسدودسازی کارت", "مسدودکردن کارت شناخته‌شده از مسیر مجاز سامانه‌ی مرکزی.", order++);
            Add(result, "کارت مشتری", "customers.provisional.create", "ثبت مشتری موقت شعبه", "ثبت محلی مشتری موقت برای تأیید مرکزی پس از برقراری ارتباط.", order++);
            Add(result, "تراکنش مالی", "finance.adjustment", "اصلاح تراکنش مالی", "ثبت اصلاح مالی با شناسه‌ی پایدار، شعبه و دلیل ثبت‌شده.", order++);
            Add(result, "همگام‌سازی", "edge.events.create", "ثبت رویداد همگام‌سازی", "ثبت محلی رویداد همان تراکنش که پس از اتصال به سامانه فرستاده می‌شود.", order++);
            Add(result, "همگام‌سازی", "edge.sync.view", "مشاهده‌ی وضعیت همگام‌سازی", "دیدن وضعیت اتصال، تصویر شعبه و رویدادهای در انتظار یا ردشده.", order++);
            Add(result, "همگام‌سازی", "edge.sync.retry", "ارسال دوباره‌ی رویداد", "تلاش دوباره برای فرستادن همان رویداد بدون اجرای دوباره‌ی عملیات کارت یا مالی.", order++);
            Add(result, "همگام‌سازی", "edge.configuration.manage", "تنظیم ارتباط فانتینو", "تنظیم نشانی امن، شناسه‌ی شعبه و اعتبارنامه‌ی این نصب.", order++);
            Add(result, "همگام‌سازی", "edge.operator.pair", "اتصال امن کاربر وب", "درخواست مجوز کوتاه‌مدت برای خواندن اطلاعات مرکزی با تأیید کاربر مجاز در پنل وب.", order++);
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
