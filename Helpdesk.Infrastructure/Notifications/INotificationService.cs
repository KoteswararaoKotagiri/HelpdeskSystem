using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Infrastructure.Notifications
{
    public interface INotificationService
    {
        Task SendTicketAssigned(
            Guid userId,
            object payload);

        Task SendTicketUpdated(
            Guid userId,
            object payload);

        Task SendCommentAdded(
            Guid userId,
            object payload);
    }
}
