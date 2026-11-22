using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Services;

namespace FuntinoControlCenter.Tests
{
    internal static class Program
    {
        private const string FixturePath = "Fixtures\\windows-edge-contract.v1.fixtures.json";
        private static int checks;

        private static int Main()
        {
            try
            {
                CheckOfficialEventEnvelopeAndSignature();
                CheckRequiredNullableEventFields();
                CheckOfficialAcknowledgements();
                CheckSnapshotSignatureFixture();
                CheckCardUidHashVector();
                CheckDatabaseCatalogGuard();
                CheckTomanToRial();
                CheckEdgeOperatorPairingFixtureAndHeaders();
                CheckEdgeOperatorSessionScopeAndExpiry();
                Console.WriteLine("PASS: " + checks.ToString(CultureInfo.InvariantCulture) + " Windows edge contract checks");
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("FAIL: " + exception.GetType().Name + " - " + exception.Message);
                return 1;
            }
        }

        private static void CheckOfficialEventEnvelopeAndSignature()
        {
            Dictionary<string, object> fixture = ReadFixture();
            var json = new JavaScriptSerializer();
            EdgeEventDto item = Deserialize<EdgeEventDto>(json.Serialize(fixture["event"]));
            byte[] body = Serialize(new EdgeEventBatchRequest
            {
                SourceInstanceId = item.SourceInstanceId,
                Events = new List<EdgeEventDto> { item }
            });
            string key = "windows-edge-contract-fixture-only-key";
            string actual = EdgeApiClient.ComputeSignature(Encoding.UTF8.GetBytes(key), body);
            Equal((string)fixture["requestSignature"], actual, "official raw request signature");
            string requestJson = Encoding.UTF8.GetString(body);
            if (!requestJson.Contains("\"sourceInstanceId\":") || !requestJson.Contains("\"events\":["))
                throw new InvalidOperationException("The request body is not the official event envelope.");
            checks++;
        }

        private static void CheckRequiredNullableEventFields()
        {
            Dictionary<string, object> fixture = ReadFixture();
            var json = new JavaScriptSerializer();
            EdgeEventDto item = Deserialize<EdgeEventDto>(json.Serialize(fixture["event"]));
            string serialized = Encoding.UTF8.GetString(Serialize(item));
            foreach (string property in new[]
            {
                "memberId", "deviceId", "sourceDeviceId", "attractionId", "promotionId", "ruleId",
                "pricingVersion", "balanceAfterRial", "snapshotVersion", "reversalOfLocalTransactionId"
            })
            {
                if (!serialized.Contains("\"" + property + "\":"))
                    throw new InvalidOperationException("Required event field is missing: " + property);
                checks++;
            }
            if (!serialized.Contains("\"memberId\":null") || !serialized.Contains("\"attractionId\":null"))
                throw new InvalidOperationException("Required nullable fields must serialize as null.");
            checks++;

            Throws<SerializationException>(
                delegate { Deserialize<EdgeEventDto>(json.Serialize(fixture["invalidEventWithoutRequiredNullableKey"])); },
                "official fixture missing required nullable field is rejected");
        }

        private static void CheckOfficialAcknowledgements()
        {
            Dictionary<string, object> fixture = ReadFixture();
            var json = new JavaScriptSerializer();
            EdgeAcknowledgementResponse valid = Deserialize<EdgeAcknowledgementResponse>(json.Serialize(fixture["validResponse"]));
            var expected = new[] { Guid.Parse("11111111-1111-4111-8111-111111111111") };
            EdgeAcknowledgementBatch batch = EdgeAcknowledgementProcessor.Validate(expected, valid);
            Equal(1, batch.ValidResults.Count, "fixture accepted event is applied");
            Equal("accepted", batch.ValidResults[0].Status, "fixture accepted status is retained");
            Equal(0, batch.MissingEventIds.Count, "accepted event is final");

            EdgeAcknowledgementResponse invalid = Deserialize<EdgeAcknowledgementResponse>(json.Serialize(fixture["invalidUnknownAcceptedResult"]));
            batch = EdgeAcknowledgementProcessor.Validate(expected, invalid);
            Equal(0, batch.ValidResults.Count, "accepted response with an error code is invalid");
            Equal(1, batch.MissingEventIds.Count, "invalid or incomplete response remains retryable");

            var incomplete = new Dictionary<string, object>(StringComparer.Ordinal);
            var invalidRoot = (Dictionary<string, object>)fixture["invalidUnknownAcceptedResult"];
            var results = (object[])invalidRoot["results"];
            var first = new Dictionary<string, object>((Dictionary<string, object>)results[0], StringComparer.Ordinal);
            first.Remove("retryable");
            incomplete["results"] = new object[] { first };
            Throws<SerializationException>(
                delegate { Deserialize<EdgeAcknowledgementResponse>(json.Serialize(incomplete)); },
                "acknowledgement without retryable field is rejected");
        }

