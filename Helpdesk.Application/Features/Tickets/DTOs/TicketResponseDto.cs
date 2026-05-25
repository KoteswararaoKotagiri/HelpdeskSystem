using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Application.Features.Tickets.DTOs
{
    public class TicketResponseDto
    {
        public Guid Id { get; set; }

        public string TicketNumber { get; set; }

        public string Title { get; set; }

        public string Status { get; set; }

        public string Priority { get; set; }

        public string Category { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
