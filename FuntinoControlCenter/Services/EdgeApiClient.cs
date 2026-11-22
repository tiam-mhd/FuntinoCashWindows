using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Security;

namespace FuntinoControlCenter.Services
{
    internal sealed class EdgeApiException : Exception
    {
        public string SafeCode { get; private set; }
        public bool Retryable { get; private set; }

        public EdgeApiException(string safeCode, bool retryable)
            : base("ارتباط با سرور همگام‌سازی کامل نشد.")
        {
            SafeCode = safeCode;
            Retryable = retryable;
        }
    }

    internal sealed class EdgeApiClient : IDisposable
    {
        private const int MaximumResponseBytes = 8 * 1024 * 1024;
        private const int MaximumSnapshotPages = 1000;
        private const int MaximumSnapshotItems = 100000;
        private readonly AppSettings settings;
        private readonly byte[] secret;

        public EdgeApiClient(AppSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException("settings");
            secret = settings.GetEdgeSecret();
            try { ValidateConfiguration(); }
            catch
            {
                if (secret != null) Array.Clear(secret, 0, secret.Length);
                throw;
            }
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        public void Dispose()
        {
            if (secret != null) Array.Clear(secret, 0, secret.Length);
        }

        public EdgeAcknowledgementResponse SendEvents(IList<EdgeEventDto> events)
        {
            if (events == null || events.Count == 0) throw new ArgumentException("رویدادی برای ارسال وجود ندارد.", "events");
            if (events.Count > 100) throw new ArgumentOutOfRangeException("events");
            string installationId = settings.EdgeSourceInstanceId.Trim();
            var batch = new EdgeEventBatchRequest { SourceInstanceId = installationId, Events = new List<EdgeEventDto>(events) };
            foreach (EdgeEventDto item in batch.Events)
            {
                if (item == null || !String.Equals(item.SourceInstanceId, installationId, StringComparison.Ordinal) ||
                    !String.Equals(item.BranchId, settings.EdgeBranchId.Trim(), StringComparison.Ordinal) ||
                    !String.Equals(item.SourceDatabase, DatabaseManager.MainCatalog, StringComparison.Ordinal) ||
                    !IsGuid(item.EventId) || !IsGuid(item.LocalTransactionId) || !IsGuid(item.OrganizationId) ||
                    !IsGuid(item.BranchId) || !IsGuid(item.CardId) || !IsGuid(item.HouseholdId) ||
                    !IsOptionalGuid(item.MemberId) || !IsOptionalGuid(item.DeviceId) || !IsOptionalGuid(item.SourceDeviceId) ||
                    !IsOptionalGuid(item.AttractionId) || !IsOptionalGuid(item.ReversalOfLocalTransactionId) ||
                    !IsSupportedTransactionKind(item.TransactionKind) || !IsSupportedOperationResult(item.OperationResult) ||
                    !String.Equals(item.CurrencyCode, "IRR", StringComparison.Ordinal) || item.AmountRial < 0 ||
                    item.GrossAmountRial < 0 || item.DiscountAmountRial < 0 || item.NetAmountRial < 0 || item.LocalSequence <= 0)
                    throw new EdgeApiException("invalid-event-scope", false);
            }
            byte[] body = Serialize(batch);
            byte[] response = Send("POST", BuildUri("/internal/v2/edge/events"), body);
            var result = Deserialize<EdgeAcknowledgementResponse>(response);
            if (result == null || result.Results == null) throw new EdgeApiException("invalid-response", true);
            return result;
        }

        public EdgeBranchSnapshotDto GetSnapshot(string cursor)
        {
            var allItems = new List<object>();
            var seenCursors = new HashSet<string>(StringComparer.Ordinal);
            Dictionary<string, object> firstPage = null;
            string requestCursor = null;
            string snapshotId = null;
            string snapshotVersion = null;
            string documentHash = null;
            string branchId = settings.EdgeBranchId.Trim();
            string generatedAt = null;
            string validFrom = null;
            string validUntil = null;
            string signingKeyId = null;
            string previousPageMaterial = null;
            int previousCursorOffset = -1;

            // A complete cache replacement always starts at the first page; a stored cursor is not a snapshot.
            for (int pageNumber = 0; pageNumber < MaximumSnapshotPages; pageNumber++)
            {
                string query = "?branchId=" + Uri.EscapeDataString(branchId);
                if (!String.IsNullOrEmpty(requestCursor)) query += "&cursor=" + Uri.EscapeDataString(requestCursor);
                byte[] responseBytes = Send("GET", BuildUri("/internal/v2/edge/snapshot" + query), new byte[0]);
                string responseJson = Encoding.UTF8.GetString(responseBytes);
                Dictionary<string, object> page = DeserializeObject(responseJson);
                string pageSignature = RequireString(page, "signature", 80);
                string pageKeyId = RequireString(page, "signingKeyId", 128);
                if (!String.Equals(pageKeyId, settings.EdgeCredentialId.Trim(), StringComparison.Ordinal) ||
                    !IsSignature(pageSignature) ||
                    !FixedTimeEquals(pageSignature, EdgeCanonicalJson.ComputeCanonicalSignature(secret, responseJson, "signature")))
                    throw new EdgeApiException("invalid-snapshot-signature", false);

                string currentSnapshotId = RequireString(page, "snapshotId", 128);
                string currentVersion = RequireString(page, "snapshotVersion", 128);
                string currentBranch = RequireString(page, "branchId", 128);
                string currentGeneratedAt = RequireString(page, "generatedAt", 64);
                string currentValidFrom = RequireString(page, "validFrom", 64);
                string currentValidUntil = RequireString(page, "validUntil", 64);
                string currentDocumentHash = RequireString(page, "documentHash", 64);
                if (!Guid.TryParse(currentSnapshotId, out _) || !IsLowerHex(currentDocumentHash, 64) ||
                    !String.Equals(currentBranch, branchId, StringComparison.Ordinal) ||
                    !String.Equals(currentBranch, settings.EdgeBranchId.Trim(), StringComparison.Ordinal))
                    throw new EdgeApiException("invalid-snapshot-scope", false);

                DateTimeOffset generatedTime, validFromTime, validUntilTime;
                if (!DateTimeOffset.TryParse(currentGeneratedAt, System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out generatedTime) ||
                    !DateTimeOffset.TryParse(currentValidFrom, System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out validFromTime) ||
                    !DateTimeOffset.TryParse(currentValidUntil, System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out validUntilTime) ||
                    validUntilTime <= DateTimeOffset.UtcNow || validUntilTime <= validFromTime ||
                    validFromTime > DateTimeOffset.UtcNow.AddMinutes(10) || generatedTime > DateTimeOffset.UtcNow.AddMinutes(10))
                    throw new EdgeApiException("invalid-snapshot-time", false);

                object pricing = RequireObject(page, "pricing");
                ValidateDisabledPricing((Dictionary<string, object>)pricing);
                IList attractions = RequireArray(page, "attractions");
                IList settingsRows = RequireArray(page, "settings");
                IList pageItems = RequireArray(page, "items");
                string pageMaterial = EdgeCanonicalJson.Canonicalize(new JavaScriptSerializer().Serialize(new Dictionary<string, object>
                {
                    { "pricing", pricing }, { "attractions", attractions }, { "settings", settingsRows }
                }));

                if (firstPage == null)
                {
                    firstPage = page;
                    snapshotId = currentSnapshotId;
                    snapshotVersion = currentVersion;
                    documentHash = currentDocumentHash;
                    generatedAt = currentGeneratedAt;
                    validFrom = currentValidFrom;
                    validUntil = currentValidUntil;
                    signingKeyId = pageKeyId;
                    previousPageMaterial = pageMaterial;
                }
                else if (!String.Equals(snapshotId, currentSnapshotId, StringComparison.Ordinal) ||
                    !String.Equals(snapshotVersion, currentVersion, StringComparison.Ordinal) ||
                    !String.Equals(documentHash, currentDocumentHash, StringComparison.Ordinal) ||
                    !String.Equals(generatedAt, currentGeneratedAt, StringComparison.Ordinal) ||
                    !String.Equals(validFrom, currentValidFrom, StringComparison.Ordinal) ||
                    !String.Equals(validUntil, currentValidUntil, StringComparison.Ordinal) ||
                    !String.Equals(signingKeyId, pageKeyId, StringComparison.Ordinal) ||
                    !String.Equals(previousPageMaterial, pageMaterial, StringComparison.Ordinal))
                    throw new EdgeApiException("inconsistent-snapshot-pages", false);

                foreach (object item in pageItems)
                {
                    ValidateSnapshotItem(item);
                    allItems.Add(item);
                    if (allItems.Count > MaximumSnapshotItems) throw new EdgeApiException("snapshot-too-large", false);
                }

                string nextCursor = OptionalString(page, "nextCursor");
                if (!page.ContainsKey("nextCursor") || (nextCursor != null && nextCursor.Length > 1000))
                    throw new EdgeApiException("invalid-snapshot-cursor", false);
                if (String.IsNullOrEmpty(nextCursor)) break;
                if (!seenCursors.Add(nextCursor)) throw new EdgeApiException("invalid-snapshot-cursor", false);
                int cursorOffset = ValidateCursor(nextCursor, snapshotId, branchId, previousCursorOffset);
                previousCursorOffset = cursorOffset;
                requestCursor = nextCursor;
                if (pageNumber == MaximumSnapshotPages - 1) throw new EdgeApiException("snapshot-too-large", false);
            }

            if (firstPage == null) throw new EdgeApiException("invalid-snapshot", false);
            var hashMaterial = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                { "snapshotId", snapshotId }, { "snapshotVersion", snapshotVersion }, { "branchId", branchId },
                { "generatedAt", generatedAt }, { "validFrom", validFrom }, { "validUntil", validUntil },
                { "pricing", firstPage["pricing"] }, { "attractions", firstPage["attractions"] },
                { "settings", firstPage["settings"] }, { "items", allItems }
            };
            string hashJson = new JavaScriptSerializer { MaxJsonLength = MaximumResponseBytes, RecursionLimit = 128 }.Serialize(hashMaterial);
            string computedDocumentHash = EdgeCanonicalJson.Sha256Hex(EdgeCanonicalJson.Canonicalize(hashJson));
            if (!String.Equals(computedDocumentHash, documentHash, StringComparison.Ordinal))
                throw new EdgeApiException("invalid-snapshot-hash", false);

            var cardMappings = new List<EdgeCardMappingDto>();
            foreach (object item in allItems)
            {
                var itemObject = item as Dictionary<string, object>;
                if (itemObject == null || !String.Equals(RequireString(itemObject, "kind", 32), "card_mapping", StringComparison.Ordinal)) continue;
                var value = (Dictionary<string, object>)RequireObject(itemObject, "value");
                cardMappings.Add(new EdgeCardMappingDto
                {
                    CardId = RequireString(value, "cardId", 128),
                    CardUidHash = RequireString(value, "cardUidHash", 64),
                    HouseholdId = OptionalString(value, "householdId"),
                    MemberId = OptionalString(value, "memberId")
                });
            }

            return new EdgeBranchSnapshotDto
            {
                Version = snapshotVersion,
                IssuedAt = generatedAt,
                ExpiresAt = validUntil,
                BranchId = branchId,
                Cursor = null,
                Devices = new List<EdgeDeviceMappingDto>(),
                PricingRules = new List<EdgePricingRuleDto>(),
                Customers = new List<EdgeCustomerDto>(),
                Households = new List<EdgeHouseholdDto>(),
                Members = new List<EdgeMemberDto>(),
                Cards = cardMappings
            };
        }

