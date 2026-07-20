using Microsoft.AspNetCore.SignalR;
using Helpdesk.Infrastructure.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Helpdesk.Infrastructure.Notifications
{
    public class NotificationService
     : INotificationService
    {
        private readonly IHubContext<NotificationHub>
            _hubContext;

        public NotificationService(
            IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendTicketAssigned(
            Guid userId,
            object payload)
        {
            await _hubContext
                .Clients
                .User(userId.ToString())
                .SendAsync(
                    "TicketAssigned",
                    payload);
        }

        public async Task SendTicketUpdated(
            Guid userId,
            object payload)
        {
            await _hubContext
                .Clients
                .User(userId.ToString())
                .SendAsync(
                    "TicketUpdated",
                    payload);
        }

        public async Task SendCommentAdded(
            Guid userId,
            object payload)
        {
            await _hubContext
                .Clients
                .User(userId.ToString())
                .SendAsync(
                    "CommentAdded",
                    payload);
        }
    }
}
