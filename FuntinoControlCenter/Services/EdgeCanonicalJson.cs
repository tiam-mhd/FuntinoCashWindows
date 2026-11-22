using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace FuntinoControlCenter.Services
{
    internal static class EdgeCanonicalJson
    {
        internal static string Canonicalize(string json)
        {
            if (json == null) throw new ArgumentNullException("json");
            var serializer = new JavaScriptSerializer { MaxJsonLength = 8 * 1024 * 1024, RecursionLimit = 128 };
            return WriteValue(serializer.DeserializeObject(json));
        }

        internal static string CanonicalizeWithoutProperty(string json, string propertyName)
        {
            if (String.IsNullOrEmpty(propertyName)) throw new ArgumentException("A property name is required.", "propertyName");
            var serializer = new JavaScriptSerializer { MaxJsonLength = 8 * 1024 * 1024, RecursionLimit = 128 };
            var root = serializer.DeserializeObject(json) as IDictionary<string, object>;
            if (root == null) throw new FormatException("The signed JSON value must be an object.");
            var copy = new Dictionary<string, object>(root, StringComparer.Ordinal);
            copy.Remove(propertyName);
            return WriteValue(copy);
        }

        internal static string ComputeCanonicalSignature(byte[] secretKey, string json, string excludedProperty)
        {
            string canonical = CanonicalizeWithoutProperty(json, excludedProperty);
            return EdgeApiClient.ComputeSignature(secretKey, Encoding.UTF8.GetBytes(canonical));
        }

        internal static string Sha256Hex(string value)
        {
            using (var sha = SHA256.Create())
                return ToLowerHex(sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? String.Empty)));
        }

        private static string WriteValue(object value)
        {
            if (value == null) return "null";
            var text = value as string;
            if (text != null) return Quote(text);
            if (value is bool) return (bool)value ? "true" : "false";

            var dictionary = value as IDictionary<string, object>;
            if (dictionary != null)
            {
                return "{" + String.Join(",", dictionary.Keys.OrderBy(key => key, StringComparer.Ordinal)
                    .Select(key => Quote(key) + ":" + WriteValue(dictionary[key]))) + "}";
            }

            var sequence = value as IEnumerable;
            if (sequence != null)
            {
                var values = new List<string>();
                foreach (object item in sequence) values.Add(WriteValue(item));
                return "[" + String.Join(",", values) + "]";
            }

            if (value is byte || value is sbyte || value is short || value is ushort || value is int ||
                value is uint || value is long || value is ulong)
                return Convert.ToString(value, CultureInfo.InvariantCulture);

            throw new FormatException("The signed JSON contains a value outside the Edge integer schema.");
        }

        private static string Quote(string value)
        {
            var builder = new StringBuilder(value.Length + 2);
            builder.Append('"');
            foreach (char ch in value)
            {
                switch (ch)
                {
                    case '"': builder.Append("\\\""); break;
                    case '\\': builder.Append("\\\\"); break;
                    case '\b': builder.Append("\\b"); break;
                    case '\f': builder.Append("\\f"); break;
                    case '\n': builder.Append("\\n"); break;
                    case '\r': builder.Append("\\r"); break;
                    case '\t': builder.Append("\\t"); break;
                    default:
                        if (ch < 0x20) builder.Append("\\u").Append(((int)ch).ToString("x4", CultureInfo.InvariantCulture));
                        else builder.Append(ch);
                        break;
                }
            }
            return builder.Append('"').ToString();
        }

        private static string ToLowerHex(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (byte value in bytes) builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
            return builder.ToString();
        }
    }
}
