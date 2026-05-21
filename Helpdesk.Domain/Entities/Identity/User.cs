using Helpdesk.Domain.Common;
using Helpdesk.Domain.Entities.Masters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Domain.Entities.Identity
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public Guid RoleId { get; set; }

        public Guid DepartmentId { get; set; }

        public bool IsActive { get; set; }

        public Role Role { get; set; }

        public Department Department { get; set; }
    }
}
