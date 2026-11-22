using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace FuntinoControlCenter.Services
{
    [DataContract]
    internal sealed class EdgeOperatorPairingStartResponse
    {
        [DataMember(Name = "pairingId", IsRequired = true)] public string PairingId { get; set; }
        [DataMember(Name = "userCode", IsRequired = true)] public string UserCode { get; set; }
        [DataMember(Name = "pollSecret", IsRequired = true)] public string PollSecret { get; set; }
        [DataMember(Name = "expiresAt", IsRequired = true)] public string ExpiresAt { get; set; }
        [DataMember(Name = "pollIntervalSeconds", IsRequired = true)] public int PollIntervalSeconds { get; set; }
    }

    [DataContract]
    internal sealed class EdgeOperatorPairingPollResponse
    {
        [DataMember(Name = "status", IsRequired = true)] public string Status { get; set; }
        [DataMember(Name = "operatorToken", EmitDefaultValue = false)] public EdgeOperatorTokenDto OperatorToken { get; set; }
    }

    [DataContract]
    internal sealed class EdgeOperatorTokenDto
    {
        [DataMember(Name = "token", IsRequired = true)] public string Token { get; set; }
        [DataMember(Name = "tokenId", IsRequired = true)] public string TokenId { get; set; }
        [DataMember(Name = "organizationId", IsRequired = true)] public string OrganizationId { get; set; }
        [DataMember(Name = "branchId", IsRequired = true)] public string BranchId { get; set; }
        [DataMember(Name = "permissionKeys", IsRequired = true)] public List<string> PermissionKeys { get; set; }
        [DataMember(Name = "expiresAt", IsRequired = true)] public string ExpiresAt { get; set; }
        [DataMember(Name = "policyVersion", IsRequired = true)] public int PolicyVersion { get; set; }
        [DataMember(Name = "catalogVersion", IsRequired = true)] public int CatalogVersion { get; set; }
    }

    [DataContract]
    internal sealed class EdgeEventDto
    {
        [DataMember(Name = "eventId", Order = 1, IsRequired = true)] public string EventId { get; set; }
        [DataMember(Name = "sourceInstanceId", Order = 2, IsRequired = true)] public string SourceInstanceId { get; set; }
        [DataMember(Name = "localTransactionId", Order = 3, IsRequired = true)] public string LocalTransactionId { get; set; }
        [DataMember(Name = "sourceDatabase", Order = 4, IsRequired = true)] public string SourceDatabase { get; set; }
        [DataMember(Name = "organizationId", Order = 5, IsRequired = true)] public string OrganizationId { get; set; }
        [DataMember(Name = "branchId", Order = 6, IsRequired = true)] public string BranchId { get; set; }
        [DataMember(Name = "cardId", Order = 7, IsRequired = true)] public string CardId { get; set; }
        [DataMember(Name = "householdId", Order = 8, IsRequired = true)] public string HouseholdId { get; set; }
        [DataMember(Name = "memberId", Order = 9, IsRequired = true)] public string MemberId { get; set; }
        [DataMember(Name = "deviceId", Order = 10, IsRequired = true)] public string DeviceId { get; set; }
        [DataMember(Name = "sourceDeviceId", Order = 11, IsRequired = true)] public string SourceDeviceId { get; set; }
        [DataMember(Name = "attractionId", Order = 12, IsRequired = true)] public string AttractionId { get; set; }
        [DataMember(Name = "transactionKind", Order = 13, IsRequired = true)] public string TransactionKind { get; set; }
        [DataMember(Name = "amountRial", Order = 14, IsRequired = true)] public long AmountRial { get; set; }
        [DataMember(Name = "currencyCode", Order = 15, IsRequired = true)] public string CurrencyCode { get; set; }
        [DataMember(Name = "grossAmountRial", Order = 16, IsRequired = true)] public long GrossAmountRial { get; set; }
        [DataMember(Name = "discountAmountRial", Order = 17, IsRequired = true)] public long DiscountAmountRial { get; set; }
        [DataMember(Name = "netAmountRial", Order = 18, IsRequired = true)] public long NetAmountRial { get; set; }
        [DataMember(Name = "promotionId", Order = 19, IsRequired = true)] public string PromotionId { get; set; }
        [DataMember(Name = "ruleId", Order = 20, IsRequired = true)] public string RuleId { get; set; }
        [DataMember(Name = "pricingVersion", Order = 21, IsRequired = true)] public string PricingVersion { get; set; }
        [DataMember(Name = "occurredAt", Order = 22, IsRequired = true)] public string OccurredAt { get; set; }
        [DataMember(Name = "localSequence", Order = 23, IsRequired = true)] public long LocalSequence { get; set; }
        [DataMember(Name = "balanceAfterRial", Order = 24, IsRequired = true)] public long? BalanceAfterRial { get; set; }
        [DataMember(Name = "operationResult", Order = 25, IsRequired = true)] public string OperationResult { get; set; }
        [DataMember(Name = "snapshotVersion", Order = 26, IsRequired = true)] public string SnapshotVersion { get; set; }
        [DataMember(Name = "reversalOfLocalTransactionId", Order = 27, IsRequired = true)] public string ReversalOfLocalTransactionId { get; set; }
    }

    [DataContract]
    internal sealed class EdgeEventBatchRequest
    {
        [DataMember(Name = "sourceInstanceId", Order = 1, IsRequired = true)] public string SourceInstanceId { get; set; }
        [DataMember(Name = "events", Order = 2, IsRequired = true)] public List<EdgeEventDto> Events { get; set; }
    }

    [DataContract]
    internal sealed class EdgeAcknowledgementDto
    {
        [DataMember(Name = "eventId", IsRequired = true)] public string EventId { get; set; }
        [DataMember(Name = "status", IsRequired = true)] public string Status { get; set; }
        [DataMember(Name = "errorCode", IsRequired = true)] public string ErrorCode { get; set; }
        [DataMember(Name = "retryable", IsRequired = true)] public bool Retryable { get; set; }
    }

    [DataContract]
    internal sealed class EdgeAcknowledgementResponse
    {
        [DataMember(Name = "results", IsRequired = true)] public List<EdgeAcknowledgementDto> Results { get; set; }
    }

    internal sealed class EdgeAcknowledgementBatch
    {
        public IList<EdgeAcknowledgementDto> ValidResults { get; set; }
        public IList<Guid> MissingEventIds { get; set; }
    }

    internal static class EdgeAcknowledgementProcessor
    {
        internal static EdgeAcknowledgementBatch Validate(IList<Guid> expectedEventIds, EdgeAcknowledgementResponse response)
        {
            if (expectedEventIds == null) throw new ArgumentNullException("expectedEventIds");
            if (response == null || response.Results == null) throw new EdgeApiException("invalid-response", true);

            var expected = new HashSet<Guid>(expectedEventIds);
            var acknowledged = new HashSet<Guid>();
            var validResults = new List<EdgeAcknowledgementDto>();
            foreach (EdgeAcknowledgementDto result in response.Results)
            {
                Guid eventId;
                if (result == null || !Guid.TryParse(result.EventId, out eventId) || !expected.Contains(eventId)) continue;

                string state = (result.Status ?? String.Empty).Trim().ToLowerInvariant();
                if (state != "accepted" && state != "duplicate" && state != "rejected") continue;
                if ((state == "accepted" || state == "duplicate") && (result.ErrorCode != null || result.Retryable)) continue;
                if (state == "rejected" && String.IsNullOrWhiteSpace(result.ErrorCode)) continue;
                if (!acknowledged.Add(eventId)) continue;

                validResults.Add(new EdgeAcknowledgementDto
                {
                    EventId = eventId.ToString("D"),
                    Status = state,
                    ErrorCode = result.ErrorCode,
                    Retryable = result.Retryable
                });
            }

            return new EdgeAcknowledgementBatch
            {
                ValidResults = validResults,
                MissingEventIds = expectedEventIds.Distinct().Where(eventId => !acknowledged.Contains(eventId)).ToList()
            };
        }
    }

    [DataContract]
    internal sealed class EdgeBranchSnapshotDto
    {
        [DataMember(Name = "version", IsRequired = true)] public string Version { get; set; }
        [DataMember(Name = "issuedAt", IsRequired = true)] public string IssuedAt { get; set; }
        [DataMember(Name = "expiresAt", IsRequired = true)] public string ExpiresAt { get; set; }
        [DataMember(Name = "branchId", IsRequired = true)] public string BranchId { get; set; }
        [DataMember(Name = "cursor", EmitDefaultValue = false)] public string Cursor { get; set; }
        [DataMember(Name = "devices", IsRequired = true)] public List<EdgeDeviceMappingDto> Devices { get; set; }
        [DataMember(Name = "pricingRules", IsRequired = true)] public List<EdgePricingRuleDto> PricingRules { get; set; }
        [DataMember(Name = "customers", IsRequired = true)] public List<EdgeCustomerDto> Customers { get; set; }
        [DataMember(Name = "households", IsRequired = true)] public List<EdgeHouseholdDto> Households { get; set; }
        [DataMember(Name = "members", IsRequired = true)] public List<EdgeMemberDto> Members { get; set; }
        [DataMember(Name = "cards", IsRequired = true)] public List<EdgeCardMappingDto> Cards { get; set; }
    }

    [DataContract]
    internal sealed class EdgeDeviceMappingDto
    {
        [DataMember(Name = "sourceDeviceId", IsRequired = true)] public string SourceDeviceId { get; set; }
        [DataMember(Name = "deviceId", IsRequired = true)] public string DeviceId { get; set; }
        [DataMember(Name = "attractionId", EmitDefaultValue = false)] public string AttractionId { get; set; }
    }

    [DataContract]
    internal sealed class EdgePricingRuleDto
    {
        [DataMember(Name = "pricingVersion", IsRequired = true)] public string PricingVersion { get; set; }
        [DataMember(Name = "promotionCode", EmitDefaultValue = false)] public int? PromotionCode { get; set; }
        [DataMember(Name = "promotionId", EmitDefaultValue = false)] public string PromotionId { get; set; }
    }

    [DataContract]
    internal sealed class EdgeCustomerDto
    {
        [DataMember(Name = "customerId", IsRequired = true)] public string CustomerId { get; set; }
        [DataMember(Name = "displayName", IsRequired = true)] public string DisplayName { get; set; }
        [DataMember(Name = "householdId", EmitDefaultValue = false)] public string HouseholdId { get; set; }
    }

    [DataContract]
    internal sealed class EdgeHouseholdDto
    {
        [DataMember(Name = "householdId", IsRequired = true)] public string HouseholdId { get; set; }
        [DataMember(Name = "displayName", IsRequired = true)] public string DisplayName { get; set; }
        [DataMember(Name = "guardianCustomerId", EmitDefaultValue = false)] public string GuardianCustomerId { get; set; }
    }

    [DataContract]
    internal sealed class EdgeMemberDto
    {
        [DataMember(Name = "memberId", IsRequired = true)] public string MemberId { get; set; }
        [DataMember(Name = "householdId", IsRequired = true)] public string HouseholdId { get; set; }
        [DataMember(Name = "customerId", EmitDefaultValue = false)] public string CustomerId { get; set; }
        [DataMember(Name = "displayName", IsRequired = true)] public string DisplayName { get; set; }
    }

    [DataContract]
    internal sealed class EdgeCardMappingDto
    {
        [DataMember(Name = "cardUidHash", IsRequired = true)] public string CardUidHash { get; set; }
        [DataMember(Name = "cardId", IsRequired = true)] public string CardId { get; set; }
        [DataMember(Name = "householdId", EmitDefaultValue = false)] public string HouseholdId { get; set; }
        [DataMember(Name = "memberId", EmitDefaultValue = false)] public string MemberId { get; set; }
    }

    [DataContract]
    internal sealed class EdgeCustomerSearchPage
    {
        [DataMember(Name = "items", IsRequired = true)] public List<EdgeCustomerSearchItem> Items { get; set; }
        [DataMember(Name = "nextCursor", IsRequired = true)] public string NextCursor { get; set; }
    }

    [DataContract]
    internal sealed class EdgeCustomerSearchItem
    {
        [DataMember(Name = "customerId", IsRequired = true)] public string CustomerId { get; set; }
        [DataMember(Name = "displayName", IsRequired = true)] public string DisplayName { get; set; }
        [DataMember(Name = "maskedPhone", IsRequired = true)] public string MaskedPhone { get; set; }
        [DataMember(Name = "householdId", IsRequired = true)] public string HouseholdId { get; set; }
        [DataMember(Name = "memberId", IsRequired = true)] public string MemberId { get; set; }
        [DataMember(Name = "recordVersion", IsRequired = true)] public int RecordVersion { get; set; }
    }

    [DataContract]
    internal sealed class EdgeCustomerProfile
    {
        [DataMember(Name = "customerId", IsRequired = true)] public string CustomerId { get; set; }
        [DataMember(Name = "firstName", IsRequired = true)] public string FirstName { get; set; }
        [DataMember(Name = "lastName", IsRequired = true)] public string LastName { get; set; }
        [DataMember(Name = "birthDate", IsRequired = true)] public string BirthDate { get; set; }
        [DataMember(Name = "gender", IsRequired = true)] public string Gender { get; set; }
        [DataMember(Name = "city", IsRequired = true)] public string City { get; set; }
        [DataMember(Name = "avatarPreset", IsRequired = true)] public string AvatarPreset { get; set; }
        [DataMember(Name = "hasUploadedAvatar", IsRequired = true)] public bool HasUploadedAvatar { get; set; }
        [DataMember(Name = "profileCompleted", IsRequired = true)] public bool ProfileCompleted { get; set; }
        [DataMember(Name = "recordVersion", IsRequired = true)] public int RecordVersion { get; set; }
        [DataMember(Name = "maskedPhone", IsRequired = true)] public string MaskedPhone { get; set; }
    }

    [DataContract]
    internal sealed class EdgeHouseholdRead
    {
        [DataMember(Name = "id", IsRequired = true)] public string Id { get; set; }
        [DataMember(Name = "name", IsRequired = true)] public string Name { get; set; }
        [DataMember(Name = "status", IsRequired = true)] public string Status { get; set; }
        [DataMember(Name = "recordVersion", IsRequired = true)] public int RecordVersion { get; set; }
        [DataMember(Name = "members", IsRequired = true)] public List<EdgeHouseholdReadMember> Members { get; set; }
    }

    [DataContract]
    internal sealed class EdgeHouseholdReadMember
    {
        [DataMember(Name = "id", IsRequired = true)] public string Id { get; set; }
        [DataMember(Name = "displayName", IsRequired = true)] public string DisplayName { get; set; }
        [DataMember(Name = "relationship", IsRequired = true)] public string Relationship { get; set; }
        [DataMember(Name = "birthDate", IsRequired = true)] public string BirthDate { get; set; }
        [DataMember(Name = "isDependent", IsRequired = true)] public bool IsDependent { get; set; }
        [DataMember(Name = "status", IsRequired = true)] public string Status { get; set; }
        [DataMember(Name = "recordVersion", IsRequired = true)] public int RecordVersion { get; set; }
    }

    [DataContract]
    internal sealed class EdgeProfileMutationResult
    {
        [DataMember(Name = "customerId", IsRequired = true)] public string CustomerId { get; set; }
        [DataMember(Name = "recordVersion", IsRequired = true)] public int RecordVersion { get; set; }
        [DataMember(Name = "maskedPhone", IsRequired = true)] public string MaskedPhone { get; set; }
    }

    [DataContract]
    internal sealed class EdgeHouseholdMutationResult
    {
        [DataMember(Name = "householdId", IsRequired = true)] public string HouseholdId { get; set; }
        [DataMember(Name = "recordVersion", IsRequired = true)] public int RecordVersion { get; set; }
    }

    [DataContract]
    internal sealed class EdgeHouseholdMemberMutationResult
    {
        [DataMember(Name = "memberId", IsRequired = true)] public string MemberId { get; set; }
        [DataMember(Name = "householdId", IsRequired = true)] public string HouseholdId { get; set; }
        [DataMember(Name = "recordVersion", IsRequired = true)] public int RecordVersion { get; set; }
    }

    [DataContract]
    internal sealed class EdgeTemporaryCustomerResult
    {
        [DataMember(Name = "provisionalId", IsRequired = true)] public string ProvisionalId { get; set; }
        [DataMember(Name = "sourceInstanceId", IsRequired = true)] public string SourceInstanceId { get; set; }
        [DataMember(Name = "branchId", IsRequired = true)] public string BranchId { get; set; }
        [DataMember(Name = "status", IsRequired = true)] public string Status { get; set; }
        [DataMember(Name = "createdAt", IsRequired = true)] public string CreatedAt { get; set; }
        [DataMember(Name = "customerConfirmed", IsRequired = true)] public bool CustomerConfirmed { get; set; }
    }

    [DataContract]
    internal sealed class EdgeCardAssignmentResult
    {
        [DataMember(Name = "assignmentId", IsRequired = true)] public string AssignmentId { get; set; }
        [DataMember(Name = "cardId", IsRequired = true)] public string CardId { get; set; }
        [DataMember(Name = "householdId", IsRequired = true)] public string HouseholdId { get; set; }
        [DataMember(Name = "memberId", IsRequired = true)] public string MemberId { get; set; }
        [DataMember(Name = "status", IsRequired = true)] public string Status { get; set; }
        [DataMember(Name = "verificationStatus", IsRequired = true)] public string VerificationStatus { get; set; }
        [DataMember(Name = "maskedIdentifier", IsRequired = true)] public string MaskedIdentifier { get; set; }
    }

    [DataContract]
    internal sealed class EdgeCardBlockResult
    {
        [DataMember(Name = "cardId", IsRequired = true)] public string CardId { get; set; }
        [DataMember(Name = "status", IsRequired = true)] public string Status { get; set; }
        [DataMember(Name = "recordVersion", IsRequired = true)] public int RecordVersion { get; set; }
        [DataMember(Name = "maskedIdentifier", IsRequired = true)] public string MaskedIdentifier { get; set; }
    }

    [DataContract]
    internal sealed class EdgeCardReplacementResult
    {
        [DataMember(Name = "requestId", IsRequired = true)] public string RequestId { get; set; }
        [DataMember(Name = "cardId", IsRequired = true)] public string CardId { get; set; }
        [DataMember(Name = "status", IsRequired = true)] public string Status { get; set; }
        [DataMember(Name = "duplicate", IsRequired = true)] public bool Duplicate { get; set; }
    }
}
