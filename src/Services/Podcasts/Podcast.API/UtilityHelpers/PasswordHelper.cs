using System.Security.Cryptography;
using System.Text;

namespace Podcast.API.UtilityHelpers
{
    public static class PasswordHelper
    {
        public static string HashPasword(string password, out string salt)
        {
            var saltHex = RandomNumberGenerator.GetBytes(64);
            salt = Convert.ToHexString(saltHex);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                saltHex,
                35000,
                HashAlgorithmName.SHA512,
                64);
            return Convert.ToHexString(hash);
        }

        public static bool VerifyPassword(string password, string hash, string salt)
        {
            var saltHex = Convert.FromHexString(salt);
            var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, saltHex, 35000, HashAlgorithmName.SHA512, 64);
            return CryptographicOperations.FixedTimeEquals(hashToCompare, Convert.FromHexString(hash));
        }
    }
}
