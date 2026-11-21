using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Services;

namespace FuntinoControlCenter.Tests
{
    internal static class Program
    {
        private static int checks;

        private static int Main()
        {
            try
            {
                CheckTomanToRial();
                CheckRawBodyHmac();
                CheckSnapshotReferences();
                CheckSnapshotBranchAndTime();
                CheckEventSerialization();
                CheckAcknowledgementValidation();
                Console.WriteLine("PASS: " + checks.ToString(CultureInfo.InvariantCulture) + " offline edge contract checks");
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("FAIL: " + exception.GetType().Name + " - " + exception.Message);
                return 1;
            }
        }

        private static void CheckTomanToRial()
        {
            Equal(123450L, EdgeAmountConverter.ToRial(12345L), "integer toman-to-rial conversion");
            Throws<OverflowException>(delegate { EdgeAmountConverter.ToRial(Int64.MaxValue); }, "overflow is rejected");
        }

        private static void CheckRawBodyHmac()
        {
            var key = new byte[32];
            for (int index = 0; index < key.Length; index++) key[index] = 0x0b;
            string actual = EdgeApiClient.ComputeSignature(key, Encoding.ASCII.GetBytes("Hi There"));
            Equal("GYpgfrRL+8aZA6Dxzyu9xboKo/PZrjwcejsWlqC2jPc=", actual, "HMAC-SHA256 signs the raw request body");
        }

        private static void CheckSnapshotReferences()
        {
            EdgeBranchSnapshotDto snapshot = ValidSnapshot();
            DateTime issuedAt, expiresAt;
            EdgeIntegrationRepository.ValidateSnapshot(snapshot, "branch-1", out issuedAt, out expiresAt);
            checks++;

            snapshot.Members[0].HouseholdId = "missing-household";
            Throws<EdgeApiException>(delegate
            {
                EdgeIntegrationRepository.ValidateSnapshot(snapshot, "branch-1", out issuedAt, out expiresAt);
            }, "snapshot rejects dangling family membership");
        }

        private static void CheckSnapshotBranchAndTime()
        {
            EdgeBranchSnapshotDto snapshot = ValidSnapshot();
            DateTime issuedAt, expiresAt;
            Throws<EdgeApiException>(delegate
            {
                EdgeIntegrationRepository.ValidateSnapshot(snapshot, "another-branch", out issuedAt, out expiresAt);
            }, "snapshot is branch scoped");

            snapshot = ValidSnapshot();
            snapshot.ExpiresAt = DateTime.UtcNow.AddMinutes(-1).ToString("o", CultureInfo.InvariantCulture);
            Throws<EdgeApiException>(delegate
            {
                EdgeIntegrationRepository.ValidateSnapshot(snapshot, "branch-1", out issuedAt, out expiresAt);
            }, "expired snapshot is rejected");
        }

