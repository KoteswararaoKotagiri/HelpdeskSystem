using System;
using System.Threading.Tasks;

namespace Helpdesk.Application.Interfaces
{
    // High-level ticket email notifications. Each method loads the ticket context, renders the
    // matching template and dispatches it. Implementations must never throw to the caller — a
    // failed/disabled email must not break the originating request.
    public interface IEmailNotificationService
    {
        Task NotifyTicketCreatedAsync(Guid ticketId);

        Task NotifyTicketAssignedAsync(Guid ticketId);

        // Detects a closed status and uses the "Ticket Closed" template automatically.
        Task NotifyStatusChangedAsync(Guid ticketId);

        Task NotifyCommentAddedAsync(Guid ticketId);
    }
}
