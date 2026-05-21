using System.Collections.Generic;
using Helpdesk.Domain.Common;

namespace Helpdesk.Domain.Entities.Masters
{
    public class Role : BaseEntity
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public bool IsActive { get; set; }
    }
}
