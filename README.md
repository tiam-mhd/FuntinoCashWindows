# فانتینو | مرکز کنترل ویندوز

سورس برنامه‌ی Windows Forms و فرایند بیلد ویندوز در این مخزن قرار دارد.

## ساخت

- پروژه با .NET Framework 4.8 و هدف x86 ساخته می‌شود.
- هر push به شاخه‌ی `main`، بیلد Release ویندوز را اجرا می‌کند.
- ZIP بیلد را از بخش **Actions → Windows build → Artifacts** دریافت کنید.
- برای ساخت محلی، Visual Studio یا Build Tools همراه .NET Framework 4.8 Developer Pack را نصب کنید و `FuntinoControlCenter/Build/Build-Windows.cmd` را اجرا کنید.

## دیتابیس‌ها

برنامه در اجرای عادی فقط به دو کاتالوگ `SharjReportDB0216_New` و `DBKart_New` وصل می‌شود. فایل‌های تحویلی با نام `_New` هستند و باید با همین نام‌ها در SQL Server Attach شوند. دیتابیس‌های مرجع در اجرای عادی استفاده نمی‌شوند.

برای راه‌اندازی کامل، ZIP تحویلی را استخراج کنید، `README_FA.md` را بخوانید و فایل‌های پوشه‌ی `Databases` را Attach کنید. نام SQL Server را در صفحه‌ی تنظیمات برنامه وارد کنید.
