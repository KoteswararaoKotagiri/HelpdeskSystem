using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Application.Features.Auth.DTOs
{
    public class RegisterRequestDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public Guid RoleId { get; set; }

        public Guid DepartmentId { get; set; }
    }
}
