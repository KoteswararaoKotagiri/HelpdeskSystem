using System.Threading.Tasks;

namespace Helpdesk.Application.Interfaces
{
    // Background job contracts. Implemented as ordinary services (in Persistence) and invoked by
    // Hangfire recurring jobs. No Hangfire types leak into the Application layer.

    public interface ISlaMonitoringJob
    {
        Task ExecuteAsync();
    }

    public interface IReminderEmailJob
    {
        Task ExecuteAsync();
    }

    public interface IDailyReportJob
    {
        Task ExecuteAsync();
    }

    public interface ICleanupJob
    {
        Task ExecuteAsync();
    }

    public interface IScheduledNotificationJob
    {
        Task ExecuteAsync();
    }
}
