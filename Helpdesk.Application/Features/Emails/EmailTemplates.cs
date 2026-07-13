using System;
using System.Net;

namespace Helpdesk.Application.Features.Emails
{
    // Reusable HTML email templates. One shared layout; each template type supplies its own
    // subject, heading, intro and action label. New ticket templates = add an enum value + a case;
    // unrelated future emails (password reset, etc.) build their own body and reuse IEmailSender.
    public static class EmailTemplates
    {
        public static EmailMessage Build(EmailTemplateType type, TicketEmailModel model)
        {
            var (heading, intro, action) = Describe(type, model);

            var subject = $"[{model.TicketNumber}] {heading}: {model.Title}";
            var html = Layout(heading, intro, action, model);

            return new EmailMessage
            {
                Subject = subject,
                HtmlBody = html
            };
        }

        private static (string Heading, string Intro, string Action) Describe(
            EmailTemplateType type, TicketEmailModel model) => type switch
        {
            EmailTemplateType.TicketCreated =>
                ("Ticket Created", "A new support ticket has been created.", "Ticket Created"),
            EmailTemplateType.TicketAssigned =>
                ("Ticket Assigned", $"This ticket has been assigned to {model.AssignedEngineer}.", "Ticket Assigned"),
            EmailTemplateType.TicketUpdated =>
                ("Ticket Updated", $"The ticket status is now \"{model.Status}\".", "Status Changed"),
            EmailTemplateType.CommentAdded =>
                ("New Comment", "A new comment was added to this ticket.", "Comment Added"),
            EmailTemplateType.TicketClosed =>
                ("Ticket Closed", "This ticket has been closed.", "Ticket Closed"),
            _ => ("Ticket Notification", "This ticket was updated.", "Updated")
        };

        private static string Layout(string heading, string intro, string action, TicketEmailModel model)
        {
            string Enc(string value) => WebUtility.HtmlEncode(value ?? string.Empty);

            return $@"<!DOCTYPE html>
<html>
<body style=""margin:0;padding:0;background:#f4f5f7;font-family:Segoe UI,Arial,sans-serif;color:#172b4d;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""padding:24px 0;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff;border-radius:8px;overflow:hidden;border:1px solid #e0e4e9;"">
          <tr>
            <td style=""background:#0052cc;padding:20px 28px;color:#ffffff;font-size:18px;font-weight:600;"">
              {Enc(heading)}
            </td>
          </tr>
          <tr>
            <td style=""padding:24px 28px;font-size:14px;line-height:1.5;"">
              <p style=""margin:0 0 18px;"">{Enc(intro)}</p>
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""font-size:14px;"">
                {Row("Ticket Number", Enc(model.TicketNumber))}
                {Row("Title", Enc(model.Title))}
                {Row("Status", Enc(model.Status))}
                {Row("Priority", Enc(model.Priority))}
                {Row("Assigned Engineer", Enc(model.AssignedEngineer))}
                {Row("Requester", Enc(model.Requester))}
                {Row("Action Performed", Enc(action))}
                {Row("Timestamp", model.Timestamp.ToString("yyyy-MM-dd HH:mm 'UTC'"))}
              </table>
            </td>
          </tr>
          <tr>
            <td style=""padding:16px 28px;background:#f4f5f7;color:#5e6c84;font-size:12px;"">
              This is an automated message from the Helpdesk system.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }

        private static string Row(string label, string value) =>
            $@"<tr>
                 <td style=""padding:6px 0;color:#5e6c84;width:180px;vertical-align:top;"">{label}</td>
                 <td style=""padding:6px 0;font-weight:600;"">{value}</td>
               </tr>";
    }
}
