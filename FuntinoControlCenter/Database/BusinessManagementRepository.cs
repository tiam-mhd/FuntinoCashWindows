using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using FuntinoControlCenter.Security;
using FuntinoControlCenter.Services;

namespace FuntinoControlCenter.Database
{
    internal enum BusinessManagementArea
    {
        Attractions,
        ControllerDevices,
        Promotions,
        DiscountPolicies,
        DiscountRates,
        Deposits,
        RechargeLimit,
        HourlyPlans,
        Schedules,
        Tickets,
        DevicePromotions
    }

    internal sealed class BusinessManagementRecord
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string Name { get; set; }
        public long Amount { get; set; }
        public int Percent { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public int Weekday { get; set; }
        public long Minimum { get; set; }
        public long Maximum { get; set; }
        public bool IsActive { get; set; }
        public bool IsHalfPrice { get; set; }
        public int PricingType { get; set; }
        public int Duration { get; set; }
        public int Extra1 { get; set; }
        public int Extra2 { get; set; }
    }

    internal sealed class RechargePromotionOption
    {
        public int Code { get; set; }
        public string Name { get; set; }
        public int DiscountPercent { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public override string ToString()
        {
            if (Code < 0) return Name;
            return Name + "  |  " + DiscountPercent.ToString(CultureInfo.InvariantCulture) + "٪";
        }
    }

    internal sealed class RechargeDiscountBand
    {
        public int TierId { get; set; }
        public int DiscountPercent { get; set; }
        public long Minimum { get; set; }
        public long Maximum { get; set; }
        public int PolicyId { get; set; }
    }

    internal sealed class RechargeOperationSetup
    {
        public long MaximumAmount { get; set; }
        public List<RechargePromotionOption> Promotions { get; private set; }
        public List<RechargeDiscountBand> Discounts { get; private set; }

        public RechargeOperationSetup()
        {
            Promotions = new List<RechargePromotionOption>();
            Discounts = new List<RechargeDiscountBand>();
        }

        public RechargeDiscountBand FindDiscount(long amount)
        {
            return Discounts.Where(x => amount >= x.Minimum && amount <= x.Maximum)
                .OrderBy(x => x.Maximum - x.Minimum)
                .ThenByDescending(x => x.Minimum)
                .ThenBy(x => x.PolicyId)
                .FirstOrDefault();
        }
    }

    internal sealed class RechargeOperationTerms
    {
        public long MaximumAmount { get; set; }
        public int PromotionCode { get; set; }
        public int PromotionDiscountPercent { get; set; }
        public int DiscountTierId { get; set; }
        public int DailyDiscountPercent { get; set; }
        public string PromotionName { get; set; }
    }

    internal static class BusinessManagementRepository
    {
        public static bool HasAccess(BusinessManagementArea area, string action)
        {
            if (!AuthorizationGuard.Has(PermissionStem(area) + "." + action)) return false;
            switch (area)
            {
                case BusinessManagementArea.Attractions: return AuthorizationGuard.Has(PermissionCatalog.TableCode("reference", DataAction(action)));
                case BusinessManagementArea.ControllerDevices: return AuthorizationGuard.Has(PermissionCatalog.TableCode("device", DataAction(action)));
                case BusinessManagementArea.Promotions:
                    return AuthorizationGuard.Has(PermissionCatalog.TableCode("pricing", DataAction(action))) &&
                        AuthorizationGuard.Has(PermissionCatalog.TableCode("reference", DataAction(action)));
                case BusinessManagementArea.RechargeLimit:
                case BusinessManagementArea.HourlyPlans:
                case BusinessManagementArea.Schedules: return AuthorizationGuard.Has(PermissionCatalog.TableCode("pricing", DataAction(action)));
                case BusinessManagementArea.DiscountPolicies:
                    return AuthorizationGuard.Has("operations.discount_rates.view") && AuthorizationGuard.Has(PermissionCatalog.TableCode("pricing", DataAction(action)));
                case BusinessManagementArea.DiscountRates: return AuthorizationGuard.Has(PermissionCatalog.TableCode("pricing", DataAction(action)));
                case BusinessManagementArea.Deposits: return AuthorizationGuard.Has(PermissionCatalog.TableCode("configuration", DataAction(action)));
                case BusinessManagementArea.Tickets: return AuthorizationGuard.Has(PermissionCatalog.TableCode("ticket", DataAction(action)));
                case BusinessManagementArea.DevicePromotions:
                    return AuthorizationGuard.Has("operations.promotions.assign_devices") &&
                        AuthorizationGuard.Has("operations.devices.view") &&
                        AuthorizationGuard.Has(PermissionCatalog.TableCode("pricing", "view")) &&
                        AuthorizationGuard.Has("operations.promotions.view") &&
                        AuthorizationGuard.Has(PermissionCatalog.TableCode("device", DataAction(action))) &&
                        AuthorizationGuard.Has(PermissionCatalog.TableCode("reference", "view"));
                default: return false;
            }
        }

        public static DataTable Load(BusinessManagementArea area)
        {
            RequireArea(area, "view");
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                return DatabaseManager.Query(connection, QueryFor(area));
            }
        }

        public static void RequireAccess(BusinessManagementArea area, string action)
        {
            RequireArea(area, action);
        }

        public static RechargeOperationSetup LoadRechargeOperationSetup()
        {
            AuthorizationGuard.Require("cards.recharge");
            AuthorizationGuard.Require("operations.recharge_terms.view");
            AuthorizationGuard.Require(PermissionCatalog.TableCode("finance", "create"));

            var result = new RechargeOperationSetup();
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT TOP(1) LimitValue FROM pricing.RechargeLimit ORDER BY RechargeLimitId DESC", connection))
                {
                    object value = command.ExecuteScalar();
                    if (value == null || value == DBNull.Value || Convert.ToInt64(value, CultureInfo.InvariantCulture) <= 0)
                        throw new InvalidOperationException("سقف مجاز شارژ در تنظیمات تعریف نشده است؛ تا تعیین سقف، شارژ انجام نمی‌شود.");
                    result.MaximumAmount = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                }

                using (var command = new SqlCommand(@"
SELECT p.PromotionCode,p.PromotionName,p.DiscountPercent,p.StartsAt,p.EndsAt
FROM pricing.PromotionEvent p
WHERE p.StartsAt<=@now AND p.EndsAt>=@now
  AND EXISTS
  (
   SELECT 1 FROM FuntinoDeviceOperations.reference.PromotionEvent d
   WHERE d.PromotionCode=p.PromotionCode AND d.PromotionName=p.PromotionName
     AND d.DiscountPercent=p.DiscountPercent AND d.StartsAt=p.StartsAt AND d.EndsAt=p.EndsAt
  )
  AND (SELECT COUNT(1) FROM FuntinoDeviceOperations.reference.PromotionEvent d WHERE d.PromotionCode=p.PromotionCode)=1
ORDER BY p.StartsAt,p.PromotionCode;", connection))
                {
                    command.Parameters.Add("@now", SqlDbType.DateTime).Value = DateTime.Now;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Promotions.Add(new RechargePromotionOption
                            {
                                Code = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                DiscountPercent = reader.GetInt32(2),
                                StartsAt = reader.GetDateTime(3),
                                EndsAt = reader.GetDateTime(4)
                            });
                        }
                    }
                }

                int legacyWeekday = ToLegacyWeekday(DateTime.Now.DayOfWeek);
                using (var command = new SqlCommand(@"
SELECT p.DiscountPolicyId,p.DiscountTypeId,r.DiscountPercent,p.MinimumPrice,p.MaximumPrice
FROM pricing.DiscountPolicy p
INNER JOIN pricing.DiscountRule r ON r.DiscountTypeId=p.DiscountTypeId
WHERE p.IsActive=1 AND p.DayOfWeek=@day AND p.MinimumPrice<=@max AND p.MaximumPrice>=0
ORDER BY (p.MaximumPrice-p.MinimumPrice),p.MinimumPrice DESC,p.DiscountPolicyId;", connection))
                {
                    command.Parameters.Add("@day", SqlDbType.Int).Value = legacyWeekday;
                    command.Parameters.Add("@max", SqlDbType.BigInt).Value = result.MaximumAmount;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Discounts.Add(new RechargeDiscountBand
                            {
                                PolicyId = reader.GetInt32(0),
                                TierId = reader.GetInt32(1),
                                DiscountPercent = reader.GetInt32(2),
                                Minimum = reader.GetInt64(3),
                                Maximum = reader.GetInt64(4)
                            });
                        }
                    }
                }
            }