        public EdgeCustomerSearchPage SearchCustomers(string query, string cursor, int pageSize)
        {
            AuthorizationGuard.Require("customers.view");
            query = (query ?? String.Empty).Trim();
            if (query.Length < 1 || query.Length > 120) throw new ArgumentException("عبارت جست‌وجو معتبر نیست.", "query");
            if (pageSize < 1 || pageSize > 100) throw new ArgumentOutOfRangeException("pageSize");
            Guid cursorId;
            if (!String.IsNullOrWhiteSpace(cursor) && !Guid.TryParse(cursor, out cursorId))
                throw new ArgumentException("نشانگر صفحه معتبر نیست.", "cursor");
            string path = "/internal/v2/edge/customers?branchId=" + Uri.EscapeDataString(settings.EdgeBranchId.Trim()) +
                "&query=" + Uri.EscapeDataString(query) + "&pageSize=" + pageSize.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (!String.IsNullOrWhiteSpace(cursor)) path += "&cursor=" + Uri.EscapeDataString(cursor.Trim());
            EdgeCustomerSearchPage result = Deserialize<EdgeCustomerSearchPage>(Send("GET", BuildUri(path), new byte[0]));
            if (result.Items == null || result.Items.Count > pageSize || result.Items.Any(item =>
                item == null || !IsGuid(item.CustomerId) || !IsPositiveVersion(item.RecordVersion) ||
                !IsMaskedPhone(item.MaskedPhone) || !IsOptionalGuid(item.HouseholdId) || !IsOptionalGuid(item.MemberId)))
                throw new EdgeApiException("invalid-response", true);
            return result;
        }

