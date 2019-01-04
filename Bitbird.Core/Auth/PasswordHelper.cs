using System;
using System.Linq;
using System.Security.Cryptography;

namespace Bitbird.Core.Auth
{
    public static class PasswordHelper
    {
        public static void StorePassword(string password, out string key, out string salt)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, 32))
            {
                var saltBytes = deriveBytes.Salt;
                var keyBytes = deriveBytes.GetBytes(32);

                salt = Convert.ToBase64String(saltBytes);
                key = Convert.ToBase64String(keyBytes);
            }
        }
        public static bool CheckPassword(string password, string key, string salt)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(salt) || string.IsNullOrWhiteSpace(password))
                return false;

            var saltBytes = Convert.FromBase64String(salt);
            var keyBytes = Convert.FromBase64String(key);

            using (var deriveBytes = new Rfc2898DeriveBytes(password, saltBytes))
            {
                var testKeyBytes = deriveBytes.GetBytes(32);
                return testKeyBytes.SequenceEqual(keyBytes);
            }
        }
    }
}