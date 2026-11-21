using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;

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
            byte[] body = Serialize(new List<EdgeEventDto>(events));
            byte[] response = Send("POST", BuildUri("/internal/v2/edge/events"), body);
            var result = Deserialize<EdgeAcknowledgementResponse>(response);
            if (result == null || result.Results == null) throw new EdgeApiException("invalid-response", true);
            return result;
        }

        public EdgeBranchSnapshotDto GetSnapshot(string cursor)
        {
            string query = "?branchId=" + Uri.EscapeDataString(settings.EdgeBranchId.Trim());
            if (!String.IsNullOrWhiteSpace(cursor)) query += "&cursor=" + Uri.EscapeDataString(cursor.Trim());
            byte[] response = Send("GET", BuildUri("/internal/v2/edge/snapshot" + query), new byte[0]);
            return Deserialize<EdgeBranchSnapshotDto>(response);
        }

        private byte[] Send(string method, Uri uri, byte[] body)
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
                request.Headers["X-Funtino-Credential-Id"] = settings.EdgeCredentialId.Trim();
                request.Headers["X-Funtino-Timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", System.Globalization.CultureInfo.InvariantCulture);
                request.Headers["X-Funtino-Signature"] = ComputeSignature(secret, body);
                if (String.Equals(method, "POST", StringComparison.OrdinalIgnoreCase))
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
                    response.Dispose();
                    bool retryable = status == 408 || status == 429 || status >= 500;
                    throw new EdgeApiException("http-" + status.ToString(System.Globalization.CultureInfo.InvariantCulture), retryable);
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
            if (!Uri.TryCreate(settings.EdgeApiBaseUrl, UriKind.Absolute, out baseUri) ||
                !String.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
                !String.IsNullOrEmpty(baseUri.UserInfo) || !String.IsNullOrEmpty(baseUri.Query) || !String.IsNullOrEmpty(baseUri.Fragment))
                throw new EdgeApiException("invalid-configuration", false);
            if (String.IsNullOrWhiteSpace(settings.EdgeCredentialId) || secret == null || secret.Length < 32 ||
                String.IsNullOrWhiteSpace(settings.EdgeOrganizationId) || String.IsNullOrWhiteSpace(settings.EdgeBranchId) ||
                String.IsNullOrWhiteSpace(settings.EdgeSourceInstanceId))
                throw new EdgeApiException("incomplete-configuration", false);
        }

        internal static string ComputeSignature(byte[] secretKey, byte[] rawBody)
        {
            if (secretKey == null || secretKey.Length < 32) throw new ArgumentException("کلید HMAC نامعتبر است.", "secretKey");
            if (rawBody == null) rawBody = new byte[0];
            using (var hmac = new HMACSHA256(secretKey))
                return Convert.ToBase64String(hmac.ComputeHash(rawBody));
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
