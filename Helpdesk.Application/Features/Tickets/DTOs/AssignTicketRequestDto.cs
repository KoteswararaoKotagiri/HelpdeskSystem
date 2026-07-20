using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Application.Features.Tickets.DTOs
{
    public class AssignTicketRequestDto
    {
        public Guid AssignedToUserId { get; set; }
    }
}
