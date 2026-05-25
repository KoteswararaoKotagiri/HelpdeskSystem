using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Application.Features.Tickets.DTOs
{
    public class TicketAttachmentResponseDto
    {
        public Guid Id { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public long FileSize { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