        public EdgeCustomerProfile ReadCustomerProfile(string customerId)
        {
            AuthorizationGuard.Require("customers.view");
            if (!IsGuid(customerId)) throw new ArgumentException("شناسهٔ مشتری معتبر نیست.", "customerId");
            string path = "/internal/v2/edge/customers/" + Uri.EscapeDataString(customerId.Trim()) +
                "?branchId=" + Uri.EscapeDataString(settings.EdgeBranchId.Trim());
            EdgeCustomerProfile profile = Deserialize<EdgeCustomerProfile>(Send("GET", BuildUri(path), new byte[0]));
            if (profile == null || !IsGuid(profile.CustomerId) || !IsPositiveVersion(profile.RecordVersion) || !IsMaskedPhone(profile.MaskedPhone))
                throw new EdgeApiException("invalid-response", true);
            return profile;
        }

        public EdgeHouseholdRead ReadHousehold(string householdId)
        {
            AuthorizationGuard.Require("families.view");
            if (!IsGuid(householdId)) throw new ArgumentException("شناسهٔ خانواده معتبر نیست.", "householdId");
            string path = "/internal/v2/edge/households/" + Uri.EscapeDataString(householdId.Trim()) +
                "?branchId=" + Uri.EscapeDataString(settings.EdgeBranchId.Trim());
            EdgeHouseholdRead household = Deserialize<EdgeHouseholdRead>(Send("GET", BuildUri(path), new byte[0]));
            if (household == null || !IsGuid(household.Id) || !IsPositiveVersion(household.RecordVersion) || household.Members == null ||
                household.Members.Any(item => item == null || !IsGuid(item.Id) || !IsPositiveVersion(item.RecordVersion)))
                throw new EdgeApiException("invalid-response", true);
            return household;
        }

