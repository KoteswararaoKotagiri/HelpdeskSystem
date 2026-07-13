using System;

namespace Helpdesk.Application.Features.Dashboard.DTOs
{
    public class RecentTicketDto
    {
        public string TicketNumber { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string Requester { get; set; } = string.Empty;

        public string? Assignee { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
