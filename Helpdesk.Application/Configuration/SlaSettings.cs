using System.Collections.Generic;

namespace Helpdesk.Application.Configuration
{
    // SLA policies are configuration-driven (the schema has no SLA-policy table). The engine
    // resolves the most specific policy for a ticket and falls back to the priority's SlaHours.
    public class SlaSettings
    {
        // Percent of the SLA window elapsed at which a ticket becomes "At Risk".
        public int WarningThresholdPercent { get; set; } = 80;

        // Window (days) used by SLA performance/compliance metrics.
        public int PerformanceWindowDays { get; set; } = 30;

        public WorkingHoursSettings WorkingHours { get; set; } = new();

        public SlaPolicySettings Default { get; set; } = new();

        public List<SlaPolicySettings> Policies { get; set; } = new();

        public List<EscalationLevelSettings> Escalations { get; set; } = new();
    }

    public class SlaPolicySettings
    {
        // Optional match keys. A policy matches a ticket when every non-empty key matches;
        // the most specific matching policy (category > department > priority) wins.
        public string? PriorityCode { get; set; }

        public string? DepartmentName { get; set; }

        public string? CategoryName { get; set; }

        public int ResponseMinutes { get; set; }

        // Resolution SLA in hours. When 0, the engine falls back to the priority's SlaHours.
        public int ResolutionHours { get; set; }

        public bool UseBusinessHours { get; set; }
    }

    public class WorkingHoursSettings
    {
        public int StartHour { get; set; } = 9;

        public int EndHour { get; set; } = 17;

        // 0 = Sunday .. 6 = Saturday. Default: Monday–Friday.
        public int[] WorkingDays { get; set; } = { 1, 2, 3, 4, 5 };
    }

    public class EscalationLevelSettings
    {
        public int Level { get; set; }

        // Escalate to this level once the elapsed SLA percentage reaches this value (100 = breached).
        public int TriggerAtPercent { get; set; }

        // "AssignedEngineer", "TeamLead", or "Administrator".
        public string NotifyRole { get; set; } = "AssignedEngineer";
    }
}
