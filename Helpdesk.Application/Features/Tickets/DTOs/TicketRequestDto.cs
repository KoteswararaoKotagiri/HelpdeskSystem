using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Application.Features.Tickets.DTOs
{
    public class TicketRequestDto
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public Guid PriorityId { get; set; }

        public Guid CategoryId { get; set; }
    }
}
