using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FuntinoControlCenter.Database
{
    internal static class LegacySchemaMigrator
    {
        private const int LegacyRenameMigrationVersion = 1;
        private static readonly CatalogMap ParkCatalog = CreateParkCatalog();
        private static readonly CatalogMap DeviceCatalog = CreateDeviceCatalog();

        public static void Apply(SqlConnection connection, SqlTransaction transaction, bool deviceDatabase)
        {
            CatalogMap map = deviceDatabase ? DeviceCatalog : ParkCatalog;
            EnsureSchema(connection, transaction, "app");
            if (ObjectExists(connection, transaction, "app", "SchemaMigrationHistory", "U") &&
                IsMigrationApplied(connection, transaction, LegacyRenameMigrationVersion)) return;

            ValidateLegacyCatalog(connection, transaction, map);
            EnsureMigrationHistory(connection, transaction);
            Dictionary<string, long> originalRowCounts = CaptureRowCounts(connection, transaction, map);
            foreach (string schema in map.Tables.Select(x => x.TargetSchema)
                .Concat(map.Routines.Select(x => x.TargetSchema))
                .Concat(new[] { "compat" }).Distinct(StringComparer.OrdinalIgnoreCase))
                EnsureSchema(connection, transaction, schema);

            Dictionary<string, ModuleDefinition> modules = LoadModuleDefinitions(connection, transaction, map);
            MoveAndRenameTables(connection, transaction, map);
            CreateCompatibilityViews(connection, transaction, map);
            MoveAndRenameModules(connection, transaction, map, modules);
            RenameDefaultConstraints(connection, transaction, map);
            ValidateRenamedCatalog(connection, transaction, map, originalRowCounts);

            InsertMigrationHistory(connection, transaction, LegacyRenameMigrationVersion,
                deviceDatabase
                    ? "Renamed device catalog schema while preserving reference data and stored routines"
                    : "Renamed park operations catalog schema while preserving reference data and stored routines");
        }

        private static CatalogMap CreateParkCatalog()
        {
            return new CatalogMap(
                21,
                126,
                51,
                0,
                0,
                new[]
                {
                    Table("Cash", "finance", "CashRegister",
                        "ID", "CashRegisterId", "Name", "CashRegisterName"),
                    Table("CashUser", "security", "CashRegisterUser",
                        "ID", "CashRegisterUserId", "UserID", "LegacyUserAccountId", "CashID", "CashRegisterId"),
                    Table("City", "reference", "City",
                        "ID", "CityId", "Name", "CityName"),
                    Table("CreditSharj", "finance", "CreditRechargeTransaction",
                        "ID", "CreditRechargeTransactionId",
                        "CartNo1", "CardNumberSegment1", "CartNo2", "CardNumberSegment2",
                        "CartNo3", "CardNumberSegment3", "CartNo4", "CardNumberSegment4",
                        "Count", "CreditCount", "UserName", "CreatedByUserName", "CreateDate", "CreatedAt"),
                    Table("Devices", "reference", "AttractionDevice",
                        "ID", "AttractionDeviceId", "Name", "AttractionName", "Code", "AttractionCode",
                        "Price", "BasePrice", "Active", "IsActive"),
                    Table("Discount", "pricing", "DiscountRule",
                        "DiscountType", "DiscountTypeId", "DisPercent", "DiscountPercent"),
                    Table("DiscountType", "pricing", "DiscountPolicy",
                        "ID", "DiscountPolicyId", "DiscountTypeID", "DiscountTypeId", "WeekDay", "DayOfWeek",
                        "MinPrice", "MinimumPrice", "MaxPrice", "MaximumPrice", "Active", "IsActive"),
                    Table("Festival", "pricing", "PromotionEvent",
                        "ID", "PromotionEventId", "Code", "PromotionCode", "Name", "PromotionName",
                        "FromDate", "StartsAt", "ToDate", "EndsAt", "DiscountPercent", "DiscountPercent"),
                    Table("HourSharj", "finance", "HourlyRechargeTransaction",
                        "ID", "HourlyRechargeTransactionId", "HourSharjID", "HourlyRechargePlanId",
                        "CartNo1", "CardNumberSegment1", "CartNo2", "CardNumberSegment2",
                        "CartNo3", "CardNumberSegment3", "CartNo4", "CardNumberSegment4",
                        "Amount", "RechargeAmount", "UserName", "CreatedByUserName",
                        "CreateDate", "CreatedAt", "PosResultID", "PointOfSaleTransactionId"),
                    Table("HourSharjSetting", "pricing", "HourlyRechargePlan",
                        "ID", "HourlyRechargePlanId", "Name", "PlanName",
                        "SharjValue", "RechargeAmount", "Active", "IsActive"),
                    Table("Park", "reference", "Park",
                        "ID", "ParkId", "Name", "ParkName", "CityID", "CityId"),
                    Table("PosTable", "payments", "PointOfSaleTransaction",
                        "ID", "PointOfSaleTransactionId", "Composite", "IsCompositePayment",
                        "Amount", "TransactionAmount", "CompositeAmount", "CompositeAmount",
                        "Status", "IsSuccessful", "ResponseCode", "ResponseCode",
                        "ResponseDescription", "ResponseDescription", "CardNumberMask", "MaskedCardNumber",
                        "CardNumberHash_Sha1", "CardNumberSha1Hash", "TraceNumber", "TraceNumber",
                        "TxnDate", "TransactionDateText", "RRN", "RetrievalReferenceNumber",
                        "TerminalId", "TerminalId", "SerialId", "TerminalSerialId",
                        "ReqAmount", "RequestedAmountText", "AffectiveAmount", "EffectiveAmountText",
                        "POS_Version", "PointOfSaleVersion", "SharjTableID", "RechargeTransactionId"),
                    Table("Roles", "security", "Role",
                        "ID", "RoleId", "Role", "RoleName", "RoleType", "RoleType"),
                    Table("Setting", "configuration", "ApplicationConfiguration",
                        "Row", "ConfigurationRowId", "PercentView", "DiscountDisplayPercent",
                        "UpdateDate", "UpdatedAt", "Active", "IsActive", "RunProgram", "IsProgramEnabled",
                        "Code", "ConfigurationCode", "ParkID", "ParkId", "Deposite", "DepositAmount",
                        "PortPos", "PointOfSalePort"),
                    Table("SharjLimit", "pricing", "RechargeLimit",
                        "ID", "RechargeLimitId", "Value", "LimitValue"),
                    Table("SharjTble", "finance", "RechargeTransaction",
                        "ID", "RechargeTransactionId", "Price", "RechargeAmount",
                        "Disscount", "DiscountAmount", "FestivalCode", "PromotionCode",
                        "CartNo1", "CardNumberSegment1", "CartNo2", "CardNumberSegment2",
                        "CartNo3", "CardNumberSegment3", "CartNo4", "CardNumberSegment4",
                        "CreateDate", "CreatedAt", "UserName", "CreatedByUserName", "SharjTime", "RechargeAt",
                        "DiscountPercent", "DiscountPercent", "IsPos", "IsPointOfSale",
                        "PosResultID", "PointOfSaleTransactionId", "HourSharj", "IsHourlyRecharge",
                        "HourSharjID", "HourlyRechargePlanId", "CreditSharj", "IsCreditRecharge",
                        "Credit", "CreditAmount"),
                    Table("SharjTimeTbl", "pricing", "RechargeSchedule",
                        "ID", "RechargeScheduleId", "SharjTime", "ScheduledAt", "Active", "IsActive"),
                    Table("Ticket", "ticket", "TicketTransaction",
                        "ID", "TicketTransactionId", "TicketID", "TicketSettingId",
                        "CartNo1", "CardNumberSegment1", "CartNo2", "CardNumberSegment2",
                        "CartNo3", "CardNumberSegment3", "CartNo4", "CardNumberSegment4",
                        "Count", "TicketCount", "SharjValue", "TicketValue", "Type", "IsRechargeTicket",
                        "CreateDate", "CreatedAt", "UserName", "CreatedByUserName"),
                    Table("TicketSetting", "ticket", "TicketSetting",
                        "ID", "TicketSettingId", "Value", "TicketValue", "Active", "IsActive"),
                    Table("UserRole", "security", "UserRole",
                        "UserID", "LegacyUserAccountId", "RoleID", "RoleId"),
                    Table("UserTable", "security", "LegacyUserAccount",
                        "ID", "LegacyUserAccountId", "UserName", "UserName", "Password", "LegacyPassword",
                        "IsAdmin", "IsAdministrator", "CashID", "CashRegisterId", "IsLogin", "IsLoggedIn")
                },
                new[]
                {
                    Routine("AddLimitSharj", "business", "usp_FuntinoPark_AddRechargeLimit", "P"),
                    Routine("GetAllDeviceName", "business", "usp_FuntinoPark_GetAttractionDeviceNames", "P"),
                    Routine("GetDiscount", "business", "usp_FuntinoPark_GetDiscountRate", "P"),
                    Routine("SharjTimeSet", "business", "usp_FuntinoPark_SetRechargeSchedule", "P"),
                    Routine("spAddUserCash", "business", "usp_FuntinoPark_AssignCashRegisterToUser", "P"),
                    Routine("spCreditSharjSetInfo", "business", "usp_FuntinoPark_CreateCreditRechargeTransaction", "P"),
                    Routine("spDeleteDiscountType", "business", "usp_FuntinoPark_DeleteDiscountPolicy", "P"),
                    Routine("spDeleteFestival", "business", "usp_FuntinoPark_DeletePromotionEvent", "P"),
                    Routine("spDeviceAdd", "business", "usp_FuntinoPark_CreateAttractionDevice", "P"),
                    Routine("spDeviceDelete", "business", "usp_FuntinoPark_DeleteAttractionDevice", "P"),
                    Routine("spDeviceEdit", "business", "usp_FuntinoPark_UpdateAttractionDevice", "P"),
                    Routine("spDiscountSet", "business", "usp_FuntinoPark_SetDiscountRule", "P"),
                    Routine("spDiscountTypeSet", "business", "usp_FuntinoPark_SetDiscountPolicy", "P"),
                    Routine("spFestivalAdd", "business", "usp_FuntinoPark_CreatePromotionEvent", "P"),
                    Routine("spFestivalEdit", "business", "usp_FuntinoPark_UpdatePromotionEvent", "P"),
                    Routine("spFestivalGetAll", "business", "usp_FuntinoPark_GetPromotionEvents", "P"),
                    Routine("spGetAllCash", "business", "usp_FuntinoPark_GetCashRegisters", "P"),
                    Routine("spGetAllDiscountType", "business", "usp_FuntinoPark_GetDiscountPolicies", "P"),
                    Routine("spGetAllSharjTime", "business", "usp_FuntinoPark_GetRechargeSchedules", "P"),
                    Routine("spGetDeposite", "business", "usp_FuntinoPark_GetDepositAmount", "P"),
                    Routine("spGetDiscountFit", "business", "usp_FuntinoPark_GetApplicableDiscounts", "P"),
                    Routine("spGetHourSharj", "business", "usp_FuntinoPark_GetHourlyRechargePlans", "P"),
                    Routine("spGetHourSharjSetting", "business", "usp_FuntinoPark_GetHourlyRechargeSettings", "P"),
                    Routine("spGetOpSharj", "business", "usp_FuntinoPark_GetRechargeTransactions", "P"),
                    Routine("spGetPosPort", "business", "usp_FuntinoPark_GetPointOfSalePort", "P"),
                    Routine("spGetSetting", "business", "usp_FuntinoPark_GetApplicationConfiguration", "P"),
                    Routine("spGetSharjTime", "business", "usp_FuntinoPark_GetRechargeSchedule", "P"),
                    Routine("spGetUser", "business", "usp_FuntinoPark_GetUserAccount", "P"),
                    Routine("spGetUserRoles", "business", "usp_FuntinoPark_GetUserRoles", "P"),
                    Routine("spHourSharjSetInfo", "business", "usp_FuntinoPark_CreateHourlyRechargeTransaction", "P"),
                    Routine("spHourSharjSettingGetInfo", "business", "usp_FuntinoPark_GetHourlyRechargePlan", "P"),
                    Routine("spHourSharjSettingSetInfo", "business", "usp_FuntinoPark_SetHourlyRechargePlan", "P"),
                    Routine("spLimitSharjGet", "business", "usp_FuntinoPark_GetRechargeLimit", "P"),
                    Routine("spLoginInsert", "business", "usp_FuntinoPark_RecordUserLogin", "P"),
                    Routine("spPosUnSharjGetinfo", "business", "usp_FuntinoPark_GetUnsettledPointOfSaleTransactions", "P"),
                    Routine("spSetDeposite", "business", "usp_FuntinoPark_SetDepositAmount", "P"),
                    Routine("spSetPortPos", "business", "usp_FuntinoPark_SetPointOfSalePort", "P"),
                    Routine("SPSetSetting", "business", "usp_FuntinoPark_SetApplicationConfiguration", "P"),
                    Routine("spSharjPosSetInfo", "business", "usp_FuntinoPark_CreatePointOfSaleRecharge", "P"),
                    Routine("spSharjReport", "business", "usp_FuntinoPark_GetRechargeReport", "P"),
                    Routine("spSharjSetInfo", "business", "usp_FuntinoPark_CreateRechargeTransaction", "P"),
                    Routine("spTicketGetInfo", "business", "usp_FuntinoPark_GetTicketValue", "P"),
                    Routine("spTicketIDGetInfo", "business", "usp_FuntinoPark_GetTicketSetting", "P"),
                    Routine("spTicketReport", "business", "usp_FuntinoPark_GetTicketReport", "P"),
                    Routine("spTicketSetInfo", "business", "usp_FuntinoPark_CreateTicketTransaction", "P"),
                    Routine("spTicketSetting", "business", "usp_FuntinoPark_SetTicketSettings", "P"),
                    Routine("spUserAdd", "business", "usp_FuntinoPark_CreateUserAccount", "P"),
                    Routine("spUserLogin", "business", "usp_FuntinoPark_AuthenticateUser", "P"),
                    Routine("spUserPassChange", "business", "usp_FuntinoPark_ChangeUserPassword", "P"),
                    Routine("spUserRoleDelete", "business", "usp_FuntinoPark_RemoveUserRole", "P"),
                    Routine("spUserRoleSet", "business", "usp_FuntinoPark_AssignUserRole", "P")
                });
        }

        private static CatalogMap CreateDeviceCatalog()
        {
            return new CatalogMap(
                13,
                104,
                47,
                1,
                1,
                new[]
                {
                    Table("City", "reference", "City",
                        "ID", "CityId", "Name", "CityName"),
                    Table("DeviceFestival", "device", "DevicePromotion",
                        "ID", "DevicePromotionId", "DeviceID", "DeviceId",
                        "FestivalID", "PromotionEventId", "Active", "IsActive"),
                    Table("DeviceOperation", "device", "DeviceOperation",
                        "ID", "DeviceOperationId", "DeviceID", "DeviceId", "Price", "UnitPrice",
                        "Count", "OperationCount", "OpDate", "OperationDate",
                        "FesPercent", "FestivalDiscountPercent", "FesCount", "FestivalDiscountCount",
                        "Percent10", "OperationCountAt10Percent", "Percent20", "OperationCountAt20Percent",
                        "Percent30", "OperationCountAt30Percent", "Percent40", "OperationCountAt40Percent",
                        "Percent50", "OperationCountAt50Percent", "Percent60", "OperationCountAt60Percent",
                        "Percent70", "OperationCountAt70Percent", "Percent80", "OperationCountAt80Percent",
                        "Percent90", "OperationCountAt90Percent", "Percent100", "OperationCountAt100Percent",
                        "Price10", "AmountAt10Percent", "Price20", "AmountAt20Percent",
                        "Price30", "AmountAt30Percent", "Price40", "AmountAt40Percent",
                        "Price50", "AmountAt50Percent", "Price60", "AmountAt60Percent",
                        "Price70", "AmountAt70Percent", "Price80", "AmountAt80Percent",
                        "Price90", "AmountAt90Percent", "Price100", "AmountAt100Percent"),
                    Table("DeviceOperationBuffer", "device", "DeviceOperationBuffer",
                        "ID", "DeviceOperationBufferId", "DeviceID", "DeviceId", "Price", "UnitPrice",
                        "Count", "OperationCount", "OpDate", "OperationDate",
                        "FesPercent", "FestivalDiscountPercent", "FesCount", "FestivalDiscountCount",
                        "Percent10", "OperationCountAt10Percent", "Percent20", "OperationCountAt20Percent",
                        "Percent30", "OperationCountAt30Percent", "Percent40", "OperationCountAt40Percent",
                        "Percent50", "OperationCountAt50Percent", "Percent60", "OperationCountAt60Percent",
                        "Percent70", "OperationCountAt70Percent", "Percent80", "OperationCountAt80Percent",
                        "Percent90", "OperationCountAt90Percent", "Percent100", "OperationCountAt100Percent",
                        "Price10", "AmountAt10Percent", "Price20", "AmountAt20Percent",
                        "Price30", "AmountAt30Percent", "Price40", "AmountAt40Percent",
                        "Price50", "AmountAt50Percent", "Price60", "AmountAt60Percent",
                        "Price70", "AmountAt70Percent", "Price80", "AmountAt80Percent",
                        "Price90", "AmountAt90Percent", "Price100", "AmountAt100Percent"),
                    Table("Devices", "device", "Device",
                        "ID", "DeviceId", "IDConter", "ControllerDeviceId", "Name", "DeviceName",
                        "PriceType", "PricingType", "Price", "BasePrice", "Status", "IsActive",
                        "NimBaha", "IsHalfPrice", "Time", "Duration"),
                    Table("DeviceSerials", "device", "DeviceSerial",
                        "ID", "DeviceSerialId", "DeviceID", "DeviceId",
                        "Serial1", "SerialNumber1", "Serial2", "SerialNumber2",
                        "Serial3", "SerialNumber3", "Serial4", "SerialNumber4",
                        "Price", "TotalAmount", "OpDate", "OperationDate"),
                    Table("Festival", "reference", "PromotionEvent",
                        "ID", "PromotionEventId", "FID", "PromotionCode", "FName", "PromotionName",
                        "FromDate", "StartsAt", "ToDate", "EndsAt", "Percent", "DiscountPercent"),
                    Table("Park", "reference", "Park",
                        "ID", "ParkId", "Name", "ParkName", "CityID", "CityId"),
                    Table("Roles", "security", "Role",
                        "ID", "RoleId", "Role", "RoleName", "RoleType", "RoleType"),
                    Table("Setting", "configuration", "DeviceConfiguration",
                        "DCount", "DeviceCount", "ReportDate", "ReportDate",
                        "ConnectionCode", "ConnectionCode", "Auto", "IsAutomatic",
                        "ParkID", "ParkId"),
                    Table("TotalDeviceOP", "device", "DeviceOperationSummary",
                        "ID", "DeviceOperationSummaryId", "DeviceID", "DeviceId",
                        "SumCount", "TotalOperationCount", "Sum", "TotalAmount", "DateOP", "OperationDate"),
                    Table("UserRole", "security", "UserRole",
                        "UserID", "LegacyUserAccountId", "RoleID", "RoleId"),
                    Table("UserTable", "security", "LegacyUserAccount",
                        "ID", "LegacyUserAccountId", "UserName", "UserName",
                        "Password", "LegacyPassword", "IsAdmin", "IsAdministrator")
                },
                new[]
                {
                    Routine("GetAllDeviceName", "device", "usp_FuntinoDevice_GetDeviceNames", "P"),
                    Routine("spChangePrice", "device", "usp_FuntinoDevice_ChangeDevicePrice", "P"),
                    Routine("spDeviceAdd", "device", "usp_FuntinoDevice_CreateDevice", "P"),
                    Routine("spDeviceDelete", "device", "usp_FuntinoDevice_DeleteDevice", "P"),
                    Routine("spDeviceEdit", "device", "usp_FuntinoDevice_UpdateDevice", "P"),
                    Routine("spDeviceFestivalAdd", "device", "usp_FuntinoDevice_AssignPromotionToDevice", "P"),
                    Routine("spdeviceFestivalDeactive", "device", "usp_FuntinoDevice_DeactivateDevicePromotion", "P"),
                    Routine("spdeviceFestivalDeactive1", "device", "usp_FuntinoDevice_DeactivateSelectedDevicePromotion", "P"),
                    Routine("spDeviceFestivalGetInfo", "device", "usp_FuntinoDevice_GetDevicePromotion", "P"),
                    Routine("spDeviceFestivalsActiveAllGetInfo", "device", "usp_FuntinoDevice_GetActiveDevicePromotions", "P"),
                    Routine("spDeviceFestivalsActiveGetInfo", "device", "usp_FuntinoDevice_GetActiveDevicePromotionsByDevice", "P"),
                    Routine("spDeviceFestivalsGetInfo", "device", "usp_FuntinoDevice_GetDevicePromotions", "P"),
                    Routine("spDeviceFunctionReport", "device", "usp_FuntinoDevice_GetDeviceUsageReport", "P"),
                    Routine("spDeviceKatrkhanReport", "device", "usp_FuntinoDevice_GetDeviceCardReaderReport", "P"),
                    Routine("spDeviceNimbahaSetting", "device", "usp_FuntinoDevice_SetHalfPriceConfiguration", "P"),
                    Routine("spDeviceOnOff", "device", "usp_FuntinoDevice_SetDeviceAvailability", "P"),
                    Routine("spDeviceOPDelInfo", "device", "usp_FuntinoDevice_DeleteBufferedOperation", "P"),
                    Routine("spDeviceOPFixManualPriceSetInfo", "device", "usp_FuntinoDevice_SetManualOperationPrice", "P"),
                    Routine("spDeviceOPFixManualPriceSetInfoBuffer", "device", "usp_FuntinoDevice_SetBufferedManualOperationPrice", "P"),
                    Routine("spDeviceOPFixPriceGetInfo", "device", "usp_FuntinoDevice_GetOperationPriceAdjustment", "P"),
                    Routine("spDeviceOPFixPriceSetInfo", "device", "usp_FuntinoDevice_SetOperationPriceAdjustment", "P"),
                    Routine("spDeviceOPSetInfo", "device", "usp_FuntinoDevice_RecordDeviceOperation", "P"),
                    Routine("spFestivalAdd", "device", "usp_FuntinoDevice_CreatePromotionEvent", "P"),
                    Routine("spFestivalByCode", "device", "usp_FuntinoDevice_GetPromotionEventByCode", "P"),
                    Routine("spFestivalDelete", "device", "usp_FuntinoDevice_DeletePromotionEvent", "P"),
                    Routine("spFestivalEdit", "device", "usp_FuntinoDevice_UpdatePromotionEvent", "P"),
                    Routine("spFestivalGetAll", "device", "usp_FuntinoDevice_GetPromotionEvents", "P"),
                    Routine("spFunctionSetInfo", "device", "usp_FuntinoDevice_RecordDeviceFunctionReport", "P"),
                    Routine("spGetDeviceInfoByID", "device", "usp_FuntinoDevice_GetDeviceById", "P"),
                    Routine("spGetDeviceNameByID", "device", "usp_FuntinoDevice_GetDeviceNameById", "P"),
                    Routine("spGetDeviceNameByID1", "device", "usp_FuntinoDevice_GetAlternateDeviceNameById", "P"),
                    Routine("spGetDeviceNimbahaByID", "device", "usp_FuntinoDevice_GetDeviceHalfPriceById", "P"),
                    Routine("spGetDevicePriceByID", "device", "usp_FuntinoDevice_GetDevicePriceById", "P"),
                    Routine("spGetOpKart", "device", "usp_FuntinoDevice_GetDeviceOperationsByCard", "P"),
                    Routine("spGetUser", "device", "usp_FuntinoDevice_GetUserAccount", "P"),
                    Routine("spGetUserRoles", "device", "usp_FuntinoDevice_GetUserRoles", "P"),
                    Routine("spSettingAutoSetInfo", "device", "usp_FuntinoDevice_SetAutomaticConfiguration", "P"),
                    Routine("spSettingGetInfo", "device", "usp_FuntinoDevice_GetConfiguration", "P"),
                    Routine("spSettingSetInfo", "device", "usp_FuntinoDevice_SetConfiguration", "P"),
                    Routine("spUserAdd", "device", "usp_FuntinoDevice_CreateUserAccount", "P"),
                    Routine("spUserLogin", "device", "usp_FuntinoDevice_AuthenticateUser", "P"),
                    Routine("spUserPassChange", "device", "usp_FuntinoDevice_ChangeUserPassword", "P"),
                    Routine("spUserRoleDelete", "device", "usp_FuntinoDevice_RemoveUserRole", "P"),
                    Routine("spUserRoleSet", "device", "usp_FuntinoDevice_AssignUserRole", "P"),
                    Routine("spWebTransfer", "device", "usp_FuntinoDevice_ExportWebOperationSummary", "P"),
                    Routine("GetColumnValue", "device", "ufn_FuntinoDevice_GetDelimitedValue", "FN"),
                    Routine("fnSplit", "device", "ufn_FuntinoDevice_SplitDelimitedValues", "TF")
                });
        }

        private static TableNameMap Table(string sourceName, string targetSchema, string targetName, params string[] columns)
        {
            if (columns.Length % 2 != 0) throw new InvalidOperationException("Invalid column mapping for " + sourceName + ".");
            var names = new List<ColumnNameMap>();
            for (int index = 0; index < columns.Length; index += 2)
                names.Add(new ColumnNameMap(columns[index], columns[index + 1]));
            return new TableNameMap(sourceName, targetSchema, targetName, names);
        }

        private static RoutineNameMap Routine(string sourceName, string targetSchema, string targetName, string objectType)
        {
            return new RoutineNameMap(sourceName, targetSchema, targetName, objectType);
        }

        private static void ValidateLegacyCatalog(SqlConnection connection, SqlTransaction transaction, CatalogMap map)
        {
            int tableCount = ScalarInt(connection, transaction,
                "SELECT COUNT(*) FROM sys.tables WHERE is_ms_shipped=0 AND schema_id=SCHEMA_ID(N'dbo')", null);
            int columnCount = ScalarInt(connection, transaction,
                "SELECT COUNT(*) FROM sys.columns c INNER JOIN sys.tables t ON t.object_id=c.object_id WHERE t.is_ms_shipped=0 AND t.schema_id=SCHEMA_ID(N'dbo')", null);
            int routineCount = ScalarInt(connection, transaction,
                "SELECT COUNT(*) FROM sys.objects WHERE is_ms_shipped=0 AND schema_id=SCHEMA_ID(N'dbo') AND type IN ('P','FN','TF')", null);
            if (tableCount != map.ExpectedTableCount || columnCount != map.ExpectedColumnCount || routineCount != map.ExpectedRoutineCount)
                throw new InvalidOperationException(
                    "The attached reference database does not match the supported schema. Expected " +
                    map.ExpectedTableCount + " tables, " + map.ExpectedColumnCount + " columns and " +
                    map.ExpectedRoutineCount + " stored procedures/functions; found " +
                    tableCount + ", " + columnCount + " and " + routineCount + ".");

            int unsupportedObjects = ScalarInt(connection, transaction,
                "SELECT COUNT(*) FROM sys.objects WHERE is_ms_shipped=0 AND schema_id=SCHEMA_ID(N'dbo') AND type NOT IN ('U','P','FN','TF','D','PK')", null);
            int defaultCount = ScalarInt(connection, transaction,
                "SELECT COUNT(*) FROM sys.objects WHERE is_ms_shipped=0 AND schema_id=SCHEMA_ID(N'dbo') AND type='D'", null);
            int keyCount = ScalarInt(connection, transaction,
                "SELECT COUNT(*) FROM sys.objects WHERE is_ms_shipped=0 AND schema_id=SCHEMA_ID(N'dbo') AND type='PK'", null);
            int indexCount = ScalarInt(connection, transaction,
                "SELECT COUNT(*) FROM sys.indexes i INNER JOIN sys.tables t ON t.object_id=i.object_id WHERE t.is_ms_shipped=0 AND t.schema_id=SCHEMA_ID(N'dbo') AND i.index_id>0", null);
            int foreignKeyCount = ScalarInt(connection, transaction, "SELECT COUNT(*) FROM sys.foreign_keys WHERE schema_id=SCHEMA_ID(N'dbo')", null);
            int checkConstraintCount = ScalarInt(connection, transaction, "SELECT COUNT(*) FROM sys.check_constraints WHERE schema_id=SCHEMA_ID(N'dbo')", null);
            int triggerCount = ScalarInt(connection, transaction,
                "SELECT COUNT(*) FROM sys.triggers tr LEFT JOIN sys.tables t ON tr.parent_class=1 AND t.object_id=tr.parent_id " +
                "WHERE tr.is_ms_shipped=0 AND (tr.parent_class<>1 OR t.schema_id=SCHEMA_ID(N'dbo'))", null);
            int viewCount = ScalarInt(connection, transaction, "SELECT COUNT(*) FROM sys.views WHERE is_ms_shipped=0 AND schema_id=SCHEMA_ID(N'dbo')", null);
            int synonymCount = ScalarInt(connection, transaction, "SELECT COUNT(*) FROM sys.synonyms WHERE schema_id=SCHEMA_ID(N'dbo')", null);
            int sequenceCount = ScalarInt(connection, transaction, "SELECT COUNT(*) FROM sys.sequences WHERE schema_id=SCHEMA_ID(N'dbo')", null);
            int userTypeCount = ScalarInt(connection, transaction, "SELECT COUNT(*) FROM sys.types WHERE is_user_defined=1 AND schema_id=SCHEMA_ID(N'dbo')", null);
            int tablesOutsideSupportedSchemas = ScalarInt(connection, transaction,
                "SELECT COUNT(*) FROM sys.tables WHERE is_ms_shipped=0 AND schema_id NOT IN (SCHEMA_ID(N'dbo'),SCHEMA_ID(N'app'))", null);
            if (unsupportedObjects != 0 || defaultCount != map.ExpectedDefaultConstraintCount ||
                keyCount != map.ExpectedKeyConstraintCount || indexCount != 0 || foreignKeyCount != 0 ||
                checkConstraintCount != 0 || triggerCount != 0 || viewCount != 0 || synonymCount != 0 ||
                sequenceCount != 0 || userTypeCount != 0 || tablesOutsideSupportedSchemas != 0)
                throw new InvalidOperationException(
                    "The attached reference database contains object types not covered by this migration. " +
                    "No schema changes were applied.");

            foreach (TableNameMap table in map.Tables)
            {
                if (!ObjectExists(connection, transaction, "dbo", table.SourceName, "U"))
                    throw new InvalidOperationException("Reference table is missing: dbo." + table.SourceName);

                int actualColumns = ScalarInt(connection, transaction,
                    "SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID(@ObjectName)", new SqlParameter("@ObjectName", SqlDbType.NVarChar, 300)
                    { Value = "dbo." + table.SourceName });
                if (actualColumns != table.Columns.Count)
                    throw new InvalidOperationException("Unexpected column count in reference table dbo." + table.SourceName + ".");

                foreach (ColumnNameMap column in table.Columns)
                {
                    if (!ColumnExists(connection, transaction, "dbo", table.SourceName, column.SourceName))
                        throw new InvalidOperationException("Reference column is missing: dbo." + table.SourceName + "." + column.SourceName);
                }
                if (ObjectExists(connection, transaction, table.TargetSchema, table.TargetName, "U"))
                    throw new InvalidOperationException("Target table already exists without a migration record: " + table.TargetSchema + "." + table.TargetName);
            }

            Dictionary<string, ModuleDefinition> modules = LoadModuleDefinitions(connection, transaction, map);
            if (modules.Count != map.Routines.Count)
                throw new InvalidOperationException("Stored procedure/function definitions do not match the supported reference schema.");
            foreach (RoutineNameMap routine in map.Routines)
            {
                ModuleDefinition definition;
                if (!modules.TryGetValue(routine.SourceName, out definition))
                    throw new InvalidOperationException("Reference SQL module is missing: dbo." + routine.SourceName);
                if (!String.Equals(definition.ObjectType, routine.ObjectType, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Reference SQL module has an unexpected type: dbo." + routine.SourceName);
                if (ObjectExists(connection, transaction, routine.TargetSchema, routine.TargetName, routine.ObjectType))
                    throw new InvalidOperationException("Target SQL module already exists without a migration record: " + routine.TargetSchema + "." + routine.TargetName);
                if (definition.Definition == null)
                    throw new InvalidOperationException("SQL module definition is encrypted or unavailable: dbo." + routine.SourceName);
            }
        }

        private static Dictionary<string, long> CaptureRowCounts(SqlConnection connection, SqlTransaction transaction, CatalogMap map)
        {
            var rowCounts = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            foreach (TableNameMap table in map.Tables)
            {
                using (var command = new SqlCommand(
                    "SELECT COUNT_BIG(1) FROM " + QuoteIdentifier("dbo") + "." + QuoteIdentifier(table.SourceName),
                    connection, transaction))
                {
                    command.CommandTimeout = 300;
                    rowCounts.Add(table.SourceName, Convert.ToInt64(command.ExecuteScalar()));
                }
            }
            return rowCounts;
        }

        private static void ValidateRenamedCatalog(
            SqlConnection connection,
            SqlTransaction transaction,
            CatalogMap map,
            IDictionary<string, long> originalRowCounts)
        {
            foreach (TableNameMap table in map.Tables)
            {
                if (ObjectExists(connection, transaction, "dbo", table.SourceName, "U"))
                    throw new InvalidOperationException("Legacy table remains after migration: dbo." + table.SourceName);
                if (!ObjectExists(connection, transaction, table.TargetSchema, table.TargetName, "U"))
                    throw new InvalidOperationException("Renamed table was not created: " + table.TargetSchema + "." + table.TargetName);
                if (!ObjectExists(connection, transaction, "compat", "Legacy" + table.SourceName, "V"))
                    throw new InvalidOperationException("Legacy compatibility view was not created: compat.Legacy" + table.SourceName);

                int actualColumns = ScalarInt(connection, transaction,
                    "SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID(@ObjectName)",
                    new SqlParameter("@ObjectName", SqlDbType.NVarChar, 300)
                    { Value = QuoteIdentifier(table.TargetSchema) + "." + QuoteIdentifier(table.TargetName) });
                if (actualColumns != table.Columns.Count)
                    throw new InvalidOperationException("Column count changed during migration for " + table.TargetSchema + "." + table.TargetName + ".");
                foreach (ColumnNameMap column in table.Columns)
                {
                    if (!ColumnExists(connection, transaction, table.TargetSchema, table.TargetName, column.TargetName))
                        throw new InvalidOperationException("Renamed column was not created: " + table.TargetSchema + "." + table.TargetName + "." + column.TargetName);
                    if (!ColumnExists(connection, transaction, "compat", "Legacy" + table.SourceName, column.SourceName))
                        throw new InvalidOperationException("Legacy column is missing from compatibility view compat.Legacy" + table.SourceName + ".");
                }

                using (var command = new SqlCommand(
                    "SELECT COUNT_BIG(1) FROM " + QuoteIdentifier(table.TargetSchema) + "." + QuoteIdentifier(table.TargetName),
                    connection, transaction))
                {
                    command.CommandTimeout = 300;
                    long migratedRows = Convert.ToInt64(command.ExecuteScalar());
                    if (migratedRows != originalRowCounts[table.SourceName])
                        throw new InvalidOperationException("Row count changed during migration for " + table.TargetSchema + "." + table.TargetName + ".");
                }
            }

            foreach (RoutineNameMap routine in map.Routines)
            {
                if (ObjectExists(connection, transaction, "dbo", routine.SourceName, routine.ObjectType))
                    throw new InvalidOperationException("Legacy SQL module remains after migration: dbo." + routine.SourceName);
                if (!ObjectExists(connection, transaction, routine.TargetSchema, routine.TargetName, routine.ObjectType))
                    throw new InvalidOperationException("Renamed SQL module was not created: " + routine.TargetSchema + "." + routine.TargetName);
            }
        }

        private static Dictionary<string, ModuleDefinition> LoadModuleDefinitions(SqlConnection connection, SqlTransaction transaction, CatalogMap map)
        {
            var definitions = new Dictionary<string, ModuleDefinition>(StringComparer.OrdinalIgnoreCase);
            using (var command = new SqlCommand(
                "SELECT o.name,o.type,sm.definition,sm.uses_ansi_nulls,sm.uses_quoted_identifier " +
                "FROM sys.objects o INNER JOIN sys.schemas s ON s.schema_id=o.schema_id " +
                "INNER JOIN sys.sql_modules sm ON sm.object_id=o.object_id " +
                "WHERE s.name=N'dbo' AND o.is_ms_shipped=0 AND o.type IN ('P','FN','TF')", connection, transaction))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var definition = new ModuleDefinition(
                        reader.GetString(0),
                        reader.GetString(1).Trim(),
                        reader.IsDBNull(2) ? null : reader.GetString(2),
                        reader.GetBoolean(3),
                        reader.GetBoolean(4));
                    definitions.Add(definition.SourceName, definition);
                }
            }
            return definitions;
        }

        private static void MoveAndRenameTables(SqlConnection connection, SqlTransaction transaction, CatalogMap map)
        {
            foreach (TableNameMap table in map.Tables)
            {
                RenameObject(connection, transaction, "dbo", table.SourceName, table.TargetName);
                Execute(connection, transaction,
                    "ALTER SCHEMA " + QuoteIdentifier(table.TargetSchema) + " TRANSFER " +
                    QuoteIdentifier("dbo") + "." + QuoteIdentifier(table.TargetName) + ";");

                foreach (ColumnNameMap column in table.Columns)
                    RenameColumn(connection, transaction, table.TargetSchema, table.TargetName, column.SourceName, column.TargetName);
            }
        }

        private static void CreateCompatibilityViews(SqlConnection connection, SqlTransaction transaction, CatalogMap map)
        {
            foreach (TableNameMap table in map.Tables)
            {
                var projections = new List<string>();
                foreach (ColumnNameMap column in table.Columns)
                    projections.Add(QuoteIdentifier(column.TargetName) + " AS " + QuoteIdentifier(column.SourceName));

                string sql = "CREATE VIEW " + QuoteIdentifier("compat") + "." +
                    QuoteIdentifier("Legacy" + table.SourceName) + " AS SELECT " +
                    String.Join(", ", projections) + " FROM " +
                    QuoteIdentifier(table.TargetSchema) + "." + QuoteIdentifier(table.TargetName) + ";";
                Execute(connection, transaction, sql);
            }
        }

        private static void MoveAndRenameModules(
            SqlConnection connection,
            SqlTransaction transaction,
            CatalogMap map,
            Dictionary<string, ModuleDefinition> modules)
        {
            foreach (RoutineNameMap routine in map.Routines)
            {
                ModuleDefinition definition = modules[routine.SourceName];
                Execute(connection, transaction,
                    "ALTER SCHEMA " + QuoteIdentifier(routine.TargetSchema) + " TRANSFER " +
                    QuoteIdentifier("dbo") + "." + QuoteIdentifier(routine.SourceName) + ";");
                RenameObject(connection, transaction, routine.TargetSchema, routine.SourceName, routine.TargetName);

                string updatedDefinition = RewriteModuleDefinition(definition.Definition, map);
                using (var command = new SqlCommand("SET ANSI_NULLS " + (definition.UsesAnsiNulls ? "ON" : "OFF") + ";", connection, transaction))
                    command.ExecuteNonQuery();
                using (var command = new SqlCommand("SET QUOTED_IDENTIFIER " + (definition.UsesQuotedIdentifier ? "ON" : "OFF") + ";", connection, transaction))
                    command.ExecuteNonQuery();
                try
                {
                    Execute(connection, transaction, updatedDefinition);
                }
                catch (SqlException ex)
                {
                    throw new InvalidOperationException(
                        "Could not migrate SQL module dbo." + routine.SourceName + " to " +
                        routine.TargetSchema + "." + routine.TargetName + ": " + ex.Message, ex);
                }
            }
        }

        private static string RewriteModuleDefinition(string definition, CatalogMap map)
        {
            string result = TransformCodeSegments(definition, delegate(string code)
            {
                string transformed = code;
                foreach (TableNameMap table in map.Tables)
                {
                    string sourceIdentifier = @"(?:\[\s*" + Regex.Escape(table.SourceName) + @"\s*\]|" + Regex.Escape(table.SourceName) + @")";
                    string unqualifiedSource = @"(?i)\b(?:FROM|JOIN|UPDATE|INTO|INSERT(?:\s+INTO)?|MERGE(?:\s+INTO)?|DELETE(?:\s+FROM)?)\s+" +
                        sourceIdentifier + @"(?![\w])";
                    string aliasClause = @"(?i)\b(?:FROM|JOIN|UPDATE|INTO|INSERT(?:\s+INTO)?|MERGE(?:\s+INTO)?|DELETE(?:\s+FROM)?)\s+" +
                        sourceIdentifier + @"\s+(?:AS\s+)?(?<alias>[A-Za-z_][\w$#]*)";
                    string[] sqlKeywords = { "AS", "WHERE", "ON", "JOIN", "INNER", "LEFT", "RIGHT", "FULL", "CROSS", "OUTER", "GROUP", "ORDER", "HAVING", "UNION", "EXCEPT", "INTERSECT", "WITH", "OPTION", "FOR", "SET", "VALUES", "OUTPUT", "WHEN", "USING", "AND", "OR", "NOT", "RETURN", "END" };
                    bool unqualifiedSourceIsUsed = Regex.IsMatch(transformed, unqualifiedSource, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    bool hasExplicitAlias = Regex.Matches(transformed, aliasClause, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
                        .Cast<Match>()
                        .Any(match => !sqlKeywords.Contains(match.Groups["alias"].Value, StringComparer.OrdinalIgnoreCase));

                    string tableReference = @"(?i)(\b(?:FROM|JOIN|UPDATE|INTO|INSERT(?:\s+INTO)?|MERGE(?:\s+INTO)?|DELETE(?:\s+FROM)?|TRUNCATE\s+TABLE)\s+)" +
                        @"(?:(?:\[\s*dbo\s*\]|\bdbo)\s*\.\s*)?(?:\[\s*" +
                        Regex.Escape(table.SourceName) + @"\s*\]|" + Regex.Escape(table.SourceName) + @")(?![\w])";
                    transformed = Regex.Replace(transformed, tableReference,
                        "$1" + QuoteIdentifier("compat") + "." + QuoteIdentifier("Legacy" + table.SourceName),
                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                    string qualifiedTableReference = @"(?i)(?<![\w@.])(?:\[\s*dbo\s*\]|\bdbo)\s*\.\s*" +
                        @"(?:\[\s*" + Regex.Escape(table.SourceName) + @"\s*\]|" + Regex.Escape(table.SourceName) + @")(?![\w])(?=\s*\.)";
                    transformed = Regex.Replace(transformed, qualifiedTableReference,
                        QuoteIdentifier("compat") + "." + QuoteIdentifier("Legacy" + table.SourceName),
                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                    if (unqualifiedSourceIsUsed && !hasExplicitAlias)
                    {
                        string unqualifiedTableQualifier = @"(?i)(?<![\w@.])" + sourceIdentifier + @"(?![\w])(?=\s*\.)";
                        transformed = Regex.Replace(transformed, unqualifiedTableQualifier,
                            QuoteIdentifier("compat") + "." + QuoteIdentifier("Legacy" + table.SourceName),
                            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    }
                }

                foreach (RoutineNameMap routine in map.Routines)
                {
                    string objectName = @"(?:\[\s*" + Regex.Escape(routine.SourceName) + @"\s*\]|" +
                        Regex.Escape(routine.SourceName) + @")";
                    string declaration = @"(?i)(\b(?:CREATE|ALTER)\s+(?:PROCEDURE|PROC|FUNCTION)\s+)" +
                        @"(?:(?:\[\s*dbo\s*\]|\bdbo)\s*\.\s*)?" + objectName + @"(?![\w])";
                    transformed = Regex.Replace(transformed, declaration,
                        "$1" + QuoteIdentifier(routine.TargetSchema) + "." + QuoteIdentifier(routine.TargetName),
                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                    if (String.Equals(routine.ObjectType, "P", StringComparison.OrdinalIgnoreCase))
                    {
                        string procedureCall = @"(?i)(\bEXEC(?:UTE)?\s+(?:@[\w$#]+\s*=\s*)?)" +
                            @"(?:(?:\[\s*dbo\s*\]|\bdbo)\s*\.\s*)?" + objectName + @"(?![\w])";
                        transformed = Regex.Replace(transformed, procedureCall,
                            "$1" + QuoteIdentifier(routine.TargetSchema) + "." + QuoteIdentifier(routine.TargetName),
                            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    }
                    else
                    {
                        string functionCall = @"(?i)(?<![\w@.])(?:(?:\[\s*dbo\s*\]|\bdbo)\s*\.\s*)?" +
                            objectName + @"(?![\w])(?=\s*\()";
                        transformed = Regex.Replace(transformed, functionCall,
                            QuoteIdentifier(routine.TargetSchema) + "." + QuoteIdentifier(routine.TargetName),
                            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    }
                }

                transformed = Regex.Replace(transformed,
                    @"(?i)\bCREATE(?=\s+(?:PROCEDURE|PROC|FUNCTION)\b)",
                    "ALTER",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                return transformed;
            });

            if (map.Routines.Any(x => String.Equals(x.SourceName, "spHourSharjSetInfo", StringComparison.OrdinalIgnoreCase)))
            {
                // This legacy routine filters PosTable by IsHourSharj, a column absent from the reference table.
                // The hourly recharge routine already receives the exact POS row ID, so the invalid predicate is redundant.
                result = TransformCodeSegments(result, delegate(string code)
                {
                    return Regex.Replace(code, @"(?i)\s+AND\s+IsHourSharj\s*=\s*1\b", "",
                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                });
            }

            if (String.Equals(map.SpecialFunctionSourceName, "fnSplit", StringComparison.OrdinalIgnoreCase) &&
                Regex.IsMatch(result, @"(?i)\bRETURNS\s+@\w+\s+TABLE\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                Match functionReturnTable = Regex.Match(result,
                    @"(?is)(RETURNS\s+@\w+\s+TABLE\s*\()(.*?)(\)\s*(?:--[^\r\n]*(?:\r?\n|$)\s*)?AS\b)",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                if (!functionReturnTable.Success)
                {
                    int returnPosition = result.IndexOf("RETURNS", StringComparison.OrdinalIgnoreCase);
                    string excerpt = returnPosition < 0 ? result.Substring(0, Math.Min(300, result.Length)) :
                        result.Substring(returnPosition, Math.Min(400, result.Length - returnPosition));
                    throw new InvalidOperationException("The delimited-value function return table could not be located for its key-constraint rename. Definition excerpt: " + excerpt);
                }

                string returnColumns = Regex.Replace(functionReturnTable.Groups[2].Value,
                    @"(?i)(\bposition\s+int\s+IDENTITY)\s+PRIMARY\s+KEY\b", "$1",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                if (returnColumns == functionReturnTable.Groups[2].Value)
                    throw new InvalidOperationException("The delimited-value function key definition did not match the supported reference schema.");
                // SQL Server does not allow a named key constraint in an inline TVF return table.
                // The identity value remains unique and available to callers for explicit ordering.
                result = result.Substring(0, functionReturnTable.Index) +
                    functionReturnTable.Groups[1].Value + returnColumns +
                    functionReturnTable.Groups[3].Value +
                    result.Substring(functionReturnTable.Index + functionReturnTable.Length);
            }
            return result;
        }

        private static string TransformCodeSegments(string source, Func<string, string> transform)
        {
            var output = new StringBuilder(source.Length + 128);
            var code = new StringBuilder();
            int index = 0;
            while (index < source.Length)
            {
                if (source[index] == '\'' || source[index] == '"')
                {
                    FlushCode(output, code, transform);
                    int end = FindQuotedEnd(source, index, source[index]);
                    output.Append(source, index, end - index);
                    index = end;
                }
                else if (index + 1 < source.Length && source[index] == '-' && source[index + 1] == '-')
                {
                    FlushCode(output, code, transform);
                    int end = index + 2;
                    while (end < source.Length && source[end] != '\r' && source[end] != '\n') end++;
                    output.Append(source, index, end - index);
                    index = end;
                }
                else if (index + 1 < source.Length && source[index] == '/' && source[index + 1] == '*')
                {
                    FlushCode(output, code, transform);
                    int end = FindBlockCommentEnd(source, index);
                    output.Append(source, index, end - index);
                    index = end;
                }
                else
                {
                    code.Append(source[index]);
                    index++;
                }
            }
            FlushCode(output, code, transform);
            return output.ToString();
        }

        private static int FindQuotedEnd(string source, int start, char quote)
        {
            int index = start + 1;
            while (index < source.Length)
            {
                if (source[index] == quote)
                {
                    if (index + 1 < source.Length && source[index + 1] == quote)
                    {
                        index += 2;
                        continue;
                    }
                    return index + 1;
                }
                index++;
            }
            return source.Length;
        }

        private static int FindBlockCommentEnd(string source, int start)
        {
            int depth = 1;
            int index = start + 2;
            while (index < source.Length && depth > 0)
            {
                if (index + 1 < source.Length && source[index] == '/' && source[index + 1] == '*')
                {
                    depth++;
                    index += 2;
                }
                else if (index + 1 < source.Length && source[index] == '*' && source[index + 1] == '/')
                {
                    depth--;
                    index += 2;
                }
                else index++;
            }
            return index;
        }

        private static void FlushCode(StringBuilder output, StringBuilder code, Func<string, string> transform)
        {
            if (code.Length == 0) return;
            output.Append(transform(code.ToString()));
            code.Clear();
        }

        private static void RenameDefaultConstraints(SqlConnection connection, SqlTransaction transaction, CatalogMap map)
        {
            foreach (TableNameMap table in map.Tables)
            {
                var constraints = new List<DefaultConstraintName>();
                using (var command = new SqlCommand(
                    "SELECT os.name,dc.name,c.name FROM sys.default_constraints dc " +
                    "INNER JOIN sys.tables t ON t.object_id=dc.parent_object_id " +
                    "INNER JOIN sys.schemas ts ON ts.schema_id=t.schema_id " +
                    "INNER JOIN sys.columns c ON c.object_id=t.object_id AND c.column_id=dc.parent_column_id " +
                    "INNER JOIN sys.schemas os ON os.schema_id=dc.schema_id " +
                    "WHERE ts.name=@SchemaName AND t.name=@TableName", connection, transaction))
                {
                    command.Parameters.Add("@SchemaName", SqlDbType.NVarChar, 128).Value = table.TargetSchema;
                    command.Parameters.Add("@TableName", SqlDbType.NVarChar, 128).Value = table.TargetName;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            constraints.Add(new DefaultConstraintName(reader.GetString(0), reader.GetString(1), reader.GetString(2)));
                    }
                }

                foreach (DefaultConstraintName constraint in constraints)
                {
                    string desiredName = "DF_" + table.TargetName + "_" + constraint.ColumnName;
                    if (String.Equals(constraint.Name, desiredName, StringComparison.OrdinalIgnoreCase)) continue;
                    RenameObject(connection, transaction, constraint.SchemaName, constraint.Name, desiredName);
                }
            }
        }

        private static void RenameObject(SqlConnection connection, SqlTransaction transaction, string schema, string oldName, string newName)
        {
            using (var command = new SqlCommand(
                "EXEC sys.sp_rename @objname=@ObjectName,@newname=@NewName,@objtype=N'OBJECT';", connection, transaction))
            {
                command.Parameters.Add("@ObjectName", SqlDbType.NVarChar, 776).Value =
                    QuoteIdentifier(schema) + "." + QuoteIdentifier(oldName);
                command.Parameters.Add("@NewName", SqlDbType.NVarChar, 128).Value = newName;
                command.ExecuteNonQuery();
            }
        }

        private static void RenameColumn(SqlConnection connection, SqlTransaction transaction, string schema, string table, string oldName, string newName)
        {
            if (String.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase)) return;
            using (var command = new SqlCommand(
                "EXEC sys.sp_rename @objname=@ObjectName,@newname=@NewName,@objtype=N'COLUMN';", connection, transaction))
            {
                command.Parameters.Add("@ObjectName", SqlDbType.NVarChar, 776).Value =
                    QuoteIdentifier(schema) + "." + QuoteIdentifier(table) + "." + QuoteIdentifier(oldName);
                command.Parameters.Add("@NewName", SqlDbType.NVarChar, 128).Value = newName;
                command.ExecuteNonQuery();
            }
        }

        private static void EnsureSchema(SqlConnection connection, SqlTransaction transaction, string schema)
        {
            using (var command = new SqlCommand("SELECT CASE WHEN SCHEMA_ID(@SchemaName) IS NULL THEN 0 ELSE 1 END", connection, transaction))
            {
                command.Parameters.Add("@SchemaName", SqlDbType.NVarChar, 128).Value = schema;
                if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                    Execute(connection, transaction, "CREATE SCHEMA " + QuoteIdentifier(schema) + ";");
            }
        }

        private static void EnsureMigrationHistory(SqlConnection connection, SqlTransaction transaction)
        {
            Execute(connection, transaction, @"
IF OBJECT_ID(N'app.SchemaMigrationHistory',N'U') IS NULL
BEGIN
 CREATE TABLE app.SchemaMigrationHistory
 (
  MigrationVersion int NOT NULL CONSTRAINT PK_SchemaMigrationHistory PRIMARY KEY,
  AppliedAt datetime2(0) NOT NULL CONSTRAINT DF_SchemaMigrationHistory_AppliedAt DEFAULT(SYSDATETIME()),
  Description nvarchar(240) NOT NULL
 );
END");
        }

        private static bool IsMigrationApplied(SqlConnection connection, SqlTransaction transaction, int version)
        {
            using (var command = new SqlCommand(
                "SELECT CASE WHEN EXISTS(SELECT 1 FROM app.SchemaMigrationHistory WHERE MigrationVersion=@Version) THEN 1 ELSE 0 END",
                connection, transaction))
            {
                command.Parameters.Add("@Version", SqlDbType.Int).Value = version;
                return Convert.ToInt32(command.ExecuteScalar()) == 1;
            }
        }

        private static void InsertMigrationHistory(SqlConnection connection, SqlTransaction transaction, int version, string description)
        {
            using (var command = new SqlCommand(
                "IF NOT EXISTS(SELECT 1 FROM app.SchemaMigrationHistory WHERE MigrationVersion=@Version) " +
                "INSERT app.SchemaMigrationHistory(MigrationVersion,Description) VALUES(@Version,@Description)",
                connection, transaction))
            {
                command.Parameters.Add("@Version", SqlDbType.Int).Value = version;
                command.Parameters.Add("@Description", SqlDbType.NVarChar, 240).Value = description;
                command.ExecuteNonQuery();
            }
        }

        private static bool ObjectExists(SqlConnection connection, SqlTransaction transaction, string schema, string name, string type)
        {
            using (var command = new SqlCommand(
                "SELECT CASE WHEN EXISTS(SELECT 1 FROM sys.objects WHERE schema_id=SCHEMA_ID(@SchemaName) AND name=@ObjectName AND type=@ObjectType) THEN 1 ELSE 0 END",
                connection, transaction))
            {
                command.Parameters.Add("@SchemaName", SqlDbType.NVarChar, 128).Value = schema;
                command.Parameters.Add("@ObjectName", SqlDbType.NVarChar, 128).Value = name;
                command.Parameters.Add("@ObjectType", SqlDbType.Char, 2).Value = type;
                return Convert.ToInt32(command.ExecuteScalar()) == 1;
            }
        }

        private static bool ColumnExists(SqlConnection connection, SqlTransaction transaction, string schema, string table, string column)
        {
            using (var command = new SqlCommand(
                "SELECT CASE WHEN EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@ObjectName) AND name=@ColumnName) THEN 1 ELSE 0 END",
                connection, transaction))
            {
                command.Parameters.Add("@ObjectName", SqlDbType.NVarChar, 300).Value =
                    QuoteIdentifier(schema) + "." + QuoteIdentifier(table);
                command.Parameters.Add("@ColumnName", SqlDbType.NVarChar, 128).Value = column;
                return Convert.ToInt32(command.ExecuteScalar()) == 1;
            }
        }

        private static int ScalarInt(SqlConnection connection, SqlTransaction transaction, string sql, SqlParameter parameter)
        {
            using (var command = new SqlCommand(sql, connection, transaction))
            {
                if (parameter != null) command.Parameters.Add(parameter);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static void Execute(SqlConnection connection, SqlTransaction transaction, string sql)
        {
            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.CommandTimeout = 180;
                command.ExecuteNonQuery();
            }
        }

        private static string QuoteIdentifier(string value)
        {
            return "[" + value.Replace("]", "]]") + "]";
        }

        private sealed class CatalogMap
        {
            public int ExpectedTableCount { get; private set; }
            public int ExpectedColumnCount { get; private set; }
            public int ExpectedRoutineCount { get; private set; }
            public int ExpectedDefaultConstraintCount { get; private set; }
            public int ExpectedKeyConstraintCount { get; private set; }
            public IList<TableNameMap> Tables { get; private set; }
            public IList<RoutineNameMap> Routines { get; private set; }
            public string SpecialFunctionSourceName { get; private set; }

            public CatalogMap(int tableCount, int columnCount, int routineCount, int defaultCount, int keyCount,
                IList<TableNameMap> tables, IList<RoutineNameMap> routines)
            {
                ExpectedTableCount = tableCount;
                ExpectedColumnCount = columnCount;
                ExpectedRoutineCount = routineCount;
                ExpectedDefaultConstraintCount = defaultCount;
                ExpectedKeyConstraintCount = keyCount;
                Tables = tables;
                Routines = routines;
                SpecialFunctionSourceName = routines.Any(x => String.Equals(x.SourceName, "fnSplit", StringComparison.OrdinalIgnoreCase))
                    ? "fnSplit"
                    : null;
            }
        }

        private sealed class TableNameMap
        {
            public string SourceName { get; private set; }
            public string TargetSchema { get; private set; }
            public string TargetName { get; private set; }
            public IList<ColumnNameMap> Columns { get; private set; }

            public TableNameMap(string source, string targetSchema, string target, IList<ColumnNameMap> columns)
            {
                SourceName = source;
                TargetSchema = targetSchema;
                TargetName = target;
                Columns = columns;
            }
        }

        private sealed class ColumnNameMap
        {
            public string SourceName { get; private set; }
            public string TargetName { get; private set; }
            public ColumnNameMap(string source, string target) { SourceName = source; TargetName = target; }
        }

        private sealed class RoutineNameMap
        {
            public string SourceName { get; private set; }
            public string TargetSchema { get; private set; }
            public string TargetName { get; private set; }
            public string ObjectType { get; private set; }
            public RoutineNameMap(string source, string schema, string target, string type)
            {
                SourceName = source;
                TargetSchema = schema;
                TargetName = target;
                ObjectType = type;
            }
        }

        private sealed class ModuleDefinition
        {
            public string SourceName { get; private set; }
            public string ObjectType { get; private set; }
            public string Definition { get; private set; }
            public bool UsesAnsiNulls { get; private set; }
            public bool UsesQuotedIdentifier { get; private set; }
            public ModuleDefinition(string source, string type, string definition, bool ansiNulls, bool quotedIdentifier)
            {
                SourceName = source;
                ObjectType = type;
                Definition = definition;
                UsesAnsiNulls = ansiNulls;
                UsesQuotedIdentifier = quotedIdentifier;
            }
        }

        private sealed class DefaultConstraintName
        {
            public string SchemaName { get; private set; }
            public string Name { get; private set; }
            public string ColumnName { get; private set; }
            public DefaultConstraintName(string schema, string name, string column)
            {
                SchemaName = schema;
                Name = name;
                ColumnName = column;
            }
        }
    }
}