        public EdgeProfileMutationResult PatchCustomerProfile(string customerId, int expectedVersion,
            IDictionary<string, object> changes, string operatorToken, string idempotencyKey)
        {
            AuthorizationGuard.Require("customers.profile.update");
            if (!IsGuid(customerId) || expectedVersion < 1 || changes == null || changes.Count == 0)
                throw new ArgumentException("اطلاعات ویرایش پرونده معتبر نیست.");
            var payload = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                { "branchId", settings.EdgeBranchId.Trim() }, { "expectedVersion", expectedVersion }
            };
            var allowed = new HashSet<string>(new[] { "firstName", "lastName", "birthDate", "gender", "city", "avatarPreset" }, StringComparer.Ordinal);
            foreach (KeyValuePair<string, object> change in changes)
            {
                if (!allowed.Contains(change.Key)) throw new ArgumentException("فیلد ویرایش‌شده در قرارداد مرکزی وجود ندارد.", "changes");
                payload.Add(change.Key, change.Value);
            }
            string body = SerializeOperatorPayload(payload);
            string path = "/internal/v2/edge/customers/" + Uri.EscapeDataString(customerId.Trim());
            EdgeProfileMutationResult result = SendOperatorRequest<EdgeProfileMutationResult>("PATCH", path, body, operatorToken, RequireIdempotencyKey(idempotencyKey));
            if (result == null || !String.Equals(result.CustomerId, customerId, StringComparison.OrdinalIgnoreCase) ||
                result.RecordVersion <= expectedVersion || !IsMaskedPhone(result.MaskedPhone))
                throw new EdgeApiException("invalid-response", true);
            return result;
        }

