using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Domain.Common
{
    public class AuditLog : BaseEntity
    {
        public string EntityName { get; set; }

        public Guid EntityId { get; set; }

        public string Action { get; set; }

        public string Changes { get; set; }

        public Guid PerformedByUserId { get; set; }
    }
}
