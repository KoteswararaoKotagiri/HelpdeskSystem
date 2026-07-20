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
    // Sends reminder digests to engineers for their open/assigned work, flagging overdue
    // and SLA-warning (near-breach) tickets.
    public class ReminderEmailJob : IReminderEmailJob
    {
        private readonly HelpdeskDbContext _context;
        private readonly ISlaService _slaService;
        private readonly IEmailSender _emailSender;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<ReminderEmailJob> _logger;

        public ReminderEmailJob(
            HelpdeskDbContext context,
            ISlaService slaService,
            IEmailSender emailSender,
            IOptions<EmailSettings> emailSettings,
            ILogger<ReminderEmailJob> logger)
        {
            _context = context;
            _slaService = slaService;
            _emailSender = emailSender;
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var openTickets = await _context.Tickets
                .Where(t => !t.Status.IsClosed)
                .Select(t => new OpenTicket
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    Title = t.Title,
                    StatusName = t.Status.Name,
                    AssignedToUserId = t.AssignedToUserId
                })
                .ToListAsync();

            var pending = openTickets.Count(t => t.AssignedToUserId == null);
            var assigned = openTickets.Count(t => t.AssignedToUserId != null);
            var overdue = await _slaService.GetOverdueAsync();
            var near = await _slaService.GetNearBreachAsync();

            _logger.LogInformation(
                "Reminders: {Pending} pending, {Assigned} assigned, {Overdue} overdue, {Warning} SLA warning(s).",
                pending, assigned, overdue.Count, near.Count);

            if (!_emailSettings.Enabled)
            {
                return;
            }

            var overdueIds = overdue.Select(o => o.TicketId).ToHashSet();
            var nearIds = near.Select(n => n.TicketId).ToHashSet();

            foreach (var group in openTickets
                         .Where(t => t.AssignedToUserId != null)
                         .GroupBy(t => t.AssignedToUserId!.Value))
            {
                var email = await _context.Users
                    .Where(u => u.Id == group.Key && u.IsActive)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(email))
                {
                    continue;
                }

                await SendReminderAsync(email, group.ToList(), overdueIds, nearIds);
            }
        }

        private async Task SendReminderAsync(
            string email, List<OpenTicket> tickets, HashSet<Guid> overdueIds, HashSet<Guid> nearIds)
        {
            var rows = string.Join(string.Empty, tickets.Select(t =>
            {
                var flag = overdueIds.Contains(t.Id) ? "Overdue"
                    : nearIds.Contains(t.Id) ? "SLA Warning"
                    : t.StatusName;
                return $"<tr><td>{WebUtility.HtmlEncode(t.TicketNumber)}</td>" +
                       $"<td>{WebUtility.HtmlEncode(t.Title)}</td>" +
                       $"<td>{WebUtility.HtmlEncode(flag)}</td></tr>";
            }));

            var html =
                $"<h3>You have {tickets.Count} open ticket(s)</h3>" +
                "<table border='1' cellpadding='6' cellspacing='0'>" +
                "<tr><th>Ticket</th><th>Title</th><th>Status</th></tr>" + rows + "</table>";

            try
            {
                await _emailSender.SendAsync(new EmailMessage
                {
                    To = email,
                    Subject = $"Reminder: {tickets.Count} open ticket(s) assigned to you",
                    HtmlBody = html
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reminder email to {Email}.", email);
            }
        }

        private sealed class OpenTicket
        {
            public Guid Id { get; set; }
            public string TicketNumber { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string StatusName { get; set; } = string.Empty;
            public Guid? AssignedToUserId { get; set; }
        }
    }
}