        private static void CheckSnapshotSignatureFixture()
        {
            Dictionary<string, object> fixture = ReadFixture();
            var json = new JavaScriptSerializer();
            string signedMaterial = json.Serialize(fixture["snapshotUnsigned"]);
            string actual = EdgeCanonicalJson.ComputeCanonicalSignature(
                Encoding.UTF8.GetBytes("windows-edge-contract-fixture-only-key"),
                signedMaterial,
                "signature");
            Equal((string)fixture["snapshotSignature"], actual, "official canonical snapshot signature");
        }

        private static void CheckCardUidHashVector()
        {
            Dictionary<string, object> fixture = ReadFixture();
            var json = new JavaScriptSerializer();
            var vector = (Dictionary<string, object>)fixture["cardUidHashVector"];
            byte[] uid = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            string hash = ToLowerHex(EdgeIntegrationRepository.GetCardKeyHash(uid));
            Equal((string)vector["hash"], hash, "official canonical card UID hash");
        }

        private static void CheckDatabaseCatalogGuard()
        {
            DatabaseManager.ValidateOperationalCatalogs();
            Equal("FuntinoParkOperations", DatabaseManager.MainCatalog, "main database uses the approved operational catalog");
            Equal("FuntinoDeviceOperations", DatabaseManager.DeviceCatalog, "device database uses the approved operational catalog");
        }

        private static void CheckTomanToRial()
        {
            Equal(123450L, EdgeAmountConverter.ToRial(12345L), "integer toman-to-rial conversion");
            Throws<OverflowException>(delegate { EdgeAmountConverter.ToRial(Int64.MaxValue); }, "overflow is rejected");
        }

        private static void CheckEdgeOperatorPairingFixtureAndHeaders()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fixtures\\edge-operator-pairing.v1.fixtures.json");
            if (!File.Exists(path)) throw new FileNotFoundException("The Web pairing fixture was not copied to the test output.", path);
            var fixture = new JavaScriptSerializer().DeserializeObject(File.ReadAllText(path, Encoding.UTF8)) as Dictionary<string, object>;
            if (fixture == null || !Convert.ToString(fixture["fixturePurpose"], CultureInfo.InvariantCulture).Contains("synthetic"))
                throw new InvalidOperationException("Pairing test data is not labeled synthetic.");
            EdgeOperatorPairingStartResponse started = Deserialize<EdgeOperatorPairingStartResponse>(new JavaScriptSerializer().Serialize(fixture["startResponse"]));
            EdgeOperatorPairingPollResponse pending = Deserialize<EdgeOperatorPairingPollResponse>(new JavaScriptSerializer().Serialize(fixture["pendingResponse"]));
            EdgeOperatorPairingPollResponse authorized = Deserialize<EdgeOperatorPairingPollResponse>(new JavaScriptSerializer().Serialize(fixture["authorizedResponse"]));
            Equal("ABCD2-EFGH3", started.UserCode, "pairing user code fixture");
            Equal(3, started.PollIntervalSeconds, "pairing polling interval fixture");
            Equal("pending", pending.Status, "pairing pending state fixture");
            Equal("authorized", authorized.Status, "pairing authorized state fixture");
            Equal("customer.lookup.limited", authorized.OperatorToken.PermissionKeys[0], "pairing permission fixture");