        private static void CheckEventSerialization()
        {
            var dto = new EdgeEventDto
            {
                EventId = Guid.NewGuid().ToString("D"),
                AmountRial = EdgeAmountConverter.ToRial(75),
                CurrencyCode = "IRR",
                GrossAmountRial = 750,
                DiscountAmountRial = 0,
                NetAmountRial = 750,
                OperationResult = "succeeded"
            };
            var serializer = new DataContractJsonSerializer(typeof(EdgeEventDto));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, dto);
                string json = Encoding.UTF8.GetString(stream.ToArray());
                if (!json.Contains("\"amountRial\":750") || !json.Contains("\"currencyCode\":\"IRR\"") || !json.Contains("\"netAmountRial\":750"))
                    throw new InvalidOperationException("Event JSON did not preserve integer IRR amounts.");
                checks++;
            }
        }

        private static void CheckAcknowledgementValidation()
        {
            Guid first = Guid.NewGuid();
            Guid second = Guid.NewGuid();
            EdgeAcknowledgementBatch batch = EdgeAcknowledgementProcessor.Validate(
                new[] { first, second },
                new EdgeAcknowledgementResponse
                {
                    Results = new List<EdgeAcknowledgementDto>
                    {
                        new EdgeAcknowledgementDto { EventId = first.ToString("D"), Status = "unknown" },
                        new EdgeAcknowledgementDto { EventId = first.ToString("D"), Status = "ACCEPTED" },
                        new EdgeAcknowledgementDto { EventId = first.ToString("D"), Status = "duplicate" },
                        new EdgeAcknowledgementDto { EventId = second.ToString("D"), Status = "rejected" }
                    }
                });
            Equal(2, batch.ValidResults.Count, "invalid status does not consume the event acknowledgement");
            Equal(0, batch.MissingEventIds.Count, "accepted and rejected events both have final acknowledgements");
            Equal("accepted", batch.ValidResults[0].Status, "acknowledgement status is normalized");
            Equal("rejected", batch.ValidResults[1].Status, "rejection status is retained");

            batch = EdgeAcknowledgementProcessor.Validate(
                new[] { first, second },
                new EdgeAcknowledgementResponse
                {
                    Results = new List<EdgeAcknowledgementDto>
                    {
                        new EdgeAcknowledgementDto { EventId = first.ToString("D"), Status = "pending" },
                        new EdgeAcknowledgementDto { EventId = Guid.NewGuid().ToString("D"), Status = "accepted" }
                    }
                });
            Equal(0, batch.ValidResults.Count, "unknown and out-of-batch acknowledgement values are ignored");
            Equal(2, batch.MissingEventIds.Count, "events without a final acknowledgement remain retryable");

            batch = EdgeAcknowledgementProcessor.Validate(
                new[] { first },
                new EdgeAcknowledgementResponse
                {
                    Results = new List<EdgeAcknowledgementDto>
                    {
                        new EdgeAcknowledgementDto { EventId = first.ToString("D"), Status = "duplicate" }
                    }
                });
            Equal(1, batch.ValidResults.Count, "server duplicate is a final acknowledgement for the existing event");
            Equal("duplicate", batch.ValidResults[0].Status, "server duplicate status is retained");
            Equal(0, batch.MissingEventIds.Count, "server duplicate does not leave the event pending");
        }

        private static EdgeBranchSnapshotDto ValidSnapshot()
        {
            DateTime now = DateTime.UtcNow;
            return new EdgeBranchSnapshotDto
            {
                Version = "snapshot-1",
                IssuedAt = now.ToString("o", CultureInfo.InvariantCulture),
                ExpiresAt = now.AddMinutes(5).ToString("o", CultureInfo.InvariantCulture),
                BranchId = "branch-1",
                Cursor = "cursor-1",
                Customers = new List<EdgeCustomerDto>
                {
                    new EdgeCustomerDto { CustomerId = "customer-1", DisplayName = "Customer", HouseholdId = "household-1" }
                },
                Households = new List<EdgeHouseholdDto>
                {
                    new EdgeHouseholdDto { HouseholdId = "household-1", DisplayName = "Household", GuardianCustomerId = "customer-1" }
                },
                Members = new List<EdgeMemberDto>
                {
                    new EdgeMemberDto { MemberId = "member-1", HouseholdId = "household-1", CustomerId = "customer-1", DisplayName = "Member" }
                },
                Cards = new List<EdgeCardMappingDto>
                {
                    new EdgeCardMappingDto { LocalCardUid = "01-02-03-04", CardId = "card-1", HouseholdId = "household-1", MemberId = "member-1" }
                },
                Devices = new List<EdgeDeviceMappingDto>
                {
                    new EdgeDeviceMappingDto { SourceDeviceId = "local-device-1", DeviceId = "device-1", AttractionId = "attraction-1" }
                },
                PricingRules = new List<EdgePricingRuleDto>
                {
                    new EdgePricingRuleDto { PricingVersion = "price-1", PromotionCode = 5, PromotionId = "promotion-1" }
                }
            };
        }

        private static void Equal<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new InvalidOperationException(message + ": expected " + expected + ", got " + actual + ".");
            checks++;
        }

        private static void Throws<TException>(Action action, string message) where TException : Exception
        {
            try { action(); }
            catch (TException) { checks++; return; }
            throw new InvalidOperationException(message + ": expected " + typeof(TException).Name + ".");
        }
    }
}
