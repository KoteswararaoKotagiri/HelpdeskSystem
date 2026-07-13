using System;

namespace Helpdesk.Application.Features.Users.DTOs
{
    public class UserListQueryDto
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }

        public Guid? RoleId { get; set; }

        public Guid? DepartmentId { get; set; }

        public bool? IsActive { get; set; }

        // "name" or "createdDate" (defaults to newest first).
        public string? SortBy { get; set; }

        public bool SortDescending { get; set; }
    }
}
