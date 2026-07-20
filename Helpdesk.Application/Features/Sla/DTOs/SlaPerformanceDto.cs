namespace Helpdesk.Application.Features.Sla.DTOs
{
    public class SlaPerformanceDto
    {
        public int TotalEvaluated { get; set; }

        public int Met { get; set; }

        public int Breached { get; set; }

        public int AtRisk { get; set; }

        public int OnTrack { get; set; }

        public int Overdue { get; set; }

        public double CompliancePercent { get; set; }

        public int WindowDays { get; set; }
    }
}
