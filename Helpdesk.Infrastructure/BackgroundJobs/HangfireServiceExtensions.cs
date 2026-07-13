using System;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Helpdesk.Application.Configuration;
using Helpdesk.Application.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Helpdesk.Infrastructure.BackgroundJobs
{
    // Isolates all Hangfire framework wiring (storage, server, dashboard, retry, scheduling)
    // in the Infrastructure layer. Job business logic lives behind Application interfaces.
    public static class HangfireServiceExtensions
    {
        public static IServiceCollection AddHangfireBackgroundJobs(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<HangfireSettings>(configuration.GetSection("Hangfire"));

            var settings = configuration.GetSection("Hangfire").Get<HangfireSettings>() ?? new HangfireSettings();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

            services.AddHangfireServer(options =>
            {
                options.Queues = settings.Queues is { Length: > 0 } ? settings.Queues : new[] { "default" };
                if (settings.WorkerCount > 0)
                {
                    options.WorkerCount = settings.WorkerCount;
                }
            });

            // Global retry policy for every job.
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = settings.RetryCount });

            return services;
        }

        public static IApplicationBuilder UseHangfireBackgroundJobs(
            this IApplicationBuilder app, IConfiguration configuration, bool allowAnonymousDashboard)
        {
            var settings = configuration.GetSection("Hangfire").Get<HangfireSettings>() ?? new HangfireSettings();

            app.UseHangfireDashboard(settings.DashboardPath, new DashboardOptions
            {
                Authorization = new[] { new HangfireDashboardAuthorizationFilter(allowAnonymousDashboard) }
            });

            ScheduleRecurringJobs(settings);

            return app;
        }

        private static void ScheduleRecurringJobs(HangfireSettings settings)
        {
            try
            {
                RecurringJob.AddOrUpdate<ISlaMonitoringJob>(
                    "sla-monitoring", job => job.ExecuteAsync(), settings.Schedules.SlaMonitoring);

                RecurringJob.AddOrUpdate<IReminderEmailJob>(
                    "reminder-emails", job => job.ExecuteAsync(), settings.Schedules.ReminderEmails);

                RecurringJob.AddOrUpdate<IDailyReportJob>(
                    "daily-report", job => job.ExecuteAsync(), settings.Schedules.DailyReport);

                RecurringJob.AddOrUpdate<ICleanupJob>(
                    "cleanup", job => job.ExecuteAsync(), settings.Schedules.Cleanup);

                RecurringJob.AddOrUpdate<IScheduledNotificationJob>(
                    "scheduled-notifications", job => job.ExecuteAsync(), settings.Schedules.ScheduledNotifications);
            }
            catch (Exception)
            {
                // Storage not reachable yet (e.g. database still starting) — recurring jobs
                // register on the next successful startup. Never block application start.
            }
        }
    }
}
