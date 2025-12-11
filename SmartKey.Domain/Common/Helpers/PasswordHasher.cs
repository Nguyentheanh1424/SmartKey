using System.Security.Cryptography;

namespace SmartKey.Domain.Common.Helpers
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;     // 128 bit
        private const int KeySize = 32;      // 256 bit
        private const int Iterations = 100_000;

        /// <summary>
        /// Hash password theo chuẩn PBKDF2 + Salt
        /// </summary>
        public static (string Hash, string Salt) Hash(string password)
        {
            // Sinh salt ngẫu nhiên
            var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);

            // Hash bằng PBKDF2
            var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                password,
                saltBytes,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize
            );

            return (
                Convert.ToBase64String(hashBytes),
                Convert.ToBase64String(saltBytes)
            );
        }

        /// <summary>
        /// Verify password với hash + salt
        /// </summary>
        public static bool Verify(string password, string storedHash, string storedSalt)
        {
            var saltBytes = Convert.FromBase64String(storedSalt);
            var hashBytes = Convert.FromBase64String(storedHash);

            // Hash lại password nhập vào
            var newHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                saltBytes,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize
            );

            // So sánh tránh timing attack
            return CryptographicOperations.FixedTimeEquals(newHash, hashBytes);
        }

    }
}
