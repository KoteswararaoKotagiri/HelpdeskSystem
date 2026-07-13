using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Helpdesk.Application.Configuration;
using Helpdesk.Application.Features.Emails;
using Helpdesk.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Helpdesk.Infrastructure.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(
            IOptions<EmailSettings> settings,
            ILogger<SmtpEmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            using var client = new SmtpClient(_settings.Smtp.Host, _settings.Smtp.Port)
            {
                EnableSsl = _settings.Smtp.EnableSsl,
                Credentials = new NetworkCredential(
                    _settings.Smtp.Username,
                    _settings.Smtp.Password)
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = message.Subject,
                Body = message.HtmlBody,
                IsBodyHtml = true
            };

            mail.To.Add(message.To);

            await client.SendMailAsync(mail, cancellationToken);

            _logger.LogInformation(
                "SMTP email sent to {Recipient} with subject {Subject}",
                message.To, message.Subject);
        }
    }
}
