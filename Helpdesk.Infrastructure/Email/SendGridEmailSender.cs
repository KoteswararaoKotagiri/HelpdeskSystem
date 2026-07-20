using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Helpdesk.Application.Configuration;
using Helpdesk.Application.Features.Emails;
using Helpdesk.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Helpdesk.Infrastructure.Email
{
    // Sends via the SendGrid v3 REST API using a typed HttpClient (no extra NuGet dependency).
    public class SendGridEmailSender : IEmailSender
    {
        private const string SendEndpoint = "https://api.sendgrid.com/v3/mail/send";

        private readonly HttpClient _httpClient;
        private readonly EmailSettings _settings;
        private readonly ILogger<SendGridEmailSender> _logger;

        public SendGridEmailSender(
            HttpClient httpClient,
            IOptions<EmailSettings> settings,
            ILogger<SendGridEmailSender> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                personalizations = new[]
                {
                    new { to = new[] { new { email = message.To } } }
                },
                from = new { email = _settings.FromEmail, name = _settings.FromName },
                subject = message.Subject,
                content = new[]
                {
                    new { type = "text/html", value = message.HtmlBody }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, SendEndpoint)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json")
            };

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.SendGrid.ApiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation(
                "SendGrid email sent to {Recipient} with subject {Subject}",
                message.To, message.Subject);
        }
    }
}
