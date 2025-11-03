using SmartKey.Application.Common.Interfaces.Services;

namespace SmartKey.Infrastructure.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
