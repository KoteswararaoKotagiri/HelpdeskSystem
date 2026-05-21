using Helpdesk.Domain.Common;

namespace Helpdesk.Domain.Entities.Masters
{
    public class Department : BaseEntity
    {
        public string Name { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
