using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace FuntinoControlCenter.Services
{
    [DataContract]
    internal sealed class EdgeEventDto
    {
        [DataMember(Name = "eventId", Order = 1)] public string EventId { get; set; }
        [DataMember(Name = "sourceInstanceId", Order = 2)] public string SourceInstanceId { get; set; }
        [DataMember(Name = "localTransactionId", Order = 3)] public string LocalTransactionId { get; set; }
        [DataMember(Name = "sourceDatabase", Order = 4)] public string SourceDatabase { get; set; }
        [DataMember(Name = "organizationId", Order = 5)] public string OrganizationId { get; set; }
        [DataMember(Name = "branchId", Order = 6)] public string BranchId { get; set; }
        [DataMember(Name = "cardId", Order = 7)] public string CardId { get; set; }
        [DataMember(Name = "householdId", Order = 8, EmitDefaultValue = false)] public string HouseholdId { get; set; }
        [DataMember(Name = "memberId", Order = 9, EmitDefaultValue = false)] public string MemberId { get; set; }
        [DataMember(Name = "deviceId", Order = 10, EmitDefaultValue = false)] public string DeviceId { get; set; }
        [DataMember(Name = "sourceDeviceId", Order = 11, EmitDefaultValue = false)] public string SourceDeviceId { get; set; }
        [DataMember(Name = "transactionKind", Order = 12)] public string TransactionKind { get; set; }
        [DataMember(Name = "amountRial", Order = 13)] public long AmountRial { get; set; }
        [DataMember(Name = "currencyCode", Order = 14)] public string CurrencyCode { get; set; }
        [DataMember(Name = "grossAmountRial", Order = 15)] public long GrossAmountRial { get; set; }
        [DataMember(Name = "discountAmountRial", Order = 16)] public long DiscountAmountRial { get; set; }
        [DataMember(Name = "netAmountRial", Order = 17)] public long NetAmountRial { get; set; }
        [DataMember(Name = "promotionId", Order = 18, EmitDefaultValue = false)] public string PromotionId { get; set; }
        [DataMember(Name = "pricingVersion", Order = 19)] public string PricingVersion { get; set; }
        [DataMember(Name = "occurredAt", Order = 20)] public string OccurredAt { get; set; }
        [DataMember(Name = "localSequence", Order = 21)] public long LocalSequence { get; set; }
        [DataMember(Name = "balanceAfterRial", Order = 22, EmitDefaultValue = false)] public long? BalanceAfterRial { get; set; }
        [DataMember(Name = "operationResult", Order = 23)] public string OperationResult { get; set; }
    }

    [DataContract]
    internal sealed class EdgeAcknowledgementDto
    {
        [DataMember(Name = "eventId", IsRequired = true)] public string EventId { get; set; }
        [DataMember(Name = "status", IsRequired = true)] public string Status { get; set; }
        [DataMember(Name = "reason", EmitDefaultValue = false)] public string Reason { get; set; }
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
                if (!acknowledged.Add(eventId)) continue;

                validResults.Add(new EdgeAcknowledgementDto
                {
                    EventId = eventId.ToString("D"),
                    Status = state,
                    Reason = result.Reason
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
        [DataMember(Name = "localCardUid", IsRequired = true)] public string LocalCardUid { get; set; }
        [DataMember(Name = "cardId", IsRequired = true)] public string CardId { get; set; }
        [DataMember(Name = "householdId", EmitDefaultValue = false)] public string HouseholdId { get; set; }
        [DataMember(Name = "memberId", EmitDefaultValue = false)] public string MemberId { get; set; }
    }
}
