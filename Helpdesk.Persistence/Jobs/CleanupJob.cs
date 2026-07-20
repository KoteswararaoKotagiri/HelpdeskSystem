using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Helpdesk.Application.Configuration;
using Helpdesk.Application.Interfaces;
using Helpdesk.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Helpdesk.Persistence.Jobs
{
    // Removes expired audit logs and orphaned upload files. Retention is configuration-driven.
    public class CleanupJob : ICleanupJob
    {
        private readonly HelpdeskDbContext _context;
        private readonly CleanupSettings _settings;
        private readonly ILogger<CleanupJob> _logger;

        public CleanupJob(
            HelpdeskDbContext context,
            IOptions<HangfireSettings> settings,
            ILogger<CleanupJob> logger)
        {
            _context = context;
            _settings = settings.Value.Cleanup;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            await CleanExpiredAuditLogsAsync();
            await CleanOrphanedUploadsAsync();
        }

        private async Task CleanExpiredAuditLogsAsync()
        {
            var cutoff = DateTime.UtcNow.AddDays(-_settings.LogRetentionDays);

            // Set-based delete — no entities loaded into memory.
            var deleted = await _context.AuditLogs
                .Where(a => a.CreatedOn < cutoff)
                .ExecuteDeleteAsync();

            _logger.LogInformation(
                "Cleanup: removed {Count} audit log(s) older than {Days} day(s).",
                deleted, _settings.LogRetentionDays);
        }

        private async Task CleanOrphanedUploadsAsync()
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), _settings.UploadsPath);
            if (!Directory.Exists(uploadsPath))
            {
                return;
            }

            var known = (await _context.TicketAttachments
                    .Select(a => a.StoredFileName)
                    .ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var cutoff = DateTime.UtcNow.AddDays(-_settings.OrphanFileRetentionDays);
            var removed = 0;

            foreach (var file in Directory.GetFiles(uploadsPath))
            {
                var name = Path.GetFileName(file);
                if (known.Contains(name) || File.GetLastWriteTimeUtc(file) >= cutoff)
                {
                    continue;
                }

                try
                {
                    File.Delete(file);
                    removed++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Cleanup: could not delete orphan file {File}.", name);
                }
            }

            _logger.LogInformation("Cleanup: removed {Count} orphaned upload file(s).", removed);
        }
    }
}
