namespace Helpdesk.Application.Features.Emails
{
    public class EmailMessage
    {
        public string To { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string HtmlBody { get; set; } = string.Empty;
    }
}
