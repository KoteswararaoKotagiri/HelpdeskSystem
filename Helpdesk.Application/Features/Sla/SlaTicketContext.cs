using System;

namespace Helpdesk.Application.Features.Sla
{
    // Minimal input the SLA calculator needs. Kept free of EF types so the engine stays pure.
    public class SlaTicketContext
    {
        public DateTime CreatedOn { get; set; }

        public string PriorityCode { get; set; } = string.Empty;

        public int PrioritySlaHours { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public bool IsClosed { get; set; }

        // When the ticket reached a closed status (proxied by ModifiedOn). Null while still open.
        public DateTime? ClosedAt { get; set; }
    }
}
