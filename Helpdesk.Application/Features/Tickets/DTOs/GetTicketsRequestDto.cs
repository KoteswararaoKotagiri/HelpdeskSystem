using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Application.Features.Tickets.DTOs
{
    public class GetTicketsRequestDto
    {
        public string? Search { get; set; }

        public Guid? StatusId { get; set; }

        public Guid? PriorityId { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
