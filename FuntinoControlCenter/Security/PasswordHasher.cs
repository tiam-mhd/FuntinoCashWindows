using System;
using System.Security.Cryptography;
using System.Text;

namespace FuntinoControlCenter.Security
{
    internal static class PasswordHasher
    {
        public const int Iterations = 120000;
        private const int SaltLength = 32;
        private const int HashLength = 32;

        public static void Create(string password, out byte[] salt, out byte[] hash)
        {
            if (password == null) throw new ArgumentNullException("password");
            salt = new byte[SaltLength];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            using (var derive = new Rfc2898DeriveBytes(password, salt, Iterations)) hash = derive.GetBytes(HashLength);
        }

        public static string GenerateTemporaryPassword()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%&*+-_";
            const int length = 24;
            int limit = 256 - (256 % alphabet.Length);
            var result = new StringBuilder(length);
            var bytes = new byte[48];
            using (var random = RandomNumberGenerator.Create())
            {
                while (result.Length < length)
                {
                    random.GetBytes(bytes);
                    for (int i = 0; i < bytes.Length && result.Length < length; i++)
                    {
                        if (bytes[i] >= limit) continue;
                        result.Append(alphabet[bytes[i] % alphabet.Length]);
                    }
                }
            }
            return result.ToString();
        }

        public static bool Verify(string password, byte[] salt, byte[] expected, int iterations)
        {
            if (password == null || salt == null || expected == null || expected.Length != HashLength || iterations < 10000) return false;
            byte[] actual;
            using (var derive = new Rfc2898DeriveBytes(password, salt, iterations)) actual = derive.GetBytes(expected.Length);
            int diff = 0;
            for (int i = 0; i < actual.Length; i++) diff |= actual[i] ^ expected[i];
            return diff == 0;
        }
    }
}