            byte[] emptyObjectBody = EdgeApiClient.SerializeOperatorPairingStartBody();
            Equal("{}", Encoding.UTF8.GetString(emptyObjectBody), "pairing start exact raw body");
            byte[] pollBody = EdgeApiClient.SerializeOperatorPairingBody(started.PairingId, started.PollSecret);
            var pollPayload = new JavaScriptSerializer().DeserializeObject(Encoding.UTF8.GetString(pollBody)) as Dictionary<string, object>;
            Equal(2, pollPayload.Count, "pair poll contains exactly its two contract fields");
            Equal(started.PairingId, Convert.ToString(pollPayload["pairingId"], CultureInfo.InvariantCulture), "pair poll id");
            Equal(started.PollSecret, Convert.ToString(pollPayload["pollSecret"], CultureInfo.InvariantCulture), "pair poll secret body");

            byte[] secret = Encoding.UTF8.GetBytes("fixture-only-edge-hmac-key-not-for-use");
            string installationId = "33333333-3333-4333-8333-333333333333";
            var request = (HttpWebRequest)WebRequest.Create("https://edge-fixture.invalid/internal/v2/edge/customers");
            EdgeApiClient.ApplyEdgeRequestHeaders(request, installationId, "fixture-key", secret, emptyObjectBody,
                "fixture-operator-bearer-not-a-credential", null);
            Equal(installationId, request.Headers["x-edge-installation-id"], "pairing request installation header");
            Equal("fixture-key", request.Headers["x-edge-key-id"], "pairing request key header");
            Equal(EdgeApiClient.ComputeSignature(secret, emptyObjectBody), request.Headers["x-edge-signature"], "pairing HMAC covers exact raw body");
            Equal("Bearer fixture-operator-bearer-not-a-credential", request.Headers[HttpRequestHeader.Authorization], "customer read sends operator bearer");
            checks++;
        }

        private static void CheckEdgeOperatorSessionScopeAndExpiry()
        {
            const string organizationId = "11111111-1111-4111-8111-111111111111";
            const string branchId = "22222222-2222-4222-8222-222222222222";
            var token = new EdgeOperatorTokenDto
            {
                Token = "fixture-operator-token-not-a-credential-0123456789",
                TokenId = "55555555-5555-4555-8555-555555555555",
                OrganizationId = organizationId,
                BranchId = branchId,
                PermissionKeys = new List<string> { "customer.lookup.limited", "customer.family.read" },
                ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(2).ToString("O", CultureInfo.InvariantCulture),
                PolicyVersion = 1,
                CatalogVersion = 1
            };
            EdgeOperatorSession.Store(token, organizationId, branchId);
            string stored;
            Equal(true, EdgeOperatorSession.TryGetToken(branchId, "customer.lookup.limited", out stored), "operator session lookup permission");
            Equal(token.Token, stored, "operator token remains process-local and readable before expiry");
            Equal(false, EdgeOperatorSession.TryGetToken("77777777-7777-4777-8777-777777777777", "customer.lookup.limited", out stored), "operator token branch isolation");
            Equal(false, EdgeOperatorSession.TryGetToken(branchId, "card.assign", out stored), "operator token permission restriction");
            Throws<SecurityException>(delegate { EdgeOperatorSession.Store(token, organizationId, "77777777-7777-4777-8777-777777777777"); },
                "operator token scope mismatch is rejected");
            Thread.Sleep(2200);
            Equal(false, EdgeOperatorSession.TryGetToken(branchId, "customer.lookup.limited", out stored), "operator token is cleared at expiration");
            EdgeOperatorSession.Clear();
        }

        private static Dictionary<string, object> ReadFixture()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FixturePath);
            if (!File.Exists(path)) throw new FileNotFoundException("The official Windows edge contract fixture was not copied to the test output.", path);
            string content = File.ReadAllText(path, Encoding.UTF8);
            return new JavaScriptSerializer { MaxJsonLength = 8 * 1024 * 1024, RecursionLimit = 128 }
                .DeserializeObject(content) as Dictionary<string, object>;
        }

        private static T Deserialize<T>(string json)
        {
            var serializer = new DataContractJsonSerializer(typeof(T),
                new DataContractJsonSerializerSettings { MaxItemsInObjectGraph = 250000 });
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json), false))
                return (T)serializer.ReadObject(stream);
        }

        private static byte[] Serialize<T>(T value)
        {
            var serializer = new DataContractJsonSerializer(typeof(T),
                new DataContractJsonSerializerSettings { MaxItemsInObjectGraph = 250000 });
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, value);
                return stream.ToArray();
            }
        }

        private static string ToLowerHex(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (byte value in bytes) builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
            return builder.ToString();
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
