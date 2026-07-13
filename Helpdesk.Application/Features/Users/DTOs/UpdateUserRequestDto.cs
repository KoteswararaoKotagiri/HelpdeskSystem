using System;
using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Application.Features.Users.DTOs
{
    public class UpdateUserRequestDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public Guid DepartmentId { get; set; }

        public bool IsActive { get; set; }
    }
}
