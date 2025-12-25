using System.Security.Cryptography;
using System.Text;

namespace SmartKey.Infrastructure.MQTT
{
    public static class MqttFingerprint
    {
        public static string Create(string topic, string payload)
        {
            var raw = $"{topic}|{payload}";
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes);
        }
    }
}
