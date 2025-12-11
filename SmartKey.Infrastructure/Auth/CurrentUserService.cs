using Microsoft.AspNetCore.Http;
using SmartKey.Application.Common.Interfaces.Auth;

namespace SmartKey.Infrastructure.Auth
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst("userId")?
                    .Value;

                return Guid.TryParse(userIdClaim, out var id) ? id : null;
            }
        }

        public string? Provider =>
            _httpContextAccessor.HttpContext?
                .User?
                .FindFirst("provider")?
                .Value;
    }
}
