using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Application.Features.Tickets.DTOs
{
    public class TicketCommentResponseDto
    {
        public Guid Id { get; set; }

        public Guid TicketId { get; set; }

        public string Body { get; set; }

        public string AuthorName { get; set; }

        public string AuthorRole { get; set; }

        public bool IsInternal { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
