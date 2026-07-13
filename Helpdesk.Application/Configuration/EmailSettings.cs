namespace Helpdesk.Application.Configuration
{
    // Bound from the "EmailSettings" configuration section. Credentials come from configuration
    // (appsettings / environment / user-secrets) only — never hardcoded.
    public class EmailSettings
    {
        // Master switch. When false the notification service is a no-op (safe default with no credentials).
        public bool Enabled { get; set; }

        // "Smtp" or "SendGrid".
        public string Provider { get; set; } = "Smtp";

        public string FromEmail { get; set; } = string.Empty;

        public string FromName { get; set; } = string.Empty;

        public SmtpSettings Smtp { get; set; } = new();

        public SendGridSettings SendGrid { get; set; } = new();
    }

    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; } = 587;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool EnableSsl { get; set; } = true;
    }

    public class SendGridSettings
    {
        public string ApiKey { get; set; } = string.Empty;
    }
}
