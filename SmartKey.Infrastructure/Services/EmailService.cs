using Microsoft.Extensions.Logging;
using SmartKey.Application.Common.Interfaces.Services;

namespace SmartKey.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string to, string subject, string body)
        {
            // Giả lập việc gửi email
            _logger.LogInformation($"Email sent to {to} - Subject: {subject}");
            return Task.CompletedTask;
        }
    }
}
