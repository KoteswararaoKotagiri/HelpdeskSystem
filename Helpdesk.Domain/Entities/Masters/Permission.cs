using Helpdesk.Domain.Common;

namespace Helpdesk.Domain.Entities.Masters
{
    public class Permission : BaseEntity
    {
        public string Name { get; set; }

        public string? Description { get; set; }

        public string? Claim { get; set; }
    }
}
