using Helpdesk.Domain.Common;
using Helpdesk.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Domain.Entities.Tickets
{
    public class TicketComment : BaseEntity
    {
        public Guid TicketId { get; set; }

        public Ticket Ticket { get; set; }

        public Guid UserId { get; set; }

        public User User { get; set; }

        public string Comment { get; set; }

        // INTERNAL = engineer/admin only
        // PUBLIC = visible to employee

        public bool IsInternal { get; set; }
    }
}
