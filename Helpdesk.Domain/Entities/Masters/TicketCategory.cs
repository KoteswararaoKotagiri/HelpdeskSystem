using Helpdesk.Domain.Common;

namespace Helpdesk.Domain.Entities.Masters
{
    public class TicketCategory : BaseEntity
    {
        public string Name { get; set; }

        public string? Description { get; set; }
    }
}
