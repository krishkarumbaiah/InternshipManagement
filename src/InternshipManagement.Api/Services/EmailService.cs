using InternshipManagement.Api.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace InternshipManagement.Api.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_settings.Smtp, _settings.Port)
                {
                    Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                    EnableSsl = true
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(_settings.From, "IMS System"),
                    Subject = subject,
                    Body = body + $"<br><br>(Sent at {DateTime.UtcNow})",
                    IsBodyHtml = true
                };

                mail.To.Add(to);

                _logger.LogInformation("📧 Connecting to {Host}:{Port} as {User}", _settings.Smtp, _settings.Port, _settings.Username);

                await client.SendMailAsync(mail);

                _logger.LogInformation("✅ Email sent successfully to {Recipient}", to);
            }
            catch (SmtpException smtpEx)
            {
                // Detailed SMTP errors (like Gmail rejecting)
                _logger.LogError(smtpEx, "❌ SMTP Error while sending email to {Recipient}: {StatusCode} - {Message}", 
                    to, smtpEx.StatusCode, smtpEx.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ General error while sending email to {Recipient}", to);
                throw;
            }
        }
    }
}
