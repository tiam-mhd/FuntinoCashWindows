using System.Data.SqlClient;

namespace FuntinoControlCenter.Database
{
    internal static class EdgeSchemaMigrator
    {
        public static void Ensure(SqlConnection connection, SqlTransaction transaction, bool deviceDatabase)
        {
            Execute(connection, transaction, @"
IF OBJECT_ID(N'app.EdgeOutbox',N'U') IS NULL
BEGIN
 CREATE TABLE app.EdgeOutbox
 (
  EventId uniqueidentifier NOT NULL CONSTRAINT PK_EdgeOutbox PRIMARY KEY,
  LocalTransactionId uniqueidentifier NOT NULL,
  SourceInstanceId uniqueidentifier NOT NULL,
  SourceDatabase nvarchar(128) NOT NULL,
  OrganizationId nvarchar(128) NULL,
  BranchId nvarchar(128) NULL,
  CardId nvarchar(128) NULL,
  HouseholdId nvarchar(128) NULL,
  MemberId nvarchar(128) NULL,
  DeviceId nvarchar(128) NULL,
  SourceDeviceId nvarchar(128) NULL,
  TransactionKind varchar(24) NOT NULL,
  AmountRial bigint NOT NULL,
  CurrencyCode char(3) NOT NULL CONSTRAINT DF_EdgeOutbox_CurrencyCode DEFAULT('IRR'),
  GrossAmountRial bigint NOT NULL,
  DiscountAmountRial bigint NOT NULL CONSTRAINT DF_EdgeOutbox_DiscountAmount DEFAULT(0),
  NetAmountRial bigint NOT NULL,
  PromotionId nvarchar(128) NULL,
  PromotionCode int NULL,
  PromotionDiscountPercent int NULL,
  DailyDiscountPercent int NULL,
  DiscountTierId int NULL,
  PricingVersion nvarchar(128) NOT NULL,
  OccurredAtUtc datetime2(3) NOT NULL,
  LocalSequence bigint NOT NULL,
  BalanceAfterRial bigint NULL,
  OperationResult varchar(16) NOT NULL,
  ApplicationUserId int NOT NULL,
  OperatorUserName nvarchar(64) NOT NULL,
  CardKeyHash varbinary(32) NULL,
  Status varchar(24) NOT NULL CONSTRAINT DF_EdgeOutbox_Status DEFAULT('pending'),
  RetryCount int NOT NULL CONSTRAINT DF_EdgeOutbox_RetryCount DEFAULT(0),
  NextAttemptAtUtc datetime2(3) NULL,
  LastAttemptAtUtc datetime2(3) NULL,
  AcknowledgedAtUtc datetime2(3) NULL,
  Acknowledgement varchar(16) NULL,
  LastError nvarchar(500) NULL,
  CreatedAtUtc datetime2(3) NOT NULL CONSTRAINT DF_EdgeOutbox_CreatedAtUtc DEFAULT(SYSUTCDATETIME()),
  CONSTRAINT UQ_EdgeOutbox_LocalTransactionId UNIQUE(LocalTransactionId)
 );
 CREATE INDEX IX_EdgeOutbox_Delivery ON app.EdgeOutbox(Status,NextAttemptAtUtc,CreatedAtUtc);
 CREATE INDEX IX_EdgeOutbox_Branch ON app.EdgeOutbox(BranchId,Status,CreatedAtUtc);
END;
IF COL_LENGTH(N'app.EdgeOutbox',N'PromotionCode') IS NULL ALTER TABLE app.EdgeOutbox ADD PromotionCode int NULL;
IF COL_LENGTH(N'app.EdgeOutbox',N'PromotionDiscountPercent') IS NULL ALTER TABLE app.EdgeOutbox ADD PromotionDiscountPercent int NULL;
IF COL_LENGTH(N'app.EdgeOutbox',N'DailyDiscountPercent') IS NULL ALTER TABLE app.EdgeOutbox ADD DailyDiscountPercent int NULL;
IF COL_LENGTH(N'app.EdgeOutbox',N'DiscountTierId') IS NULL ALTER TABLE app.EdgeOutbox ADD DiscountTierId int NULL;
IF OBJECT_ID(N'app.EdgeSyncState',N'U') IS NULL
BEGIN
 CREATE TABLE app.EdgeSyncState
 (
  StateId tinyint NOT NULL CONSTRAINT PK_EdgeSyncState PRIMARY KEY,
  BranchId nvarchar(128) NULL,
  IsOnline bit NOT NULL CONSTRAINT DF_EdgeSyncState_IsOnline DEFAULT(0),
  LastServerResponseUtc datetime2(3) NULL,
  LastSuccessfulSyncUtc datetime2(3) NULL,
  LastSnapshotAtUtc datetime2(3) NULL,
  SnapshotVersion nvarchar(128) NULL,
  SnapshotExpiresAtUtc datetime2(3) NULL,
  LastAcknowledgedEventId uniqueidentifier NULL,
  LastError nvarchar(500) NULL,
  HeartbeatUtc datetime2(3) NULL,
  CONSTRAINT CK_EdgeSyncState_Singleton CHECK(StateId=1)
 );
 INSERT app.EdgeSyncState(StateId) VALUES(1);
END;");

            if (deviceDatabase) return;

            Execute(connection, transaction, @"
IF OBJECT_ID(N'app.EdgeInstallation',N'U') IS NULL
BEGIN
 CREATE TABLE app.EdgeInstallation
 (
  StateId tinyint NOT NULL CONSTRAINT PK_EdgeInstallation PRIMARY KEY,
  SourceInstanceId uniqueidentifier NOT NULL,
  OrganizationId nvarchar(128) NOT NULL,
  BranchId nvarchar(128) NOT NULL,
  CredentialId nvarchar(128) NOT NULL,
  UpdatedByUserId int NOT NULL,
  UpdatedByUserName nvarchar(64) NOT NULL,
  UpdatedAtUtc datetime2(3) NOT NULL CONSTRAINT DF_EdgeInstallation_UpdatedAtUtc DEFAULT(SYSUTCDATETIME()),
  CONSTRAINT CK_EdgeInstallation_Singleton CHECK(StateId=1)
 );
END;
IF COL_LENGTH(N'app.CardOperationLog',N'LocalTransactionId') IS NULL
 ALTER TABLE app.CardOperationLog ADD LocalTransactionId uniqueidentifier NOT NULL CONSTRAINT DF_CardOperationLog_LocalTransactionId DEFAULT(NEWID());
IF COL_LENGTH(N'app.CardOperationLog',N'EventId') IS NULL
 ALTER TABLE app.CardOperationLog ADD EventId uniqueidentifier NOT NULL CONSTRAINT DF_CardOperationLog_EventId DEFAULT(NEWID());
IF COL_LENGTH(N'app.CardOperationLog',N'CardKeyHash') IS NULL ALTER TABLE app.CardOperationLog ADD CardKeyHash varbinary(32) NULL;
IF COL_LENGTH(N'app.CardOperationLog',N'StartingBalanceToman') IS NULL ALTER TABLE app.CardOperationLog ADD StartingBalanceToman bigint NULL;
IF COL_LENGTH(N'app.CardOperationLog',N'ExpectedBalanceToman') IS NULL ALTER TABLE app.CardOperationLog ADD ExpectedBalanceToman bigint NULL;
IF COL_LENGTH(N'app.CardOperationLog',N'PromotionCode') IS NULL ALTER TABLE app.CardOperationLog ADD PromotionCode int NULL;
IF COL_LENGTH(N'app.CardOperationLog',N'PromotionDiscountPercent') IS NULL ALTER TABLE app.CardOperationLog ADD PromotionDiscountPercent int NULL;
IF COL_LENGTH(N'app.CardOperationLog',N'PromotionName') IS NULL ALTER TABLE app.CardOperationLog ADD PromotionName nvarchar(160) NULL;
IF COL_LENGTH(N'app.CardOperationLog',N'DailyDiscountPercent') IS NULL ALTER TABLE app.CardOperationLog ADD DailyDiscountPercent int NULL;
IF COL_LENGTH(N'app.CardOperationLog',N'DiscountTierId') IS NULL ALTER TABLE app.CardOperationLog ADD DiscountTierId int NULL;
IF COL_LENGTH(N'app.CardOperationLog',N'PricingVersion') IS NULL ALTER TABLE app.CardOperationLog ADD PricingVersion nvarchar(128) NULL;
IF COL_LENGTH(N'app.CardOperationLog',N'OccurredAtUtc') IS NULL ALTER TABLE app.CardOperationLog ADD OccurredAtUtc datetime2(3) NOT NULL CONSTRAINT DF_CardOperationLog_OccurredAtUtc DEFAULT(SYSUTCDATETIME());
IF COL_LENGTH(N'app.CardOperationLog',N'OrganizationId') IS NULL ALTER TABLE app.CardOperationLog ADD OrganizationId nvarchar(128) NULL;
IF COL_LENGTH(N'app.CardOperationLog',N'BranchId') IS NULL ALTER TABLE app.CardOperationLog ADD BranchId nvarchar(128) NULL;
IF COL_LENGTH(N'app.CardOperationLog',N'OperatorUserName') IS NULL ALTER TABLE app.CardOperationLog ADD OperatorUserName nvarchar(64) NULL;
IF COL_LENGTH(N'app.CardOperationLog',N'SourceInstanceId') IS NULL ALTER TABLE app.CardOperationLog ADD SourceInstanceId uniqueidentifier NULL;
IF COL_LENGTH(N'app.CardOperationLog',N'LocalSequence') IS NULL ALTER TABLE app.CardOperationLog ADD LocalSequence bigint NULL;
IF COL_LENGTH(N'app.CardOperationLog',N'BalanceAfterRial') IS NULL ALTER TABLE app.CardOperationLog ADD BalanceAfterRial bigint NULL;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'app.CardOperationLog') AND name=N'UX_CardOperationLog_LocalTransactionId')
 CREATE UNIQUE INDEX UX_CardOperationLog_LocalTransactionId ON app.CardOperationLog(LocalTransactionId);

IF OBJECT_ID(N'app.EdgeSnapshotState',N'U') IS NULL
BEGIN
 CREATE TABLE app.EdgeSnapshotState
 (
  StateId tinyint NOT NULL CONSTRAINT PK_EdgeSnapshotState PRIMARY KEY,
  BranchId nvarchar(128) NOT NULL,
  SnapshotVersion nvarchar(128) NOT NULL,
  IssuedAtUtc datetime2(3) NOT NULL,
  ExpiresAtUtc datetime2(3) NOT NULL,
  CursorValue nvarchar(1000) NULL,
  ValidatedAtUtc datetime2(3) NOT NULL,
  CONSTRAINT CK_EdgeSnapshotState_Singleton CHECK(StateId=1)
 );
END;
IF OBJECT_ID(N'app.EdgeCustomerCache',N'U') IS NULL
 CREATE TABLE app.EdgeCustomerCache(BranchId nvarchar(128) NOT NULL,CustomerId nvarchar(128) NOT NULL,DisplayName nvarchar(160) NOT NULL,HouseholdId nvarchar(128) NULL,SnapshotVersion nvarchar(128) NOT NULL,CONSTRAINT PK_EdgeCustomerCache PRIMARY KEY(BranchId,CustomerId));
IF OBJECT_ID(N'app.EdgeHouseholdCache',N'U') IS NULL
 CREATE TABLE app.EdgeHouseholdCache(BranchId nvarchar(128) NOT NULL,HouseholdId nvarchar(128) NOT NULL,DisplayName nvarchar(160) NOT NULL,GuardianCustomerId nvarchar(128) NULL,SnapshotVersion nvarchar(128) NOT NULL,CONSTRAINT PK_EdgeHouseholdCache PRIMARY KEY(BranchId,HouseholdId));
IF OBJECT_ID(N'app.EdgeMemberCache',N'U') IS NULL
 CREATE TABLE app.EdgeMemberCache(BranchId nvarchar(128) NOT NULL,MemberId nvarchar(128) NOT NULL,HouseholdId nvarchar(128) NOT NULL,CustomerId nvarchar(128) NULL,DisplayName nvarchar(160) NOT NULL,SnapshotVersion nvarchar(128) NOT NULL,CONSTRAINT PK_EdgeMemberCache PRIMARY KEY(BranchId,MemberId));
IF OBJECT_ID(N'app.EdgeCardMapping',N'U') IS NULL
 CREATE TABLE app.EdgeCardMapping(BranchId nvarchar(128) NOT NULL,CardKeyHash varbinary(32) NOT NULL,CardId nvarchar(128) NOT NULL,HouseholdId nvarchar(128) NULL,MemberId nvarchar(128) NULL,SnapshotVersion nvarchar(128) NOT NULL,CONSTRAINT PK_EdgeCardMapping PRIMARY KEY(BranchId,CardKeyHash));
IF OBJECT_ID(N'app.EdgeDeviceMapping',N'U') IS NULL
 CREATE TABLE app.EdgeDeviceMapping(BranchId nvarchar(128) NOT NULL,SourceDeviceId nvarchar(128) NOT NULL,DeviceId nvarchar(128) NOT NULL,AttractionId nvarchar(128) NULL,SnapshotVersion nvarchar(128) NOT NULL,CONSTRAINT PK_EdgeDeviceMapping PRIMARY KEY(BranchId,SourceDeviceId));
IF OBJECT_ID(N'app.EdgePromotionMapping',N'U') IS NULL
 CREATE TABLE app.EdgePromotionMapping(BranchId nvarchar(128) NOT NULL,PromotionCode int NOT NULL,PromotionId nvarchar(128) NOT NULL,PricingVersion nvarchar(128) NOT NULL,SnapshotVersion nvarchar(128) NOT NULL,CONSTRAINT PK_EdgePromotionMapping PRIMARY KEY(BranchId,PromotionCode));
IF OBJECT_ID(N'app.EdgeCardSequence',N'U') IS NULL
 CREATE TABLE app.EdgeCardSequence(SourceInstanceId uniqueidentifier NOT NULL,CardKeyHash varbinary(32) NOT NULL,LastSequence bigint NOT NULL,CONSTRAINT PK_EdgeCardSequence PRIMARY KEY(SourceInstanceId,CardKeyHash));
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'app.EdgeCustomerCache') AND name=N'IX_EdgeCustomerCache_Search')
 CREATE INDEX IX_EdgeCustomerCache_Search ON app.EdgeCustomerCache(BranchId,DisplayName,CustomerId);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'app.EdgeMemberCache') AND name=N'IX_EdgeMemberCache_Household')
 CREATE INDEX IX_EdgeMemberCache_Household ON app.EdgeMemberCache(BranchId,HouseholdId,DisplayName);");

            EnsureOperationState(connection, transaction);
        }

        private static void EnsureOperationState(SqlConnection connection, SqlTransaction transaction)
        {
            Execute(connection, transaction, @"
IF COL_LENGTH(N'app.CardOperationLog',N'OperationState') IS NULL
 ALTER TABLE app.CardOperationLog ADD OperationState varchar(24) NULL;");

            Execute(connection, transaction, @"
UPDATE app.CardOperationLog
SET OperationState=CASE
 WHEN OperationStatus IN (N'started',N'increment_accepted',N'transfer_sent',N'card_written',N'verified') THEN 'unknown'
 WHEN OperationStatus=N'unknown' THEN 'unknown'
 ELSE 'legacy' END
WHERE OperationState IS NULL;");

            Execute(connection, transaction, @"
ALTER TABLE app.CardOperationLog ALTER COLUMN OperationState varchar(24) NOT NULL;
IF NOT EXISTS
(
 SELECT 1
 FROM sys.default_constraints dc
 JOIN sys.columns c ON c.object_id=dc.parent_object_id AND c.column_id=dc.parent_column_id
 WHERE dc.parent_object_id=OBJECT_ID(N'app.CardOperationLog')
   AND c.name=N'OperationState'
)
 ALTER TABLE app.CardOperationLog ADD CONSTRAINT DF_CardOperationLog_OperationState DEFAULT('started') FOR OperationState;");
        }

        private static void Execute(SqlConnection connection, SqlTransaction transaction, string sql)
        {
            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.CommandTimeout = 180;
                command.ExecuteNonQuery();
            }
        }
    }
}
