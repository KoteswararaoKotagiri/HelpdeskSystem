using System;

namespace Helpdesk.Application.Features.Sla.DTOs
{
    public class SlaInfoDto
    {
        public DateTime StartTime { get; set; }

        public DateTime? ResponseDueAt { get; set; }

        public DateTime ResolutionDueAt { get; set; }

        public double RemainingMinutes { get; set; }

        public double ElapsedPercent { get; set; }

        public string Status { get; set; } = string.Empty;

        public bool IsOverdue { get; set; }

        public int EscalationLevel { get; set; }
    }
}
