using Helpdesk.Domain.Common;

namespace Helpdesk.Domain.Entities.Masters
{
    public class TicketStatus : BaseEntity
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public string Color { get; set; }

        public int Sequence { get; set; }

        public bool IsClosed { get; set; }
    }
}
