using System;

namespace Helpdesk.Application.Features.Emails
{
    // Data shared by every ticket email template.
    public class TicketEmailModel
    {
        public string TicketNumber { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string AssignedEngineer { get; set; } = "Unassigned";

        public string Requester { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }
    }
}
