using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Services;
using System.Net.Http.Json;

namespace SmartKey.Infrastructure.Services
{
    public class OAuthVerifier : IOAuthVerifier
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public OAuthVerifier(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<OAuthProfileDto> VerifyGoogleAsync(string token)
        {
            // Hardcode client ID
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[]
                {
                    "509252528902-9m50ml26rorl4ne179mcekn8vt6jvcus.apps.googleusercontent.com"
                }

            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);

            if (!payload.EmailVerified)
                throw new UnauthorizedException("Google email chưa được xác minh.");

            return new OAuthProfileDto
            {
                Uid = payload.Subject,
                Email = payload.Email,
                Name = payload.Name
            };
        }

        public async Task<OAuthProfileDto> VerifyFacebookAsync(string token)
        {
            var url =
            $"https://graph.facebook.com/me?fields=id,name,email&access_token={token}";

            var response = await _http.GetFromJsonAsync<FacebookMeResponse>(url);

            if (response == null || string.IsNullOrEmpty(response.Id))
                throw new UnauthorizedException("Facebook token không hợp lệ");

            var email = !string.IsNullOrEmpty(response.Email)
            ? response.Email
            : $"facebook_{response.Id}@no-email.local";

            return new OAuthProfileDto
            {
                Uid = response.Id,
                Email = email,
                Name = response.Name
            };
        }

        private class FacebookMeResponse
        {
            public string Id { get; set; } = default!;
            public string? Email { get; set; }
            public string? Name { get; set; }
        }
    }
}
