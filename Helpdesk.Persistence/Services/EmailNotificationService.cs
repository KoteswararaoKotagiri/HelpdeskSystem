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

namespace Helpdesk.Persistence.Services
{
    // Orchestrates ticket email notifications: loads the ticket context, renders the matching
    // template and dispatches through the configured IEmailSender. Disabled by default and fully
    // guarded so a missing/failed provider never breaks the originating request.
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly HelpdeskDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(
            HelpdeskDbContext context,
            IEmailSender emailSender,
            IOptions<EmailSettings> settings,
            ILogger<EmailNotificationService> logger)
        {
            _context = context;
            _emailSender = emailSender;
            _settings = settings.Value;
            _logger = logger;
        }

        public Task NotifyTicketCreatedAsync(Guid ticketId) =>
            DispatchAsync(ticketId, EmailTemplateType.TicketCreated, ctx => ctx.RequesterEmail);

        public Task NotifyTicketAssignedAsync(Guid ticketId) =>
            DispatchAsync(ticketId, EmailTemplateType.TicketAssigned, ctx => ctx.AssigneeEmail);

        public Task NotifyStatusChangedAsync(Guid ticketId) =>
            DispatchAsync(
                ticketId,
                // A closed status uses the dedicated "Ticket Closed" template.
                null,
                ctx => ctx.RequesterEmail);

        public Task NotifyCommentAddedAsync(Guid ticketId) =>
            DispatchAsync(ticketId, EmailTemplateType.CommentAdded, ctx => ctx.RequesterEmail);

        // templateType == null means "status changed": resolved to Updated or Closed from the ticket state.
        private async Task DispatchAsync(
            Guid ticketId,
            EmailTemplateType? templateType,
            Func<TicketEmailContext, string?> recipientSelector)
        {
            if (!_settings.Enabled)
            {
                return;
            }

            try
            {
                var context = await LoadContextAsync(ticketId);
                if (context is null)
                {
                    _logger.LogWarning("Email skipped: ticket {TicketId} not found.", ticketId);
                    return;
                }

                var recipient = recipientSelector(context);
                if (string.IsNullOrWhiteSpace(recipient))
                {
                    _logger.LogInformation(
                        "Email skipped for ticket {TicketNumber}: no recipient address.",
                        context.Model.TicketNumber);
                    return;
                }

                var type = templateType
                    ?? (context.StatusIsClosed ? EmailTemplateType.TicketClosed : EmailTemplateType.TicketUpdated);

                var message = EmailTemplates.Build(type, context.Model);
                message.To = recipient;

                await _emailSender.SendAsync(message);
            }
            catch (Exception ex)
            {
                // Notifications are best-effort; never propagate to the originating request.
                _logger.LogError(ex,
                    "Failed to send {TemplateType} email for ticket {TicketId}.",
                    templateType, ticketId);
            }
        }

        private async Task<TicketEmailContext?> LoadContextAsync(Guid ticketId)
        {
            var ticket = await _context.Tickets
                .Where(t => t.Id == ticketId)
                .Select(t => new
                {
                    t.TicketNumber,
                    t.Title,
                    StatusName = t.Status.Name,
                    StatusIsClosed = t.Status.IsClosed,
                    PriorityName = t.Priority.Name,
                    t.CreatedByUserId,
                    t.AssignedToUserId
                })
                .FirstOrDefaultAsync();

            if (ticket is null)
            {
                return null;
            }

            var requester = await _context.Users
                .Where(u => u.Id == ticket.CreatedByUserId)
                .Select(u => new { u.Email, FullName = u.FirstName + " " + u.LastName })
                .FirstOrDefaultAsync();

            var assignee = ticket.AssignedToUserId.HasValue
                ? await _context.Users
                    .Where(u => u.Id == ticket.AssignedToUserId.Value)
                    .Select(u => new { u.Email, FullName = u.FirstName + " " + u.LastName })
                    .FirstOrDefaultAsync()
                : null;

            var model = new TicketEmailModel
            {
                TicketNumber = ticket.TicketNumber,
                Title = ticket.Title,
                Status = ticket.StatusName,
                Priority = ticket.PriorityName,
                AssignedEngineer = assignee?.FullName ?? "Unassigned",
                Requester = requester?.FullName ?? string.Empty,
                Timestamp = DateTime.UtcNow
            };

            return new TicketEmailContext(
                model,
                requester?.Email ?? string.Empty,
                assignee?.Email,
                ticket.StatusIsClosed);
        }

        private sealed record TicketEmailContext(
            TicketEmailModel Model,
            string RequesterEmail,
            string? AssigneeEmail,
            bool StatusIsClosed);
    }
}
