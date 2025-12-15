using SmartKey.Application.Common.Mappings;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.UserFeatures.Dtos
{
    public class UserDto : IMapFrom<User>
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
    }
}
