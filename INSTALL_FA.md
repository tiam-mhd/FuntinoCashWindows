# راهنمای ساخت و نصب فانتینو

## آماده‌سازی بسته‌ها

۱. رسانه‌های آفلاین رسمی را طبق جدول `FuntinoControlCenter/Build/Prerequisites.md` در مسیرهای دقیق قرار دهید.
۲. نصب‌گر به فایل‌های دیتابیس مرجع نیاز ندارد و آن‌ها را بسته‌بندی یا attach نمی‌کند. دو catalog عملیاتی باید جداگانه و پس از انتقال بازبینی‌شده آماده شده باشند.
۳. روی رایانه‌ی سازنده، Visual Studio یا Build Tools با .NET Framework 4.6.1 Developer Pack و Inno Setup 6 نصب باشد.
۴. در PowerShell، از ریشه‌ی مخزن، اجرا کنید:

```powershell
Set-Location FuntinoControlCenter\Build
./Build-Installer.ps1
```

شش نصب‌گر نسخه‌دار و manifest هش SHA-256 در `FuntinoControlCenter/Build/Output` ساخته می‌شوند. نسخه‌ی جاری در `FuntinoControlCenter/Build/Version.props` است؛ برای انتشار بعدی آن را تغییر دهید. رسانه‌های پیش‌نیاز و خروجی در Git نادیده گرفته شده‌اند. این مخزن ابزار انتقال دیتابیس مرجع به catalog عملیاتی را اجرا نمی‌کند.

## انتخاب نصب‌گر

| فایل نصب | ویندوز و معماری | پایگاه‌داده |
|---|---|---|
| `FuntinoControlCenter-Setup-Windows7-x86-<version>.exe` | Windows 7 SP1 x86 | SQL Server Express 2014 SP3 |
| `FuntinoControlCenter-Setup-Windows7-x64-<version>.exe` | Windows 7 SP1 x64 | SQL Server Express 2014 SP3 |
| `FuntinoControlCenter-Setup-Windows8-x64-<version>.exe` | Windows 8 x64 | SQL Server Express 2017 |
| `FuntinoControlCenter-Setup-Windows8.1-x64-<version>.exe` | Windows 8.1 x64 | SQL Server Express 2017 |
| `FuntinoControlCenter-Setup-Windows10-x64-<version>.exe` | Windows 10 x64 | SQL Server Express 2019 |
| `FuntinoControlCenter-Setup-Windows11-x64-<version>.exe` | Windows 11 x64 | SQL Server Express 2019 |

نصب‌گر مقصد را بررسی می‌کند و اجازه نمی‌دهد بسته‌ی یک ویندوز یا معماری اشتباه اجرا شود. نصب بدون اینترنت انجام می‌شود. فقط بسته‌ی Windows 8.1 شامل به‌روزرسانی حجیم KB2919355 است؛ بسته‌های Windows 7 نیز بر اساس معماری از هم جدا هستند.

## نصب روی رایانه‌ی مقصد

۱. فایل متناسب با ویندوز و معماری را با دسترسی Administrator اجرا کنید.
۲. حساب ویندوزی‌ای را که قرار است برنامه را اجرا کند وارد یا تأیید کنید.
۳. نصب‌گر پیش‌نیازهای همراه همان بسته را نصب می‌کند و Instance محلی SQL را با نام `FuntinoSqlServer` می‌سازد.
۴. اگر نصب پیش‌نیازی درخواست Restart ویندوز کرد، رایانه را راه‌اندازی مجدد کنید و همان فایل نصب را دوباره اجرا کنید تا مراحل باقی‌مانده ادامه یابند.
۵. نصب‌گر فقط وجود `FuntinoParkOperations` و `FuntinoDeviceOperations` را در Instance بررسی می‌کند و به حساب ویندوزی انتخاب‌شده دسترسی می‌دهد. اگر هرکدام وجود نداشته باشد نصب متوقف می‌شود؛ Setup هیچ فایل مرجعی را attach یا migrate نمی‌کند.
۶. در اولین اجرای برنامه، migrationهای نسخه‌دار فقط روی همین دو catalog عملیاتی اجرا می‌شوند و نتیجه در تاریخچه‌ی همان پایگاه ثبت می‌شود.
۷. برنامه حساب مدیر و جداول داخلی خودش را در schema با نام `app` ایجاد می‌کند و گذرواژه‌ی یک‌بارمصرف مدیر را نشان می‌دهد.

## پایگاه‌های داده

نام‌های ثابت پایگاه عملیاتی `FuntinoParkOperations` و `FuntinoDeviceOperations` هستند. انتقال اولیه از پایگاه مرجع باید جدا از Setup انجام شود و پیش از اجرا به dry-run، checksum، شمارش رکوردها، بررسی تکراری‌ها، برنامه‌ی پشتیبان‌گیری و گزارش قابل‌بازبینی نیاز دارد. انتقال یا migration روی production در فرایند ساخت و نصب این نسخه اجرا نشده است. حذف برنامه، SQL Server و پایگاه‌های داده را حذف نمی‌کند.

## نکته‌ی پشتیبانی

برای Windows 7 از SQL Server 2014 SP3 استفاده می‌شود که پشتیبانی امنیتی Microsoft آن در ژوئیه‌ی ۲۰۲۴ پایان یافته است. Windows 7، Windows 8 و Windows 8.1 نیز دیگر به‌روزرسانی امنیتی دریافت نمی‌کنند.
