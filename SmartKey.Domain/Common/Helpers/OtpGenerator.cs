using System.Security.Cryptography;
using System.Text;

namespace SmartKey.Domain.Common.Helpers
{
    public class OtpGenerator
    {
        private const string Digits = "0123456789";
        private const string AlphaNumeric = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const string LowerAlphaNumeric = "abcdefghjkmnpqrstuvwxyz23456789";
        private const string Charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static string GenerateNumeric(int length = 6)
        {
            return GenerateFromCharset(Digits, length);
        }

        public static string GenerateAlphaNumeric(int length = 6)
        {
            return GenerateFromCharset(AlphaNumeric, length);
        }

        public static string GenerateLowerAlphaNumeric(int length = 6)
        {
            return GenerateFromCharset(LowerAlphaNumeric, length);
        }

        public static string Generate(int length = 6)
        {
            return GenerateFromCharset(Charset, length);
        }

        private static string GenerateFromCharset(string charset, int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "OTP length must be greater than 0.");
            }

            var result = new StringBuilder(length);

            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);

            for (int i = 0; i < length; i++)
            {
                int index = bytes[i] % charset.Length;
                result.Append(charset[index]);
            }

            return result.ToString();
        }
    }
}
