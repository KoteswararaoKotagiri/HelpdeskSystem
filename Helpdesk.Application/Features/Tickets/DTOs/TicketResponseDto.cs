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

        public string Description { get; set; }

        public string StatusName { get; set; }

        public string PriorityName { get; set; }

        public string CategoryName { get; set; }

        public string RequesterName { get; set; }

        public string AssigneeName { get; set; }

        public int CommentCount { get; set; }

        public int AttachmentCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
