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

        public string UserName { get; set; }

        public string Comment { get; set; }

        public bool IsInternal { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
