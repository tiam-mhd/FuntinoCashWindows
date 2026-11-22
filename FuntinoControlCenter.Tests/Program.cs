using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
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
