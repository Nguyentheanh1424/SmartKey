using SmartKey.Application.Common.Mappings;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.UserAuthFeatures.Dtos
{
    public class LoginResultDto : IMapFrom<UserAuth>
    {
        public Guid UserId { get; set; } = default!;
        public string Provider { get; set; } = default!;
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
    }
}
