using System;

namespace Helpdesk.Application.Features.Sla.DTOs
{
    public class SlaTicketDto
    {
        public Guid TicketId { get; set; }

        public string TicketNumber { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string Assignee { get; set; } = "Unassigned";

        public DateTime ResolutionDueAt { get; set; }

        public double RemainingMinutes { get; set; }

        public string SlaStatus { get; set; } = string.Empty;

        public bool IsOverdue { get; set; }

        public int EscalationLevel { get; set; }
    }
}