            return result;
        }

        public static RechargeOperationTerms ResolveRechargeTerms(long amount, int promotionCode)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException("amount", "مبلغ شارژ باید بیشتر از صفر باشد.");
            if (amount % 100 != 0) throw new ArgumentOutOfRangeException("amount", "مبلغ شارژ باید مضربی از ۱۰۰ تومان باشد.");

            RechargeOperationSetup setup = LoadRechargeOperationSetup();
            if (amount > setup.MaximumAmount)
                throw new ArgumentOutOfRangeException("amount", "حداکثر مبلغ هر بار شارژ " + setup.MaximumAmount.ToString("N0", CultureInfo.GetCultureInfo("fa-IR")) + " تومان است.");

            RechargePromotionOption selected = null;
            if (promotionCode > 0)
            {
                selected = setup.Promotions.FirstOrDefault(x => x.Code == promotionCode);
                if (selected == null) throw new InvalidOperationException("این جشنواره دیگر فعال نیست یا در دو پایگاه عملیاتی همگام نشده است؛ شرایط شارژ را تازه‌سازی کنید.");
            }
            else if (promotionCode == 0 && setup.Promotions.Count == 1)
            {
                selected = setup.Promotions[0];
            }
            else if (promotionCode == 0 && setup.Promotions.Count > 1)
            {
                throw new InvalidOperationException("چند جشنواره فعال است؛ یکی را از فهرست جشنواره‌ها انتخاب کنید.");
            }

