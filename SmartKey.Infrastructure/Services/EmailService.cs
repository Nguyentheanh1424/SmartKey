using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Services;
using System.Net;
using System.Net.Mail;

namespace SmartKey.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(
            ILogger<EmailService> logger,
            IConfiguration configuration
            )
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var host = _configuration["EmailSettings:Host"];
            var port = int.Parse(_configuration["EmailSettings:Port"]!);
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]!);
            var username = _configuration["EmailSettings:UserName"];
            var password = _configuration["EmailSettings:Password"];
            var from = _configuration["EmailSettings:From"];

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            var mail = new MailMessage
            {
                From = new MailAddress(from!),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(to);

            try
            {
                await client.SendMailAsync(mail);
                _logger.LogInformation($"Email sent to {to} - Subject: {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FAILED to send email");
                throw new AppException("Failed to send email. Please try again.");
            }
        }
    }
}