        public EdgeHouseholdMutationResult PatchHousehold(string householdId, int expectedVersion, string name,
            string operatorToken, string idempotencyKey)
        {
            AuthorizationGuard.Require("families.update");
            if (!IsGuid(householdId) || expectedVersion < 1 || String.IsNullOrWhiteSpace(name) || name.Trim().Length < 2 || name.Trim().Length > 80)
                throw new ArgumentException("اطلاعات ویرایش خانواده معتبر نیست.");
            var payload = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                { "branchId", settings.EdgeBranchId.Trim() }, { "expectedVersion", expectedVersion }, { "name", name.Trim() }
            };
            EdgeHouseholdMutationResult result = SendOperatorRequest<EdgeHouseholdMutationResult>("PATCH",
                "/internal/v2/edge/households/" + Uri.EscapeDataString(householdId.Trim()), SerializeOperatorPayload(payload),
                operatorToken, RequireIdempotencyKey(idempotencyKey));
            if (result == null || !String.Equals(result.HouseholdId, householdId, StringComparison.OrdinalIgnoreCase) || result.RecordVersion <= expectedVersion)
                throw new EdgeApiException("invalid-response", true);
            return result;
        }

        public EdgeHouseholdMemberMutationResult AddHouseholdMember(string householdId, string displayName, string relationship,
            DateTime? birthDate, bool? isDependent, string operatorToken, string idempotencyKey)
        {
            AuthorizationGuard.Require("families.members.create");
            if (!IsGuid(householdId) || String.IsNullOrWhiteSpace(displayName) || displayName.Trim().Length < 2 || displayName.Trim().Length > 80 ||
                String.IsNullOrWhiteSpace(relationship) || relationship.Trim().Length < 2 || relationship.Trim().Length > 40)
                throw new ArgumentException("اطلاعات عضو خانواده معتبر نیست.");
            var payload = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                { "branchId", settings.EdgeBranchId.Trim() }, { "displayName", displayName.Trim() }, { "relationship", relationship.Trim() }
            };
            if (birthDate.HasValue) payload.Add("birthDate", birthDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
            if (isDependent.HasValue) payload.Add("isDependent", isDependent.Value);
            EdgeHouseholdMemberMutationResult result = SendOperatorRequest<EdgeHouseholdMemberMutationResult>("POST",
                "/internal/v2/edge/households/" + Uri.EscapeDataString(householdId.Trim()) + "/members", SerializeOperatorPayload(payload),
                operatorToken, RequireIdempotencyKey(idempotencyKey));
            if (result == null || !IsGuid(result.MemberId) || !String.Equals(result.HouseholdId, householdId, StringComparison.OrdinalIgnoreCase) || result.RecordVersion < 1)
                throw new EdgeApiException("invalid-response", true);
            return result;
        }

        public EdgeTemporaryCustomerResult CreateTemporaryCustomer(string provisionalId, string operatorToken)
        {
            AuthorizationGuard.Require("customers.provisional.create");
            if (!IsGuid(provisionalId)) throw new ArgumentException("شناسهٔ موقت معتبر نیست.", "provisionalId");
            var payload = new Dictionary<string, object>(StringComparer.Ordinal) { { "provisionalId", provisionalId.Trim() } };
            EdgeTemporaryCustomerResult result = SendOperatorRequest<EdgeTemporaryCustomerResult>("POST",
                "/internal/v2/edge/customers/temporary", SerializeOperatorPayload(payload), operatorToken, RequireIdempotencyKey(provisionalId));
            if (result == null || !String.Equals(result.ProvisionalId, provisionalId, StringComparison.OrdinalIgnoreCase) ||
                !String.Equals(result.SourceInstanceId, settings.EdgeSourceInstanceId, StringComparison.OrdinalIgnoreCase) ||
                !String.Equals(result.BranchId, settings.EdgeBranchId, StringComparison.OrdinalIgnoreCase) || result.CustomerConfirmed ||
                !String.Equals(result.Status, "pending_review", StringComparison.Ordinal))
                throw new EdgeApiException("invalid-response", false);
            return result;
        }

        public EdgeCardAssignmentResult AssignCard(string cardId, string householdId, string memberId, int expectedVersion,
            string reason, string operatorToken, string idempotencyKey)
        {
            AuthorizationGuard.Require("cards.assign");
            if (!IsGuid(cardId) || !IsGuid(householdId) || !IsOptionalGuid(memberId) || expectedVersion < 1 ||
                String.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 8 || reason.Trim().Length > 500)
                throw new ArgumentException("اطلاعات اتصال کارت معتبر نیست.");
            var payload = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                { "cardId", cardId.Trim() }, { "householdId", householdId.Trim() }, { "memberId", String.IsNullOrWhiteSpace(memberId) ? null : memberId.Trim() },
                { "expectedVersion", expectedVersion }, { "reason", reason.Trim() }
            };
            EdgeCardAssignmentResult result = SendOperatorRequest<EdgeCardAssignmentResult>("POST", "/internal/v2/edge/cards/assignments",
                SerializeOperatorPayload(payload), operatorToken, RequireIdempotencyKey(idempotencyKey));
            if (result == null || !String.Equals(result.CardId, cardId, StringComparison.OrdinalIgnoreCase) ||
                !String.Equals(result.HouseholdId, householdId, StringComparison.OrdinalIgnoreCase) ||
                result.Status != "assigned" || result.VerificationStatus != "verified")
                throw new EdgeApiException("invalid-response", false);
            return result;
        }

        public EdgeCardBlockResult BlockCard(string cardId, int expectedVersion, string reason, string operatorToken, string idempotencyKey)
        {
            AuthorizationGuard.Require("cards.block");
            EdgeCardCommandArguments arguments = ValidateCardCommand(cardId, expectedVersion, reason);
            EdgeCardBlockResult result = SendOperatorRequest<EdgeCardBlockResult>("POST", "/internal/v2/edge/cards/" +
                Uri.EscapeDataString(arguments.CardId) + "/block", SerializeCardCommand(arguments), operatorToken, RequireIdempotencyKey(idempotencyKey));
            if (result == null || result.CardId != arguments.CardId || result.Status != "blocked" || result.RecordVersion <= expectedVersion)
                throw new EdgeApiException("invalid-response", false);
            return result;
        }

        public EdgeCardReplacementResult RequestCardReplacement(string cardId, int expectedVersion, string reason,
            string operatorToken, string idempotencyKey)
        {
            AuthorizationGuard.Require("cards.replace");
            EdgeCardCommandArguments arguments = ValidateCardCommand(cardId, expectedVersion, reason);
            EdgeCardReplacementResult result = SendOperatorRequest<EdgeCardReplacementResult>("POST", "/internal/v2/edge/cards/" +
                Uri.EscapeDataString(arguments.CardId) + "/replacement-requests", SerializeCardCommand(arguments), operatorToken, RequireIdempotencyKey(idempotencyKey));
            if (result == null || !IsGuid(result.RequestId) || result.CardId != arguments.CardId ||
                (result.Status != "requested" && result.Status != "in_review" && result.Status != "approved"))
                throw new EdgeApiException("invalid-response", false);
            return result;
        }

        private sealed class EdgeCardCommandArguments
        {
            public string CardId { get; set; }
            public int ExpectedVersion { get; set; }
            public string Reason { get; set; }
        }

        private static EdgeCardCommandArguments ValidateCardCommand(string cardId, int expectedVersion, string reason)
        {
            if (!IsGuid(cardId) || expectedVersion < 1 || String.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 8 || reason.Trim().Length > 500)
                throw new ArgumentException("اطلاعات عملیات کارت معتبر نیست.");
            return new EdgeCardCommandArguments { CardId = cardId.Trim(), ExpectedVersion = expectedVersion, Reason = reason.Trim() };
        }

        private static string SerializeCardCommand(EdgeCardCommandArguments value)
        {
            return SerializeOperatorPayload(new Dictionary<string, object>(StringComparer.Ordinal)
            {
                { "expectedVersion", value.ExpectedVersion }, { "reason", value.Reason }
            });
        }

        private T SendOperatorRequest<T>(string method, string path, string body, string operatorToken, string idempotencyKey)
        {
            if (String.IsNullOrWhiteSpace(operatorToken) || operatorToken.Trim().Length > 256)
                throw new EdgeApiException("operator-token-required", false);
            byte[] rawBody = body == null ? new byte[0] : Encoding.UTF8.GetBytes(body);
            byte[] response = Send(method, BuildUri(path), rawBody, operatorToken.Trim(), idempotencyKey);
            return Deserialize<T>(response);
        }

        private static string SerializeOperatorPayload(object payload)
        {
            return new JavaScriptSerializer { MaxJsonLength = MaximumResponseBytes, RecursionLimit = 64 }.Serialize(payload);
        }

        private static string RequireIdempotencyKey(string value)
        {
            Guid key;
            if (!Guid.TryParse(value, out key)) throw new ArgumentException("شناسهٔ تکرارناپذیری درخواست معتبر نیست.", "idempotencyKey");
            return key.ToString("D");
        }

        private static bool IsPositiveVersion(int value) { return value > 0; }

        private static bool IsMaskedPhone(string value)
        {
            if (value == null) return true;
            return value.Length <= 24 && value.IndexOfAny(new[] { '•', '*', 'x', 'X' }) >= 0;
        }

        private int ValidateCursor(string cursor, string expectedSnapshotId, string expectedBranchId, int previousOffset)
        {
            string[] parts = (cursor ?? String.Empty).Split('.');
            if (cursor == null || cursor.Length > 1000 || parts.Length != 2 || String.IsNullOrEmpty(parts[0]) || !IsLowerHex(parts[1], 64))
                throw new EdgeApiException("invalid-snapshot-cursor", false);
            string expectedSignature = ComputeSignature(secret, Encoding.ASCII.GetBytes(parts[0])).Substring("sha256=".Length);
            if (!FixedTimeEquals(parts[1], expectedSignature)) throw new EdgeApiException("invalid-snapshot-cursor", false);
            byte[] payloadBytes;
            try
            {
                string base64 = parts[0].Replace('-', '+').Replace('_', '/');
                base64 = base64.PadRight(base64.Length + ((4 - base64.Length % 4) % 4), '=');
                payloadBytes = Convert.FromBase64String(base64);
            }
            catch (FormatException) { throw new EdgeApiException("invalid-snapshot-cursor", false); }
            Dictionary<string, object> payload = DeserializeObject(Encoding.UTF8.GetString(payloadBytes));
            int offset;
            if (!String.Equals(RequireString(payload, "snapshotId", 128), expectedSnapshotId, StringComparison.Ordinal) ||
                !String.Equals(RequireString(payload, "sourceInstanceId", 128), settings.EdgeSourceInstanceId.Trim(), StringComparison.OrdinalIgnoreCase) ||
                !String.Equals(RequireString(payload, "branchId", 128), expectedBranchId, StringComparison.Ordinal) ||
                !String.Equals(RequireString(payload, "keyId", 128), settings.EdgeCredentialId.Trim(), StringComparison.Ordinal) ||
                !Int32.TryParse(Convert.ToString(payload["offset"], System.Globalization.CultureInfo.InvariantCulture), out offset) ||
                offset <= previousOffset)
                throw new EdgeApiException("invalid-snapshot-cursor", false);
            return offset;
        }

        private static void ValidateDisabledPricing(Dictionary<string, object> pricing)
        {
            if (!String.Equals(RequireString(pricing, "status", 32), "disabled", StringComparison.Ordinal) ||
                !pricing.ContainsKey("version") || pricing["version"] != null ||
                !String.Equals(RequireString(pricing, "reasonCode", 128), "PENDING_WINDOWS_GOLDEN_VECTORS", StringComparison.Ordinal) ||
                RequireArray(pricing, "rules").Count != 0)
                throw new EdgeApiException("pricing-contract-not-validated", false);
        }

        private static void ValidateSnapshotItem(object item)
        {
            var itemObject = item as Dictionary<string, object>;
            if (itemObject == null) throw new EdgeApiException("invalid-snapshot-item", false);
            string kind = RequireString(itemObject, "kind", 32);
            var value = RequireObject(itemObject, "value") as Dictionary<string, object>;
            if (value == null) throw new EdgeApiException("invalid-snapshot-item", false);
            if (value.Keys.Any(key => String.Equals(key, "uid", StringComparison.OrdinalIgnoreCase) ||
                                      String.Equals(key, "cardUid", StringComparison.OrdinalIgnoreCase) ||
                                      String.Equals(key, "rawUid", StringComparison.OrdinalIgnoreCase) ||
                                      String.Equals(key, "phone", StringComparison.OrdinalIgnoreCase) ||
                                      String.Equals(key, "address", StringComparison.OrdinalIgnoreCase) ||
                                      String.Equals(key, "birthDate", StringComparison.OrdinalIgnoreCase)))
                throw new EdgeApiException("snapshot-contains-private-data", false);

            if (kind == "card_mapping")
            {
                RequireIdentifier(value, "cardId", 128);
                string hash = RequireString(value, "cardUidHash", 64);
                if (!IsLowerHex(hash, 64)) throw new EdgeApiException("invalid-card-uid-hash", false);
                ValidateOptionalIdentifier(value, "householdId", 128);
                ValidateOptionalIdentifier(value, "memberId", 128);
            }
            else if (kind == "device")
            {
                RequireIdentifier(value, "deviceId", 128);
                RequireString(value, "displayName", 160);
                RequireString(value, "deviceType", 64);
                RequireString(value, "version", 128);
                if (value.ContainsKey("sourceDeviceId")) throw new EdgeApiException("invalid-snapshot-item", false);
            }
            else if (kind != "attraction" && kind != "setting")
                throw new EdgeApiException("invalid-snapshot-item", false);
        }

        private static Dictionary<string, object> DeserializeObject(string json)
        {
            try
            {
                var serializer = new JavaScriptSerializer { MaxJsonLength = MaximumResponseBytes, RecursionLimit = 128 };
                var result = serializer.DeserializeObject(json) as Dictionary<string, object>;
                if (result == null) throw new FormatException();
                return result;
            }
            catch (Exception) { throw new EdgeApiException("invalid-response", true); }
        }

        private static object RequireObject(Dictionary<string, object> source, string key)
        {
            object value;
            if (source == null || !source.TryGetValue(key, out value) || !(value is Dictionary<string, object>))
                throw new EdgeApiException("invalid-snapshot", false);
            return value;
        }

        private static IList RequireArray(Dictionary<string, object> source, string key)
        {
            object value;
            if (source == null || !source.TryGetValue(key, out value) || !(value is IList))
                throw new EdgeApiException("invalid-snapshot", false);
            return (IList)value;
        }

        private static string RequireString(Dictionary<string, object> source, string key, int maximumLength)
        {
            string value = OptionalString(source, key);
            if (String.IsNullOrWhiteSpace(value) || value.Length > maximumLength)
                throw new EdgeApiException("invalid-snapshot", false);
            return value;
        }

        private static string OptionalString(Dictionary<string, object> source, string key)
        {
            object value;
            if (source == null || !source.TryGetValue(key, out value) || value == null) return null;
            string result = value as string;
            if (result == null) throw new EdgeApiException("invalid-snapshot", false);
            return result;
        }

        private static void RequireIdentifier(Dictionary<string, object> source, string key, int maximumLength)
        {
            string value = RequireString(source, key, maximumLength);
            Guid id;
            if (!Guid.TryParse(value, out id)) throw new EdgeApiException("invalid-snapshot-item", false);
        }

        private static void ValidateOptionalIdentifier(Dictionary<string, object> source, string key, int maximumLength)
        {
            string value = OptionalString(source, key);
            Guid id;
            if (value != null && (value.Length > maximumLength || !Guid.TryParse(value, out id)))
                throw new EdgeApiException("invalid-snapshot-item", false);
        }

        private static bool IsSignature(string value)
        {
            return value != null && value.Length == 71 && value.StartsWith("sha256=", StringComparison.Ordinal) && IsLowerHex(value.Substring(7), 64);
        }

        private static bool IsLowerHex(string value, int length)
        {
            if (value == null || value.Length != length) return false;
            foreach (char ch in value) if (!(ch >= '0' && ch <= '9') && !(ch >= 'a' && ch <= 'f')) return false;
            return true;
        }

        private static bool FixedTimeEquals(string left, string right)
        {
            if (left == null || right == null || left.Length != right.Length) return false;
            int difference = 0;
            for (int index = 0; index < left.Length; index++) difference |= left[index] ^ right[index];
            return difference == 0;
        }

        private byte[] Send(string method, Uri uri, byte[] body, string operatorToken = null, string idempotencyKey = null)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = method;
                request.AllowAutoRedirect = false;
                request.Accept = "application/json";
                request.ContentType = "application/json; charset=utf-8";
                request.Timeout = 15000;
                request.ReadWriteTimeout = 20000;
                request.KeepAlive = false;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.Headers["x-edge-installation-id"] = settings.EdgeSourceInstanceId.Trim();
                request.Headers["x-edge-key-id"] = settings.EdgeCredentialId.Trim();
                request.Headers["x-edge-signature"] = ComputeSignature(secret, body);
                if (!String.IsNullOrWhiteSpace(operatorToken)) request.Headers[HttpRequestHeader.Authorization] = "Bearer " + operatorToken;
                if (!String.IsNullOrWhiteSpace(idempotencyKey)) request.Headers["Idempotency-Key"] = idempotencyKey;
                if (!String.Equals(method, "GET", StringComparison.OrdinalIgnoreCase) &&
                    !String.Equals(method, "HEAD", StringComparison.OrdinalIgnoreCase))
                {
                    request.ContentLength = body.Length;
                    using (Stream stream = request.GetRequestStream()) stream.Write(body, 0, body.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                    return ReadBounded(stream, MaximumResponseBytes);
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                if (response != null)
                {
                    int status = (int)response.StatusCode;
                    string errorCode = null;
                    try
                    {
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            byte[] errorBytes = ReadBounded(responseStream, 64 * 1024);
                            if (errorBytes.Length > 0)
                            {
                                Dictionary<string, object> error = DeserializeObject(Encoding.UTF8.GetString(errorBytes));
                                errorCode = OptionalString(error, "code") ?? OptionalString(error, "errorCode");
                            }
                        }
                    }
                    catch (Exception) { }
                    response.Dispose();
                    bool retryable = status == 408 || status == 429 || status >= 500;
                    string safeCode = status == 409 ? "VERSION_CONFLICT" :
                        (!String.IsNullOrWhiteSpace(errorCode) && errorCode.Length <= 80 ? errorCode :
                        "http-" + status.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    throw new EdgeApiException(safeCode, retryable);
                }
                throw new EdgeApiException("network-unavailable", true);
            }
            catch (EdgeApiException)
            {
                throw;
            }
            catch (IOException)
            {
                throw new EdgeApiException("network-unavailable", true);
            }
            catch (Exception)
            {
                throw new EdgeApiException("request-failed", true);
            }
        }

        private Uri BuildUri(string pathAndQuery)
        {
            return new Uri(settings.EdgeApiBaseUrl.TrimEnd('/') + pathAndQuery, UriKind.Absolute);
        }

        private void ValidateConfiguration()
        {
            Uri baseUri;
            Guid installationId, organizationId, branchId;
            if (!Uri.TryCreate(settings.EdgeApiBaseUrl, UriKind.Absolute, out baseUri) ||
                !String.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
                !String.IsNullOrEmpty(baseUri.UserInfo) || !String.IsNullOrEmpty(baseUri.Query) || !String.IsNullOrEmpty(baseUri.Fragment))
                throw new EdgeApiException("invalid-configuration", false);
            if (String.IsNullOrWhiteSpace(settings.EdgeCredentialId) || secret == null || secret.Length < 32 ||
                !Guid.TryParse(settings.EdgeOrganizationId, out organizationId) || !Guid.TryParse(settings.EdgeBranchId, out branchId) ||
                !Guid.TryParse(settings.EdgeSourceInstanceId, out installationId))
                throw new EdgeApiException("incomplete-configuration", false);
        }

        private static bool IsGuid(string value)
        {
            Guid parsed;
            return Guid.TryParse(value, out parsed);
        }

        private static bool IsOptionalGuid(string value)
        {
            return String.IsNullOrEmpty(value) || IsGuid(value);
        }

        private static bool IsSupportedTransactionKind(string value)
        {
            switch (value)
            {
                case "top_up":
                case "spend":
                case "refund":
                case "adjustment":
                case "issuance":
                case "card_replacement": return true;
                default: return false;
            }
        }

        private static bool IsSupportedOperationResult(string value)
        {
            return value == "success" || value == "cancelled" || value == "failed";
        }

        internal static string ComputeSignature(byte[] secretKey, byte[] rawBody)
        {
            if (secretKey == null || secretKey.Length < 32) throw new ArgumentException("کلید HMAC نامعتبر است.", "secretKey");
            if (rawBody == null) rawBody = new byte[0];
            using (var hmac = new HMACSHA256(secretKey))
                return "sha256=" + ToLowerHex(hmac.ComputeHash(rawBody));
        }

        private static string ToLowerHex(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (byte value in bytes) builder.Append(value.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        private static byte[] Serialize<T>(T value)
        {
            var serializer = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings { MaxItemsInObjectGraph = 250000 });
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, value);
                return stream.ToArray();
            }
        }

        private static T Deserialize<T>(byte[] bytes)
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings { MaxItemsInObjectGraph = 250000 });
                using (var stream = new MemoryStream(bytes, false)) return (T)serializer.ReadObject(stream);
            }
            catch (Exception)
            {
                throw new EdgeApiException("invalid-response", true);
            }
        }

        private static byte[] ReadBounded(Stream stream, int maximumBytes)
        {
            if (stream == null) return new byte[0];
            using (var buffer = new MemoryStream())
            {
                var chunk = new byte[8192];
                int read;
                while ((read = stream.Read(chunk, 0, chunk.Length)) > 0)
                {
                    if (buffer.Length + read > maximumBytes) throw new EdgeApiException("response-too-large", false);
                    buffer.Write(chunk, 0, read);
                }
                return buffer.ToArray();
            }
        }
    }
}
