using Helpdesk.Domain.Common;
using Helpdesk.Domain.Entities.Masters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Domain.Entities.Tickets
{
    public class Ticket : BaseEntity
    {
        public string TicketNumber { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public Guid StatusId { get; set; }

        public Guid PriorityId { get; set; }

        public Guid CategoryId { get; set; }

        public Guid CreatedByUserId { get; set; }

        public Guid? AssignedToUserId { get; set; }

        public DateTime? DueDate { get; set; }

        public TicketStatus Status { get; set; }

        public TicketPriority Priority { get; set; }

        public TicketCategory Category { get; set; }
        public ICollection<TicketComment> Comments { get; set; }
        public ICollection<TicketAttachment> Attachments { get; set; }
    }
}