            RechargeDiscountBand band = setup.FindDiscount(amount);
            return new RechargeOperationTerms
            {
                MaximumAmount = setup.MaximumAmount,
                PromotionCode = selected == null ? 0 : selected.Code,
                PromotionDiscountPercent = selected == null ? 0 : selected.DiscountPercent,
                PromotionName = selected == null ? null : selected.Name,
                DiscountTierId = band == null ? 0 : band.TierId,
                DailyDiscountPercent = band == null ? 0 : band.DiscountPercent
            };
        }

        public static void Save(BusinessManagementArea area, BusinessManagementRecord record, bool isNew)
        {
            if (record == null) throw new ArgumentNullException("record");
            RequireArea(area, isNew ? "create" : "update");
            ValidateRecord(area, record);
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        string key = SaveCore(connection, transaction, area, record, isNew);
                        AuditService.Write(connection, transaction, AuditAction(area, isNew ? "create" : "update"), EntityName(area), key,
                            AuditDescription(area, record, isNew));
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static void Deactivate(BusinessManagementArea area, BusinessManagementRecord record)
        {
            if (record == null) throw new ArgumentNullException("record");
            RequireArea(area, "delete");
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        DeactivateCore(connection, transaction, area, record);
                        AuditService.Write(connection, transaction, AuditAction(area, "archive"), EntityName(area), record.Id.ToString(CultureInfo.InvariantCulture),
                            "رکورد غیرفعال شد و سوابق عملیاتی آن باقی ماند.");
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static string SaveCore(SqlConnection connection, SqlTransaction transaction, BusinessManagementArea area, BusinessManagementRecord record, bool isNew)
        {
            switch (area)
            {
                case BusinessManagementArea.Attractions:
                    using (var command = new SqlCommand(isNew ? "business.usp_FuntinoPark_CreateAttractionDevice" : "business.usp_FuntinoPark_UpdateAttractionDevice", connection, transaction))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        if (!isNew) command.Parameters.Add("@id", SqlDbType.Int).Value = record.Id;
                        command.Parameters.Add("@Code", SqlDbType.Int).Value = record.Code;
                        command.Parameters.Add("@Name", SqlDbType.NVarChar, 50).Value = record.Name;
                        command.Parameters.Add("@price", isNew ? SqlDbType.BigInt : SqlDbType.Int).Value = record.Amount;
                        command.Parameters.Add("@Active", SqlDbType.Bit).Value = record.IsActive;
                        command.ExecuteNonQuery();
                    }
                    return isNew ? record.Code.ToString(CultureInfo.InvariantCulture) : record.Id.ToString(CultureInfo.InvariantCulture);

                case BusinessManagementArea.ControllerDevices:
                    if (isNew)
                    {
                        using (var command = new SqlCommand("FuntinoDeviceOperations.device.usp_FuntinoDevice_CreateDevice", connection, transaction))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.Add("@Code", SqlDbType.Int).Value = record.Code;
                            command.Parameters.Add("@Name", SqlDbType.NVarChar, 50).Value = record.Name;
                            command.Parameters.Add("@price", SqlDbType.BigInt).Value = record.Amount;
                            command.Parameters.Add("@PriceType", SqlDbType.Bit).Value = record.PricingType != 0;
                            command.ExecuteNonQuery();
                        }
                        using (var command = new SqlCommand(@"
UPDATE FuntinoDeviceOperations.device.Device
SET IsActive=@active,IsHalfPrice=@halfPrice,Duration=@duration
WHERE ControllerDeviceId=@code;
IF @@ROWCOUNT<>1 RAISERROR(N'دستگاه ساخته شد اما تنظیمات آن به‌طور یکتا ذخیره نشد.',16,1);", connection, transaction))
                        {
                            command.Parameters.Add("@active", SqlDbType.Bit).Value = record.IsActive;
                            command.Parameters.Add("@halfPrice", SqlDbType.Bit).Value = record.IsHalfPrice;
                            command.Parameters.Add("@duration", SqlDbType.Int).Value = record.Duration;
                            command.Parameters.Add("@code", SqlDbType.Int).Value = record.Code;
                            command.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var command = new SqlCommand(@"
UPDATE FuntinoDeviceOperations.device.Device
SET ControllerDeviceId=@code,DeviceName=@name,PricingType=@pricingType,BasePrice=@price,IsActive=@active,
    IsHalfPrice=@halfPrice,Duration=@duration
WHERE DeviceId=@id;
IF @@ROWCOUNT<>1 RAISERROR(N'دستگاه برای ویرایش پیدا نشد.',16,1);", connection, transaction))
                        {
                            command.Parameters.Add("@code", SqlDbType.Int).Value = record.Code;
                            command.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = record.Name;
                            command.Parameters.Add("@pricingType", SqlDbType.Bit).Value = record.PricingType != 0;
                            command.Parameters.Add("@price", SqlDbType.BigInt).Value = record.Amount;
                            command.Parameters.Add("@active", SqlDbType.Bit).Value = record.IsActive;
                            command.Parameters.Add("@halfPrice", SqlDbType.Bit).Value = record.IsHalfPrice;
                            command.Parameters.Add("@duration", SqlDbType.Int).Value = record.Duration;
                            command.Parameters.Add("@id", SqlDbType.Int).Value = record.Id;
                            command.ExecuteNonQuery();
                        }
                    }
                    return isNew ? record.Code.ToString(CultureInfo.InvariantCulture) : record.Id.ToString(CultureInfo.InvariantCulture);

                case BusinessManagementArea.Promotions:
                    return SavePromotion(connection, transaction, record, isNew);

                case BusinessManagementArea.DiscountPolicies:
                    EnsureDiscountTier(connection, transaction, record.Code);
                    EnsureNoDiscountOverlap(connection, transaction, record);
                    using (var command = new SqlCommand(isNew
                        ? "INSERT pricing.DiscountPolicy(DiscountTypeId,DayOfWeek,MinimumPrice,MaximumPrice,IsActive) VALUES(@tier,@day,@minimum,@maximum,@active); SELECT CAST(SCOPE_IDENTITY() AS int);"
                        : "UPDATE pricing.DiscountPolicy SET DiscountTypeId=@tier,DayOfWeek=@day,MinimumPrice=@minimum,MaximumPrice=@maximum,IsActive=@active WHERE DiscountPolicyId=@id; IF @@ROWCOUNT<>1 RAISERROR(N'قانون تخفیف برای ویرایش پیدا نشد.',16,1); SELECT @id;", connection, transaction))
                    {
                        AddDiscountPolicyParameters(command, record);
                        object value = command.ExecuteScalar();
                        if (value == null || value == DBNull.Value) throw new InvalidOperationException("قانون تخفیف ذخیره نشد.");
                        return Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                    }

                case BusinessManagementArea.DiscountRates:
                    using (var command = new SqlCommand("UPDATE pricing.DiscountRule SET DiscountPercent=@percent WHERE DiscountTypeId=@id;", connection, transaction))
                    {
                        command.Parameters.Add("@percent", SqlDbType.TinyInt).Value = record.Percent;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = record.Id;
                        if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("سطح تخفیف پیدا نشد.");
                    }
                    return record.Id.ToString(CultureInfo.InvariantCulture);

                case BusinessManagementArea.Deposits:
                    using (var command = new SqlCommand("business.usp_FuntinoPark_SetDepositAmount", connection, transaction))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@deposite", SqlDbType.Int).Value = record.Amount;
                        if (Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) != 1)
                            throw new InvalidOperationException("مبلغ بیعانه ذخیره نشد؛ ردیف تنظیمات برنامه در پایگاه داده وجود ندارد.");
                    }
                    return "deposit";

                case BusinessManagementArea.RechargeLimit:
                    using (var command = new SqlCommand("INSERT pricing.RechargeLimit(LimitValue) VALUES(@value); SELECT CAST(SCOPE_IDENTITY() AS int);", connection, transaction))
                    {
                        command.Parameters.Add("@value", SqlDbType.Int).Value = record.Amount;
                        object value = command.ExecuteScalar();
                        if (value == null || value == DBNull.Value)
                            throw new InvalidOperationException("سقف شارژ ذخیره نشد.");
                        return Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                    }

                case BusinessManagementArea.HourlyPlans:
                    using (var command = new SqlCommand(@"
UPDATE pricing.HourlyRechargePlan SET RechargeAmount=@amount,IsActive=@active WHERE HourlyRechargePlanId=@id;", connection, transaction))
                    {
                        command.Parameters.Add("@amount", SqlDbType.Int).Value = record.Amount;
                        command.Parameters.Add("@active", SqlDbType.Bit).Value = record.IsActive;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = record.Id;
                        if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("گزینه‌ی شارژ ساعتی پیدا نشد.");
                    }
                    return record.Id.ToString(CultureInfo.InvariantCulture);

                case BusinessManagementArea.Schedules:
                    if (record.IsActive)
                        using (var command = new SqlCommand("UPDATE pricing.RechargeSchedule SET IsActive=0 WHERE RechargeScheduleId<>@id;", connection, transaction))
                        {
                            command.Parameters.Add("@id", SqlDbType.Int).Value = isNew ? -1 : record.Id;
                            command.ExecuteNonQuery();
                        }
                    using (var command = new SqlCommand(isNew
                        ? "INSERT pricing.RechargeSchedule(ScheduledAt,IsActive) VALUES(@time,@active); SELECT CAST(SCOPE_IDENTITY() AS int);"
                        : "UPDATE pricing.RechargeSchedule SET ScheduledAt=@time,IsActive=@active WHERE RechargeScheduleId=@id; IF @@ROWCOUNT<>1 RAISERROR(N'زمان‌بندی شارژ برای ویرایش پیدا نشد.',16,1); SELECT @id;", connection, transaction))
                    {
                        command.Parameters.Add("@time", SqlDbType.DateTime).Value = record.StartsAt;
                        command.Parameters.Add("@active", SqlDbType.Bit).Value = record.IsActive;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = record.Id;
                        object value = command.ExecuteScalar();
                        if (value == null || value == DBNull.Value) throw new InvalidOperationException("زمان‌بندی شارژ پیدا نشد.");
                        return Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                    }

                case BusinessManagementArea.Tickets:
                    if (record.IsActive)
                        using (var command = new SqlCommand("UPDATE ticket.TicketSetting SET IsActive=0 WHERE TicketSettingId<>@id;", connection, transaction))
                        {
                            command.Parameters.Add("@id", SqlDbType.Int).Value = isNew ? -1 : record.Id;
                            command.ExecuteNonQuery();
                        }
                    using (var command = new SqlCommand(isNew
                        ? "INSERT ticket.TicketSetting(TicketValue,IsActive) VALUES(@value,@active); SELECT CAST(SCOPE_IDENTITY() AS int);"
                        : "UPDATE ticket.TicketSetting SET TicketValue=@value,IsActive=@active WHERE TicketSettingId=@id; IF @@ROWCOUNT<>1 RAISERROR(N'تنظیم بلیت برای ویرایش پیدا نشد.',16,1); SELECT @id;", connection, transaction))
                    {
                        command.Parameters.Add("@value", SqlDbType.Int).Value = record.Amount;
                        command.Parameters.Add("@active", SqlDbType.Bit).Value = record.IsActive;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = record.Id;
                        object value = command.ExecuteScalar();
                        if (value == null || value == DBNull.Value) throw new InvalidOperationException("تنظیم بلیت پیدا نشد.");
                        return Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                    }

                case BusinessManagementArea.DevicePromotions:
                    return SaveDevicePromotion(connection, transaction, record, isNew);

                default: throw new ArgumentOutOfRangeException("area");
            }
        }

        private static string SavePromotion(SqlConnection connection, SqlTransaction transaction, BusinessManagementRecord record, bool isNew)
        {
            if (isNew)
            {
                using (var command = new SqlCommand(@"
IF EXISTS(SELECT 1 FROM pricing.PromotionEvent WHERE PromotionCode=@code)
 OR EXISTS(SELECT 1 FROM FuntinoDeviceOperations.reference.PromotionEvent WHERE PromotionCode=@code)
BEGIN
    RAISERROR(N'کد جشنواره در حال حاضر استفاده شده است.',16,1);
    RETURN;
END;
INSERT pricing.PromotionEvent(PromotionCode,PromotionName,StartsAt,EndsAt,DiscountPercent)
VALUES(@code,@name,@startsAt,@endsAt,@percent);
INSERT FuntinoDeviceOperations.reference.PromotionEvent(PromotionCode,PromotionName,StartsAt,EndsAt,DiscountPercent)
VALUES(@code,@name,@startsAt,@endsAt,@percent);", connection, transaction))
                {
                    AddPromotionParameters(command, record);
                    command.ExecuteNonQuery();
                }
                return record.Code.ToString(CultureInfo.InvariantCulture);
            }

            int oldCode;
            using (var command = new SqlCommand("SELECT PromotionCode FROM pricing.PromotionEvent WHERE PromotionEventId=@id", connection, transaction))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = record.Id;
                object value = command.ExecuteScalar();
                if (value == null || value == DBNull.Value) throw new InvalidOperationException("جشنواره برای ویرایش پیدا نشد.");
                oldCode = Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }
            using (var command = new SqlCommand(@"
IF @oldCode<>@code AND (EXISTS(SELECT 1 FROM pricing.PromotionEvent WHERE PromotionCode=@code)
 OR EXISTS(SELECT 1 FROM FuntinoDeviceOperations.reference.PromotionEvent WHERE PromotionCode=@code))
BEGIN
    RAISERROR(N'کد تازه‌ی جشنواره قبلاً استفاده شده است.',16,1);
    RETURN;
END;
IF (SELECT COUNT(1) FROM FuntinoDeviceOperations.reference.PromotionEvent WHERE PromotionCode=@oldCode)<>1
BEGIN
    RAISERROR(N'جشنواره‌ی متناظر در پایگاه دستگاه‌ها یکتا نیست؛ ابتدا همگام‌سازی داده‌ها را بررسی کنید.',16,1);
    RETURN;
END;
IF (SELECT COUNT(1) FROM pricing.PromotionEvent WHERE PromotionCode=@oldCode)<>1
BEGIN
    RAISERROR(N'کد جشنواره در پایگاه اصلی یکتا نیست؛ ابتدا داده‌های جشنواره را بررسی کنید.',16,1);
    RETURN;
END;
UPDATE pricing.PromotionEvent SET PromotionCode=@code,PromotionName=@name,StartsAt=@startsAt,EndsAt=@endsAt,DiscountPercent=@percent
WHERE PromotionEventId=@id;
IF @@ROWCOUNT<>1
BEGIN
    RAISERROR(N'جشنواره در پایگاه اصلی برای ویرایش پیدا نشد.',16,1);
    RETURN;
END;
UPDATE FuntinoDeviceOperations.reference.PromotionEvent
SET PromotionCode=@code,PromotionName=@name,StartsAt=@startsAt,EndsAt=@endsAt,DiscountPercent=@percent
WHERE PromotionCode=@oldCode;
IF @@ROWCOUNT<>1
BEGIN
    RAISERROR(N'جشنواره در پایگاه دستگاه‌ها یکتا نیست؛ تغییر انجام نشد.',16,1);
    RETURN;
END;", connection, transaction))
            {
                AddPromotionParameters(command, record);
                command.Parameters.Add("@id", SqlDbType.Int).Value = record.Id;
                command.Parameters.Add("@oldCode", SqlDbType.Int).Value = oldCode;
                command.ExecuteNonQuery();
            }
            return record.Code.ToString(CultureInfo.InvariantCulture);
        }

        private static string SaveDevicePromotion(SqlConnection connection, SqlTransaction transaction, BusinessManagementRecord record, bool isNew)
        {
            int deviceId = record.Extra1;
            int promotionCode = record.Extra2;
            using (var command = new SqlCommand(@"
DECLARE @devicePromotionId int;
IF NOT EXISTS(SELECT 1 FROM FuntinoDeviceOperations.device.Device WHERE DeviceId=@deviceId AND IsActive=1)
BEGIN
    RAISERROR(N'دستگاه انتخاب‌شده فعال نیست یا پیدا نشد.',16,1);
    RETURN;
END;
SELECT @devicePromotionId=d.PromotionEventId
FROM pricing.PromotionEvent p
INNER JOIN FuntinoDeviceOperations.reference.PromotionEvent d
  ON d.PromotionCode=p.PromotionCode AND d.PromotionName=p.PromotionName
   AND d.DiscountPercent=p.DiscountPercent AND d.StartsAt=p.StartsAt AND d.EndsAt=p.EndsAt
WHERE p.PromotionCode=@promotionCode AND p.EndsAt>=GETDATE()
  AND (SELECT COUNT(1) FROM pricing.PromotionEvent x WHERE x.PromotionCode=p.PromotionCode)=1
  AND (SELECT COUNT(1) FROM FuntinoDeviceOperations.reference.PromotionEvent x WHERE x.PromotionCode=p.PromotionCode)=1;
IF @devicePromotionId IS NULL
BEGIN
    RAISERROR(N'جشنواره‌ی انتخاب‌شده پایان یافته، پیدا نشد یا در دو پایگاه همگام نیست.',16,1);
    RETURN;
END;
IF EXISTS(SELECT 1 FROM FuntinoDeviceOperations.device.DevicePromotion WHERE DeviceId=@deviceId AND PromotionEventId=@devicePromotionId AND (@isNew=1 OR DevicePromotionId<>@id))
BEGIN
    RAISERROR(N'این جشنواره از قبل به این دستگاه تخصیص داده شده است؛ همان ردیف را ویرایش کنید.',16,1);
    RETURN;
END;
IF @isNew=1
    INSERT FuntinoDeviceOperations.device.DevicePromotion(DeviceId,PromotionEventId,IsActive) VALUES(@deviceId,@devicePromotionId,@active);
ELSE
BEGIN
    UPDATE FuntinoDeviceOperations.device.DevicePromotion
    SET DeviceId=@deviceId,PromotionEventId=@devicePromotionId,IsActive=@active
    WHERE DevicePromotionId=@id;
    IF @@ROWCOUNT<>1 RAISERROR(N'تخصیص جشنواره برای ویرایش پیدا نشد.',16,1);
END;", connection, transaction))
            {
                command.Parameters.Add("@deviceId", SqlDbType.Int).Value = deviceId;
                command.Parameters.Add("@promotionCode", SqlDbType.Int).Value = promotionCode;
                command.Parameters.Add("@active", SqlDbType.Bit).Value = record.IsActive;
                command.Parameters.Add("@isNew", SqlDbType.Bit).Value = isNew;
                command.Parameters.Add("@id", SqlDbType.Int).Value = record.Id;
                command.ExecuteNonQuery();
            }
            return deviceId.ToString(CultureInfo.InvariantCulture) + ":" + promotionCode.ToString(CultureInfo.InvariantCulture);
        }

        private static void DeactivateCore(SqlConnection connection, SqlTransaction transaction, BusinessManagementArea area, BusinessManagementRecord record)
        {
            string sql;
            switch (area)
            {
                case BusinessManagementArea.Attractions:
                    sql = "UPDATE reference.AttractionDevice SET IsActive=0 WHERE AttractionDeviceId=@id; IF @@ROWCOUNT<>1 RAISERROR(N'دستگاه تفریحی برای غیرفعال‌کردن پیدا نشد.',16,1);";
                    break;
                case BusinessManagementArea.ControllerDevices:
                    sql = "UPDATE FuntinoDeviceOperations.device.Device SET IsActive=0 WHERE DeviceId=@id; IF @@ROWCOUNT<>1 RAISERROR(N'دستگاه کنترل‌شونده برای غیرفعال‌کردن پیدا نشد.',16,1);";
                    break;
                case BusinessManagementArea.Promotions:
                    int code = record.Code;
                    using (var command = new SqlCommand(@"
UPDATE pricing.PromotionEvent SET EndsAt=CASE WHEN EndsAt>GETDATE() THEN DATEADD(second,-1,GETDATE()) ELSE EndsAt END WHERE PromotionEventId=@id;
IF @@ROWCOUNT<>1
BEGIN
    RAISERROR(N'جشنواره در پایگاه اصلی پیدا نشد.',16,1);
    RETURN;
END;
UPDATE FuntinoDeviceOperations.reference.PromotionEvent SET EndsAt=CASE WHEN EndsAt>GETDATE() THEN DATEADD(second,-1,GETDATE()) ELSE EndsAt END WHERE PromotionCode=@code;
IF @@ROWCOUNT<>1
BEGIN
    RAISERROR(N'جشنواره در پایگاه دستگاه‌ها یکتا نیست.',16,1);
    RETURN;
END;", connection, transaction))
                    {
                        command.Parameters.Add("@id", SqlDbType.Int).Value = record.Id;
                        command.Parameters.Add("@code", SqlDbType.Int).Value = code;
                        command.ExecuteNonQuery();
                    }
                    return;
                case BusinessManagementArea.DiscountPolicies:
                    sql = "UPDATE pricing.DiscountPolicy SET IsActive=0 WHERE DiscountPolicyId=@id; IF @@ROWCOUNT<>1 RAISERROR(N'قانون تخفیف برای غیرفعال‌کردن پیدا نشد.',16,1);";
                    break;
                case BusinessManagementArea.Schedules:
                    sql = "UPDATE pricing.RechargeSchedule SET IsActive=0 WHERE RechargeScheduleId=@id; IF @@ROWCOUNT<>1 RAISERROR(N'زمان‌بندی شارژ برای غیرفعال‌کردن پیدا نشد.',16,1);";
                    break;
                case BusinessManagementArea.Tickets:
                    sql = "UPDATE ticket.TicketSetting SET IsActive=0 WHERE TicketSettingId=@id; IF @@ROWCOUNT<>1 RAISERROR(N'تنظیم بلیت برای غیرفعال‌کردن پیدا نشد.',16,1);";
                    break;
                case BusinessManagementArea.DevicePromotions:
                    sql = "UPDATE FuntinoDeviceOperations.device.DevicePromotion SET IsActive=0 WHERE DevicePromotionId=@id; IF @@ROWCOUNT<>1 RAISERROR(N'تخصیص جشنواره برای غیرفعال‌کردن پیدا نشد.',16,1);";
                    break;
                default:
                    throw new InvalidOperationException("این نوع تنظیم حذف‌پذیر نیست.");
            }
            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = record.Id;
                command.ExecuteNonQuery();
            }
        }

        private static string QueryFor(BusinessManagementArea area)
        {
            switch (area)
            {
                case BusinessManagementArea.Attractions:
                    return "SELECT AttractionDeviceId AS RecordId,AttractionCode AS Code,AttractionName AS Name,CONVERT(bigint,BasePrice) AS Amount,CONVERT(int,NULL) AS [Percent],CONVERT(datetime,NULL) AS StartsAt,CONVERT(datetime,NULL) AS EndsAt,CONVERT(int,NULL) AS Weekday,CONVERT(bigint,NULL) AS Minimum,CONVERT(bigint,NULL) AS Maximum,IsActive AS Active,CAST(N'دستگاه شهربازی' AS nvarchar(300)) AS Detail,CONVERT(int,NULL) AS Extra1,CONVERT(int,NULL) AS Extra2 FROM reference.AttractionDevice ORDER BY AttractionName,AttractionCode";
                case BusinessManagementArea.ControllerDevices:
                    return "SELECT DeviceId AS RecordId,ControllerDeviceId AS Code,DeviceName AS Name,BasePrice AS Amount,CONVERT(int,PricingType) AS [Percent],CONVERT(datetime,NULL) AS StartsAt,CONVERT(datetime,NULL) AS EndsAt,CONVERT(int,NULL) AS Weekday,CONVERT(bigint,NULL) AS Minimum,CONVERT(bigint,NULL) AS Maximum,IsActive AS Active,CAST(CASE WHEN ISNULL(IsHalfPrice,0)=1 THEN N'نیم‌بها' ELSE N'قیمت معمولی' END+CASE WHEN ISNULL(Duration,0)>0 THEN N' | مدت '+CONVERT(nvarchar(20),Duration) ELSE N'' END AS nvarchar(300)) AS Detail,CONVERT(int,ISNULL(IsHalfPrice,0)) AS Extra1,ISNULL(Duration,0) AS Extra2 FROM FuntinoDeviceOperations.device.Device ORDER BY DeviceName,ControllerDeviceId";
                case BusinessManagementArea.Promotions:
                    return @"SELECT p.PromotionEventId AS RecordId,p.PromotionCode AS Code,p.PromotionName AS Name,CONVERT(bigint,NULL) AS Amount,p.DiscountPercent AS [Percent],p.StartsAt,p.EndsAt,CONVERT(int,NULL) AS Weekday,CONVERT(bigint,NULL) AS Minimum,CONVERT(bigint,NULL) AS Maximum,CONVERT(bit,CASE WHEN p.EndsAt>=GETDATE() THEN 1 ELSE 0 END) AS Active,
CAST(CASE WHEN ISNULL(d.EventCount,0)=0 THEN N'در پایگاه دستگاه‌ها ثبت نشده' WHEN d.EventCount<>1 THEN N'ردیف‌های تکراری در پایگاه دستگاه‌ها' WHEN ISNULL(d.PromotionName,N'')<>ISNULL(p.PromotionName,N'') OR ISNULL(d.DiscountPercent,-1)<>ISNULL(p.DiscountPercent,-1) OR ISNULL(d.StartsAt,CONVERT(datetime,'19000101'))<>ISNULL(p.StartsAt,CONVERT(datetime,'19000101')) OR ISNULL(d.EndsAt,CONVERT(datetime,'19000101'))<>ISNULL(p.EndsAt,CONVERT(datetime,'19000101')) THEN N'اطلاعات دو پایگاه یکسان نیست' WHEN p.EndsAt<GETDATE() THEN N'پایان‌یافته' WHEN p.StartsAt>GETDATE() THEN N'زمان‌بندی‌شده و همگام' ELSE N'فعال و همگام' END AS nvarchar(300)) AS Detail,
CAST(CASE WHEN d.EventCount=1 AND ISNULL(d.PromotionName,N'')=ISNULL(p.PromotionName,N'') AND ISNULL(d.DiscountPercent,-1)=ISNULL(p.DiscountPercent,-1) AND ISNULL(d.StartsAt,CONVERT(datetime,'19000101'))=ISNULL(p.StartsAt,CONVERT(datetime,'19000101')) AND ISNULL(d.EndsAt,CONVERT(datetime,'19000101'))=ISNULL(p.EndsAt,CONVERT(datetime,'19000101')) THEN 1 ELSE 0 END AS bit) AS Synchronized,
CONVERT(int,NULL) AS Extra1,CONVERT(int,NULL) AS Extra2 FROM pricing.PromotionEvent p LEFT JOIN
(SELECT PromotionCode,COUNT(1) AS EventCount,MAX(PromotionName) AS PromotionName,MAX(DiscountPercent) AS DiscountPercent,MAX(StartsAt) AS StartsAt,MAX(EndsAt) AS EndsAt FROM FuntinoDeviceOperations.reference.PromotionEvent GROUP BY PromotionCode) d
ON d.PromotionCode=p.PromotionCode ORDER BY p.StartsAt DESC,p.PromotionCode";
                case BusinessManagementArea.DiscountPolicies:
                    return "SELECT p.DiscountPolicyId AS RecordId,p.DiscountTypeId AS Code,CAST(N'قانون روزانه' AS nvarchar(300)) AS Name,CONVERT(bigint,NULL) AS Amount,r.DiscountPercent AS [Percent],CONVERT(datetime,NULL) AS StartsAt,CONVERT(datetime,NULL) AS EndsAt,p.DayOfWeek AS Weekday,CONVERT(bigint,p.MinimumPrice) AS Minimum,CONVERT(bigint,p.MaximumPrice) AS Maximum,p.IsActive AS Active,CAST(CASE WHEN r.DiscountTypeId IS NULL THEN N'سطح تخفیف تعریف نشده' WHEN (SELECT COUNT(1) FROM pricing.DiscountPolicy x WHERE x.IsActive=1 AND x.DayOfWeek=p.DayOfWeek AND x.DiscountPolicyId<>p.DiscountPolicyId AND x.MinimumPrice<=p.MaximumPrice AND x.MaximumPrice>=p.MinimumPrice)>0 THEN N'بازه با قانون فعال دیگری هم‌پوشانی دارد' ELSE N'' END AS nvarchar(300)) AS Detail,CONVERT(int,NULL) AS Extra1,CONVERT(int,NULL) AS Extra2 FROM pricing.DiscountPolicy p LEFT JOIN pricing.DiscountRule r ON r.DiscountTypeId=p.DiscountTypeId ORDER BY p.DayOfWeek,p.MinimumPrice,p.DiscountPolicyId";
                case BusinessManagementArea.DiscountRates:
                    return "SELECT DiscountTypeId AS RecordId,DiscountTypeId AS Code,CAST(N'سطح '+CONVERT(nvarchar(10),DiscountTypeId) AS nvarchar(300)) AS Name,CONVERT(bigint,NULL) AS Amount,CONVERT(int,DiscountPercent) AS [Percent],CONVERT(datetime,NULL) AS StartsAt,CONVERT(datetime,NULL) AS EndsAt,CONVERT(int,NULL) AS Weekday,CONVERT(bigint,NULL) AS Minimum,CONVERT(bigint,NULL) AS Maximum,CONVERT(bit,1) AS Active,CAST(N'درصد اعمال‌شده هنگام شارژ' AS nvarchar(300)) AS Detail,CONVERT(int,NULL) AS Extra1,CONVERT(int,NULL) AS Extra2 FROM pricing.DiscountRule ORDER BY DiscountTypeId";
                case BusinessManagementArea.Deposits:
                    return "SELECT CONVERT(int,0) AS RecordId,CONVERT(int,NULL) AS Code,CAST(N'مبلغ بیعانه' AS nvarchar(300)) AS Name,CONVERT(bigint,MAX(DepositAmount)) AS Amount,CONVERT(int,NULL) AS [Percent],CONVERT(datetime,NULL) AS StartsAt,CONVERT(datetime,NULL) AS EndsAt,CONVERT(int,NULL) AS Weekday,CONVERT(bigint,NULL) AS Minimum,CONVERT(bigint,NULL) AS Maximum,CONVERT(bit,1) AS Active,CAST(CASE WHEN COUNT(DISTINCT DepositAmount)>1 THEN N'مقادیر ردیف‌های تنظیمات یکسان نیستند' ELSE N'در همه‌ی ردیف‌های تنظیمات برنامه ثبت می‌شود' END AS nvarchar(300)) AS Detail,CONVERT(int,COUNT(1)) AS Extra1,CONVERT(int,NULL) AS Extra2 FROM configuration.ApplicationConfiguration WHERE DepositAmount IS NOT NULL HAVING COUNT(1)>0";
                case BusinessManagementArea.RechargeLimit:
                    return "SELECT TOP(1) RechargeLimitId AS RecordId,CONVERT(int,NULL) AS Code,CAST(N'سقف هر بار شارژ' AS nvarchar(300)) AS Name,CONVERT(bigint,LimitValue) AS Amount,CONVERT(int,NULL) AS [Percent],CONVERT(datetime,NULL) AS StartsAt,CONVERT(datetime,NULL) AS EndsAt,CONVERT(int,NULL) AS Weekday,CONVERT(bigint,NULL) AS Minimum,CONVERT(bigint,NULL) AS Maximum,CONVERT(bit,1) AS Active,CAST(N'این مقدار در عملیات کارت کنترل می‌شود؛ تغییر سقف، یک مقدار تازه ثبت می‌کند' AS nvarchar(300)) AS Detail,CONVERT(int,NULL) AS Extra1,CONVERT(int,NULL) AS Extra2 FROM pricing.RechargeLimit ORDER BY RechargeLimitId DESC";
                case BusinessManagementArea.HourlyPlans:
                    return "SELECT HourlyRechargePlanId AS RecordId,CONVERT(int,NULL) AS Code,PlanName AS Name,CONVERT(bigint,RechargeAmount) AS Amount,CONVERT(int,NULL) AS [Percent],CONVERT(datetime,NULL) AS StartsAt,CONVERT(datetime,NULL) AS EndsAt,CONVERT(int,NULL) AS Weekday,CONVERT(bigint,NULL) AS Minimum,CONVERT(bigint,NULL) AS Maximum,IsActive AS Active,CAST(N'تنظیم ثابت سامانه‌ی شارژ ساعتی' AS nvarchar(300)) AS Detail,CONVERT(int,NULL) AS Extra1,CONVERT(int,NULL) AS Extra2 FROM pricing.HourlyRechargePlan ORDER BY HourlyRechargePlanId";
                case BusinessManagementArea.Schedules:
                    return "SELECT RechargeScheduleId AS RecordId,CONVERT(int,NULL) AS Code,CAST(N'زمان شارژ برنامه‌ریزی‌شده' AS nvarchar(300)) AS Name,CONVERT(bigint,NULL) AS Amount,CONVERT(int,NULL) AS [Percent],ScheduledAt AS StartsAt,CONVERT(datetime,NULL) AS EndsAt,CONVERT(int,NULL) AS Weekday,CONVERT(bigint,NULL) AS Minimum,CONVERT(bigint,NULL) AS Maximum,IsActive AS Active,CAST(CASE WHEN IsActive=1 THEN N'زمان فعال فعلی' ELSE N'غیرفعال' END AS nvarchar(300)) AS Detail,CONVERT(int,NULL) AS Extra1,CONVERT(int,NULL) AS Extra2 FROM pricing.RechargeSchedule ORDER BY IsActive DESC,ScheduledAt";
                case BusinessManagementArea.Tickets:
                    return "SELECT TicketSettingId AS RecordId,CONVERT(int,NULL) AS Code,CAST(N'مبلغ بلیت' AS nvarchar(300)) AS Name,CONVERT(bigint,TicketValue) AS Amount,CONVERT(int,NULL) AS [Percent],CONVERT(datetime,NULL) AS StartsAt,CONVERT(datetime,NULL) AS EndsAt,CONVERT(int,NULL) AS Weekday,CONVERT(bigint,NULL) AS Minimum,CONVERT(bigint,NULL) AS Maximum,IsActive AS Active,CAST(CASE WHEN IsActive=1 THEN N'مبلغ فعال برای فروش بلیت' ELSE N'غیرفعال' END AS nvarchar(300)) AS Detail,CONVERT(int,NULL) AS Extra1,CONVERT(int,NULL) AS Extra2 FROM ticket.TicketSetting ORDER BY IsActive DESC,TicketSettingId DESC";
                case BusinessManagementArea.DevicePromotions:
                    return "SELECT dp.DevicePromotionId AS RecordId,d.ControllerDeviceId AS Code,d.DeviceName AS Name,CONVERT(bigint,NULL) AS Amount,pe.DiscountPercent AS [Percent],CONVERT(datetime,NULL) AS StartsAt,CONVERT(datetime,NULL) AS EndsAt,CONVERT(int,NULL) AS Weekday,CONVERT(bigint,NULL) AS Minimum,CONVERT(bigint,NULL) AS Maximum,dp.IsActive AS Active,CAST(pe.PromotionName+N' | تخفیف '+CONVERT(nvarchar(10),pe.DiscountPercent)+N'٪' AS nvarchar(300)) AS Detail,dp.DeviceId AS Extra1,pe.PromotionCode AS Extra2 FROM FuntinoDeviceOperations.device.DevicePromotion dp JOIN FuntinoDeviceOperations.device.Device d ON d.DeviceId=dp.DeviceId JOIN FuntinoDeviceOperations.reference.PromotionEvent pe ON pe.PromotionEventId=dp.PromotionEventId ORDER BY d.DeviceName,pe.PromotionName";
                default: throw new ArgumentOutOfRangeException("area");
            }
        }

        private static void ValidateRecord(BusinessManagementArea area, BusinessManagementRecord record)
        {
            if (area == BusinessManagementArea.Attractions || area == BusinessManagementArea.ControllerDevices)
            {
                if (record.Code <= 0) throw new ArgumentOutOfRangeException("record.Code", "کد دستگاه باید بزرگ‌تر از صفر باشد.");
                if (String.IsNullOrWhiteSpace(record.Name) || record.Name.Trim().Length > 50) throw new ArgumentException("نام دستگاه را حداکثر تا ۵۰ نویسه وارد کنید.");
                if (record.Duration < 0) throw new ArgumentOutOfRangeException("record.Duration", "مدت دستگاه نمی‌تواند منفی باشد.");
            }
            if (area == BusinessManagementArea.Attractions && (record.Amount < 0 || record.Amount > Int32.MaxValue))
                throw new ArgumentOutOfRangeException("record.Amount", "قیمت دستگاه تفریحی معتبر نیست.");
            if (area == BusinessManagementArea.ControllerDevices && record.Amount < 0)
                throw new ArgumentOutOfRangeException("record.Amount", "قیمت دستگاه کنترل‌شونده نمی‌تواند منفی باشد.");
            if (area == BusinessManagementArea.Promotions)
            {
                if (record.Code <= 0) throw new ArgumentOutOfRangeException("record.Code", "کد جشنواره باید بزرگ‌تر از صفر باشد.");
                if (String.IsNullOrWhiteSpace(record.Name) || record.Name.Trim().Length > 100) throw new ArgumentException("نام جشنواره را حداکثر تا ۱۰۰ نویسه وارد کنید.");
                if (record.Percent < 0 || record.Percent > 100) throw new ArgumentOutOfRangeException("record.Percent", "درصد جشنواره باید بین صفر تا ۱۰۰ باشد.");
                if (record.EndsAt <= record.StartsAt) throw new ArgumentException("پایان جشنواره باید پس از زمان آغاز آن باشد.");
            }
            if (area == BusinessManagementArea.DiscountPolicies &&
                (record.Code < 1 || record.Code > 10 || record.Weekday < 1 || record.Weekday > 7 || record.Minimum < 0 || record.Maximum < record.Minimum || record.Maximum > Int32.MaxValue))
                throw new ArgumentException("سطح، روز یا بازه‌ی مبلغ قانون تخفیف معتبر نیست.");
            if (area == BusinessManagementArea.DiscountRates && (record.Id < 1 || record.Id > 10 || record.Percent < 0 || record.Percent > 100))
                throw new ArgumentException("درصد تخفیف باید بین صفر تا ۱۰۰ باشد.");
            if (area == BusinessManagementArea.Deposits && (record.Amount < 0 || record.Amount > Int32.MaxValue))
                throw new ArgumentOutOfRangeException("record.Amount", "مبلغ بیعانه معتبر نیست.");
            if (area == BusinessManagementArea.RechargeLimit && (record.Amount <= 0 || record.Amount > Int32.MaxValue))
                throw new ArgumentOutOfRangeException("record.Amount", "سقف شارژ باید بزرگ‌تر از صفر باشد.");
            if (area == BusinessManagementArea.HourlyPlans && (record.Amount < 0 || record.Amount > Int32.MaxValue))
                throw new ArgumentOutOfRangeException("record.Amount", "مبلغ شارژ ساعتی معتبر نیست.");
            if (area == BusinessManagementArea.Schedules && record.StartsAt == DateTime.MinValue)
                throw new ArgumentException("زمان شارژ برنامه‌ریزی‌شده را وارد کنید.");
            if (area == BusinessManagementArea.Tickets && (record.Amount <= 0 || record.Amount > Int32.MaxValue))
                throw new ArgumentOutOfRangeException("record.Amount", "مبلغ بلیت باید بزرگ‌تر از صفر باشد.");
            if (area == BusinessManagementArea.DevicePromotions && (record.Extra1 <= 0 || record.Extra2 <= 0))
                throw new ArgumentException("دستگاه و جشنواره را انتخاب کنید.");
        }

        private static void EnsureDiscountTier(SqlConnection connection, SqlTransaction transaction, int tierId)
        {
            using (var command = new SqlCommand("SELECT COUNT(1) FROM pricing.DiscountRule WHERE DiscountTypeId=@id", connection, transaction))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = tierId;
                if (Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) != 1)
                    throw new InvalidOperationException("سطح تخفیف انتخاب‌شده تعریف نشده است؛ ابتدا درصدهای تخفیف را بررسی کنید.");
            }
        }

        private static void EnsureNoDiscountOverlap(SqlConnection connection, SqlTransaction transaction, BusinessManagementRecord record)
        {
            if (!record.IsActive) return;
            using (var command = new SqlCommand(@"
SELECT COUNT(1) FROM pricing.DiscountPolicy
WHERE IsActive=1 AND DayOfWeek=@day AND DiscountPolicyId<>@id
  AND MinimumPrice<=@maximum AND MaximumPrice>=@minimum;", connection, transaction))
            {
                command.Parameters.Add("@day", SqlDbType.Int).Value = record.Weekday;
                command.Parameters.Add("@id", SqlDbType.Int).Value = record.Id;
                command.Parameters.Add("@maximum", SqlDbType.Int).Value = record.Maximum;
                command.Parameters.Add("@minimum", SqlDbType.Int).Value = record.Minimum;
                if (Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) != 0)
                    throw new InvalidOperationException("این بازه با یک قانون فعال همان روز هم‌پوشانی دارد. ابتدا قانون قبلی را اصلاح یا غیرفعال کنید.");
            }
        }

        private static void AddDiscountPolicyParameters(SqlCommand command, BusinessManagementRecord record)
        {
            command.Parameters.Add("@tier", SqlDbType.Int).Value = record.Code;
            command.Parameters.Add("@day", SqlDbType.Int).Value = record.Weekday;
            command.Parameters.Add("@minimum", SqlDbType.Int).Value = record.Minimum;
            command.Parameters.Add("@maximum", SqlDbType.Int).Value = record.Maximum;
            command.Parameters.Add("@active", SqlDbType.Bit).Value = record.IsActive;
            command.Parameters.Add("@id", SqlDbType.Int).Value = record.Id;
        }

        private static void AddPromotionParameters(SqlCommand command, BusinessManagementRecord record)
        {
            command.Parameters.Add("@code", SqlDbType.Int).Value = record.Code;
            command.Parameters.Add("@name", SqlDbType.NVarChar, 300).Value = record.Name.Trim();
            command.Parameters.Add("@startsAt", SqlDbType.DateTime).Value = record.StartsAt;
            command.Parameters.Add("@endsAt", SqlDbType.DateTime).Value = record.EndsAt;
            command.Parameters.Add("@percent", SqlDbType.Int).Value = record.Percent;
        }

        private static int ToLegacyWeekday(DayOfWeek weekday)
        {
            return ((int)weekday + 1) % 7 + 1;
        }

        private static string PermissionStem(BusinessManagementArea area)
        {
            switch (area)
            {
                case BusinessManagementArea.Attractions: return "operations.attractions";
                case BusinessManagementArea.ControllerDevices: return "operations.devices";
                case BusinessManagementArea.Promotions: return "operations.promotions";
                case BusinessManagementArea.DiscountPolicies: return "operations.discounts";
                case BusinessManagementArea.DiscountRates: return "operations.discount_rates";
                case BusinessManagementArea.Deposits: return "operations.deposit";
                case BusinessManagementArea.RechargeLimit: return "operations.recharge_limit";
                case BusinessManagementArea.HourlyPlans: return "operations.hourly_plans";
                case BusinessManagementArea.Schedules: return "operations.schedules";
                case BusinessManagementArea.Tickets: return "operations.tickets";
                case BusinessManagementArea.DevicePromotions: return "operations.promotions";
                default: throw new ArgumentOutOfRangeException("area");
            }
        }

        private static void RequireArea(BusinessManagementArea area, string action)
        {
            AuthorizationGuard.Require(PermissionStem(area) + "." + action);
            switch (area)
            {
                case BusinessManagementArea.Attractions:
                    AuthorizationGuard.Require(PermissionCatalog.TableCode("reference", action == "delete" ? "update" : action)); break;
                case BusinessManagementArea.ControllerDevices:
                    AuthorizationGuard.Require(PermissionCatalog.TableCode("device", action == "delete" ? "update" : action)); break;
                case BusinessManagementArea.Promotions:
                    AuthorizationGuard.Require(PermissionCatalog.TableCode("pricing", action == "delete" ? "update" : action));
                    AuthorizationGuard.Require(PermissionCatalog.TableCode("reference", action == "delete" ? "update" : action)); break;
                case BusinessManagementArea.DiscountPolicies:
                    AuthorizationGuard.Require("operations.discount_rates.view");
                    AuthorizationGuard.Require(PermissionCatalog.TableCode("pricing", "view"));
                    AuthorizationGuard.Require(PermissionCatalog.TableCode("pricing", action == "delete" ? "update" : action));
                    break;
                case BusinessManagementArea.DiscountRates:
                case BusinessManagementArea.RechargeLimit:
                case BusinessManagementArea.HourlyPlans:
                case BusinessManagementArea.Schedules:
                    AuthorizationGuard.Require(PermissionCatalog.TableCode("pricing", action == "delete" ? "update" : action)); break;
                case BusinessManagementArea.Deposits:
                    AuthorizationGuard.Require(PermissionCatalog.TableCode("configuration", action == "delete" ? "update" : action)); break;
                case BusinessManagementArea.Tickets:
                    AuthorizationGuard.Require(PermissionCatalog.TableCode("ticket", action == "delete" ? "update" : action)); break;
                case BusinessManagementArea.DevicePromotions:
                    AuthorizationGuard.Require("operations.promotions.assign_devices");
                    AuthorizationGuard.Require("operations.devices.view");
                    AuthorizationGuard.Require("operations.promotions.view");
                    AuthorizationGuard.Require(PermissionCatalog.TableCode("pricing", "view"));
                    AuthorizationGuard.Require(PermissionCatalog.TableCode("device", action == "delete" ? "update" : action));
                    AuthorizationGuard.Require(PermissionCatalog.TableCode("reference", "view")); break;
            }
        }

        private static string DataAction(string action)
        {
            return action == "delete" ? "update" : action;
        }

        private static string AuditAction(BusinessManagementArea area, string action)
        {
            return "operations-" + area.ToString().ToLowerInvariant() + "-" + action;
        }

        private static string EntityName(BusinessManagementArea area)
        {
            return "BusinessManagement." + area;
        }

        private static string AuditDescription(BusinessManagementArea area, BusinessManagementRecord record, bool isNew)
        {
            switch (area)
            {
                case BusinessManagementArea.Promotions: return (isNew ? "ثبت جشنواره " : "ویرایش جشنواره ") + record.Name + " | کد " + record.Code;
                case BusinessManagementArea.DiscountPolicies: return "قانون روز " + record.Weekday + "، بازه‌ی " + record.Minimum + " تا " + record.Maximum + " تومان";
                case BusinessManagementArea.DiscountRates: return "سطح " + record.Id + " برابر " + record.Percent + " درصد";
                case BusinessManagementArea.Deposits: return "مبلغ بیعانه " + record.Amount + " تومان";
                case BusinessManagementArea.RechargeLimit: return "سقف هر بار شارژ " + record.Amount + " تومان";
                case BusinessManagementArea.Tickets: return "مبلغ بلیت " + record.Amount + " تومان";
                default: return (isNew ? "افزودن " : "ویرایش ") + (record.Name ?? String.Empty);
            }
        }
    }
}
