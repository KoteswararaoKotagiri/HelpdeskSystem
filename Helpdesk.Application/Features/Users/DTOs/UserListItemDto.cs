using System;

namespace Helpdesk.Application.Features.Users.DTOs
{
    public class UserListItemDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
