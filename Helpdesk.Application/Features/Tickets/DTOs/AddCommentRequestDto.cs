using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Application.Features.Tickets.DTOs
{
    public class AddCommentRequestDto
    {
        public string Comment { get; set; }

        public bool IsInternal { get; set; }
    }
}
