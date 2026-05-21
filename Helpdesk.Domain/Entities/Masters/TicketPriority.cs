using Helpdesk.Domain.Common;

namespace Helpdesk.Domain.Entities.Masters
{
    public class TicketPriority : BaseEntity
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public int SlaHours { get; set; }

        public int SortOrder { get; set; }
    }
}
