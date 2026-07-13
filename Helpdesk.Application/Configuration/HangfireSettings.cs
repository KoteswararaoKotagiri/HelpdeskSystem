namespace Helpdesk.Application.Configuration
{
    // Bound from the "Hangfire" configuration section. Schedules, retry, queues and cleanup
    // retention are all configuration-driven.
    public class HangfireSettings
    {
        public string DashboardPath { get; set; } = "/hangfire";

        // 0 = Hangfire default (based on processor count).
        public int WorkerCount { get; set; }

        public string[] Queues { get; set; } = { "default" };

        public int RetryCount { get; set; } = 3;

        public JobScheduleSettings Schedules { get; set; } = new();

        public CleanupSettings Cleanup { get; set; } = new();
    }

    // Cron expressions per recurring job.
    public class JobScheduleSettings
    {
        public string SlaMonitoring { get; set; } = "*/15 * * * *";

        public string ReminderEmails { get; set; } = "0 */4 * * *";

        public string DailyReport { get; set; } = "0 7 * * *";

        public string Cleanup { get; set; } = "0 2 * * *";

        public string ScheduledNotifications { get; set; } = "0 8 * * 1";
    }

    public class CleanupSettings
    {
        public int LogRetentionDays { get; set; } = 90;

        public int OrphanFileRetentionDays { get; set; } = 7;

        // Relative to the application's working directory.
        public string UploadsPath { get; set; } = "wwwroot/uploads/tickets";
    }
}
