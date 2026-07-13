using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Application.Features.Auth.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; }

        public DateTime ExpiresAt { get; set; }

        public Guid UserId { get; set; }

        public string Email { get; set; }

        public string FullName { get; set; }

        public string Role { get; set; }
    }
}
