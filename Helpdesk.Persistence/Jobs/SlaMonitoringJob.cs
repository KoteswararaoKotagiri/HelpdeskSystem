using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Helpdesk.Application.Configuration;
using Helpdesk.Application.Features.Emails;
using Helpdesk.Application.Features.Sla.DTOs;
using Helpdesk.Application.Interfaces;
using Helpdesk.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Helpdesk.Persistence.Jobs
{
    // Periodically detects overdue / near-breach tickets, persists their SLA due dates
    // (existing Ticket.DueDate column) and escalates overdue tickets to administrators.
    public class SlaMonitoringJob : ISlaMonitoringJob
    {
        private readonly HelpdeskDbContext _context;
        private readonly ISlaService _slaService;
        private readonly IEmailSender _emailSender;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<SlaMonitoringJob> _logger;

        public SlaMonitoringJob(
            HelpdeskDbContext context,
            ISlaService slaService,
            IEmailSender emailSender,
            IOptions<EmailSettings> emailSettings,
            ILogger<SlaMonitoringJob> logger)
        {
            _context = context;
            _slaService = slaService;
            _emailSender = emailSender;
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var overdue = await _slaService.GetOverdueAsync();
            var near = await _slaService.GetNearBreachAsync();

            _logger.LogInformation(
                "SLA monitoring: {Overdue} overdue, {Near} near-breach ticket(s).",
                overdue.Count, near.Count);

            await PersistDueDatesAsync(overdue.Concat(near));

            if (_emailSettings.Enabled && overdue.Count > 0)
            {
                await EscalateToAdminsAsync(overdue);
            }
        }

        private async Task PersistDueDatesAsync(IEnumerable<SlaTicketDto> tickets)
        {
            var dueById = tickets
                .GroupBy(t => t.TicketId)
                .ToDictionary(g => g.Key, g => g.First().ResolutionDueAt);

            if (dueById.Count == 0)
            {
                return;
            }

            var ids = dueById.Keys.ToList();
            var entities = await _context.Tickets.Where(t => ids.Contains(t.Id)).ToListAsync();

            var changed = false;
            foreach (var ticket in entities)
            {
                if (dueById.TryGetValue(ticket.Id, out var due) && ticket.DueDate != due)
                {
                    ticket.DueDate = due;
                    changed = true;
                }
            }

            if (changed)
            {
                await _context.SaveChangesAsync();
            }
        }

        private async Task EscalateToAdminsAsync(IReadOnlyList<SlaTicketDto> overdue)
        {
            var adminEmails = await _context.Users
                .Where(u => u.IsActive && u.Role.Name == "Admin")
                .Select(u => u.Email)
                .ToListAsync();

            if (adminEmails.Count == 0)
            {
                return;
            }

            var rows = string.Join(string.Empty, overdue.Select(t =>
                $"<tr><td>{WebUtility.HtmlEncode(t.TicketNumber)}</td>" +
                $"<td>{WebUtility.HtmlEncode(t.Title)}</td>" +
                $"<td>{WebUtility.HtmlEncode(t.Priority)}</td>" +
                $"<td>Level {t.EscalationLevel}</td></tr>"));

            var html =
                $"<h3>SLA Escalation — {overdue.Count} overdue ticket(s)</h3>" +
                "<table border='1' cellpadding='6' cellspacing='0'>" +
                "<tr><th>Ticket</th><th>Title</th><th>Priority</th><th>Escalation</th></tr>" +
                rows + "</table>";

            foreach (var email in adminEmails)
            {
                try
                {
                    await _emailSender.SendAsync(new EmailMessage
                    {
                        To = email,
                        Subject = $"SLA Escalation — {overdue.Count} overdue ticket(s)",
                        HtmlBody = html
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send SLA escalation email to {Email}.", email);
                }
            }
        }
    }
}
