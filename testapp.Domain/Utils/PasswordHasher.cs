using System.Security.Cryptography;

namespace testapp.Domain.Utils
{
    public class PasswordHasher
    {
        public static (string HashBase64, string SaltBase64) HashPassword(string password, int iterations = 10_000, int length = 32)
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[16];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(length);

            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

        public static bool Verify(string password, string storedHashBase64, string storedSaltBase64, int iterations = 10_000, int length = 32)
        {
            try
            {
                var salt = Convert.FromBase64String(storedSaltBase64);
                var storedHash = Convert.FromBase64String(storedHashBase64);

                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
                var computedHash = pbkdf2.GetBytes(length);

                var match = storedHash.SequenceEqual(computedHash);

                return match;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Verify] Error: " + ex.Message);
                return false;
            }
        }

    }
}
