using System;
using System.Linq;
using System.Threading.Tasks;
using Helpdesk.Application.Configuration;
using Helpdesk.Application.Features.Emails;
using Helpdesk.Application.Interfaces;
using Helpdesk.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Helpdesk.Persistence.Jobs
{
    // Emits scheduled (weekly) summary notifications: SLA compliance, breaches and warnings.
    public class ScheduledNotificationJob : IScheduledNotificationJob
    {
        private readonly HelpdeskDbContext _context;
        private readonly ISlaService _slaService;
        private readonly IEmailSender _emailSender;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<ScheduledNotificationJob> _logger;

        public ScheduledNotificationJob(
            HelpdeskDbContext context,
            ISlaService slaService,
            IEmailSender emailSender,
            IOptions<EmailSettings> emailSettings,
            ILogger<ScheduledNotificationJob> logger)
        {
            _context = context;
            _slaService = slaService;
            _emailSender = emailSender;
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var performance = await _slaService.GetPerformanceAsync();
            var breaches = await _slaService.GetBreachesAsync();
            var near = await _slaService.GetNearBreachAsync();

            _logger.LogInformation(
                "Scheduled summary: compliance={Compliance}%, breaches={Breaches}, warnings={Warnings}.",
                performance.CompliancePercent, breaches.Count, near.Count);

            if (!_emailSettings.Enabled)
            {
                return;
            }

            var adminEmails = await _context.Users
                .Where(u => u.IsActive && u.Role.Name == "Admin")
                .Select(u => u.Email)
                .ToListAsync();

            if (adminEmails.Count == 0)
            {
                return;
            }

            var html =
                "<h3>Weekly Helpdesk Summary</h3>" +
                $"<p>SLA compliance: {performance.CompliancePercent}% over the last {performance.WindowDays} day(s).</p>" +
                $"<p>Current breaches: {breaches.Count} &nbsp; SLA warnings: {near.Count} &nbsp; " +
                $"Overdue: {performance.Overdue}</p>";

            foreach (var email in adminEmails)
            {
                try
                {
                    await _emailSender.SendAsync(new EmailMessage
                    {
                        To = email,
                        Subject = "Weekly Helpdesk Summary",
                        HtmlBody = html
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send weekly summary to {Email}.", email);
                }
            }
        }
    }
}
