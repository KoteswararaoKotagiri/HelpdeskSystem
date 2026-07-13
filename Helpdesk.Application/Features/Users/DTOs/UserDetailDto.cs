using System;

namespace Helpdesk.Application.Features.Users.DTOs
{
    public class UserDetailDto
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public Guid RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public Guid DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? LastUpdated { get; set; }

        // The current schema has no last-login column, so this stays null until the field is tracked.
        public DateTime? LastLoginAt { get; set; }
    }
}
