using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace InternshipManagement.Api.Services
{
    public class EmailSender : IAppEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string message)
        {
            var smtpHost = _config["Email:Smtp"];
            var smtpPort = int.Parse(_config["Email:Port"]);
            var smtpUser = _config["Email:Username"];
            var smtpPass = _config["Email:Password"];
            var fromEmail = _config["Email:From"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage(fromEmail, to, subject, message)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mailMessage);
        }
    }
}
