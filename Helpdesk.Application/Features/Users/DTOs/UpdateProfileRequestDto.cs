using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Application.Features.Users.DTOs
{
    // Personal information only. Role and department are intentionally excluded — those are admin-only.
    public class UpdateProfileRequestDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
