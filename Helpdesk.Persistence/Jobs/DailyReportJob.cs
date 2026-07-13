using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    // Produces a daily operations report (ticket totals, SLA compliance, engineer performance)
    // and emails it to administrators.
    public class DailyReportJob : IDailyReportJob
    {
        private readonly HelpdeskDbContext _context;
        private readonly ISlaService _slaService;
        private readonly IEmailSender _emailSender;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<DailyReportJob> _logger;

        public DailyReportJob(
            HelpdeskDbContext context,
            ISlaService slaService,
            IEmailSender emailSender,
            IOptions<EmailSettings> emailSettings,
            ILogger<DailyReportJob> logger)
        {
            _context = context;
            _slaService = slaService;
            _emailSender = emailSender;
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var total = await _context.Tickets.CountAsync();
            var closed = await _context.Tickets.CountAsync(t => t.Status.IsClosed);
            var open = total - closed;
            var performance = await _slaService.GetPerformanceAsync();

            var engineerStats = await _context.Tickets
                .Where(t => t.Status.IsClosed && t.AssignedToUserId != null)
                .GroupBy(t => t.AssignedToUserId!.Value)
                .Select(g => new { UserId = g.Key, Resolved = g.Count() })
                .ToListAsync();

            var engineerIds = engineerStats.Select(e => e.UserId).ToList();
            var names = await _context.Users
                .Where(u => engineerIds.Contains(u.Id))
                .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName })
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            _logger.LogInformation(
                "Daily report: total={Total}, open={Open}, closed={Closed}, SLA compliance={Compliance}%.",
                total, open, closed, performance.CompliancePercent);

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

            var engineerRows = string.Join(string.Empty, engineerStats
                .OrderByDescending(e => e.Resolved)
                .Select(e =>
                {
                    var name = names.TryGetValue(e.UserId, out var n) ? n : "Unknown";
                    return $"<tr><td>{WebUtility.HtmlEncode(name)}</td><td>{e.Resolved}</td></tr>";
                }));

            var html =
                "<h3>Daily Helpdesk Report</h3>" +
                $"<p>Total: {total} &nbsp; Open: {open} &nbsp; Closed: {closed}</p>" +
                $"<p>SLA compliance: {performance.CompliancePercent}% " +
                $"(met {performance.Met}, breached {performance.Breached}, overdue {performance.Overdue})</p>" +
                "<h4>Engineer performance (resolved)</h4>" +
                "<table border='1' cellpadding='6' cellspacing='0'>" +
                "<tr><th>Engineer</th><th>Resolved</th></tr>" +
                (engineerRows.Length == 0 ? "<tr><td colspan='2'>No data</td></tr>" : engineerRows) +
                "</table>";

            foreach (var email in adminEmails)
            {
                try
                {
                    await _emailSender.SendAsync(new EmailMessage
                    {
                        To = email,
                        Subject = "Daily Helpdesk Report",
                        HtmlBody = html
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send daily report to {Email}.", email);
                }
            }
        }
    }
}
