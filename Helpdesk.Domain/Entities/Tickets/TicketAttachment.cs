using Helpdesk.Domain.Common;
using Helpdesk.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Domain.Entities.Tickets
{
    public class TicketAttachment : BaseEntity
    {
        public Guid TicketId { get; set; }

        public Ticket Ticket { get; set; }

        public string OriginalFileName { get; set; }

        public string StoredFileName { get; set; }

        public string FilePath { get; set; }

        public string ContentType { get; set; }

        public long FileSize { get; set; }

        public Guid UploadedByUserId { get; set; }

        public User UploadedByUser { get; set; }
    }
}
