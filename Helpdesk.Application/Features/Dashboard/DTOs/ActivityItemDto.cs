using System;

namespace Helpdesk.Application.Features.Dashboard.DTOs
{
    public class ActivityItemDto
    {
        public string ActivityType { get; set; } = string.Empty;

        public string TicketNumber { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }
    }
}
