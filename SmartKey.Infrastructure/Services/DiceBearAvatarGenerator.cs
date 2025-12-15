using SmartKey.Application.Common.Interfaces.Services;

namespace SmartKey.Infrastructure.Services
{
    public class DiceBearAvatarGenerator : IAvatarGenerator
    {
        private const string DefaultAvatarUrl =
            "https://api.dicebear.com/9.x/adventurer/svg?seed=Chase";

        private readonly HttpClient _httpClient;

        public DiceBearAvatarGenerator(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateSvgAsync(string seed)
        {
            try
            {
                var url =
                    $"https://api.dicebear.com/7.x/avataaars/svg?seed={Uri.EscapeDataString(seed)}";

                return await _httpClient.GetStringAsync(url);
            }
            catch
            {
                return await _httpClient.GetStringAsync(DefaultAvatarUrl);
            }
        }
    }
}
