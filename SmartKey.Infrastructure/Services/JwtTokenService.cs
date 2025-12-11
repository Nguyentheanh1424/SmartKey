using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SmartKey.Infrastructure.Services
{
    public class JwtTokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(
            IConfiguration config)
        {
            _config = config;
        }

        public TokenInfoDto ParseAccessToken(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(accessToken))
                throw new NotFoundException("Token không hợp lệ");

            var token = handler.ReadJwtToken(accessToken);

            var provider = token.Claims.FirstOrDefault(c => c.Type == "provider")?.Value
                           ?? "unknown";

            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (userIdClaim == null)
                throw new NotFoundException("Token không chứa userId");

            var userId = Guid.Parse(userIdClaim);

            var isExpired = token.ValidTo < DateTime.UtcNow;

            return new TokenInfoDto
            {
                UserId = userId,
                Provider = provider,
                IsExpired = isExpired
            };
        }

        public Task<(string accessToken, string refreshToken)> IssueAsync(Guid userId, AccountProvider provider)
        {
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var signingKey = _config["Jwt:SigningKey"];
            var accessMinutes = int.TryParse(_config["Jwt:AccessTokenMinutes"], out int value) ? value : 30;

            var keyBytes = Convert.FromHexString(signingKey!);
            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId", userId.ToString()),
                new Claim("provider", provider.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(accessMinutes),
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

            var refreshToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64)
            );

            return Task.FromResult((accessToken, refreshToken));
        }
    }
}
