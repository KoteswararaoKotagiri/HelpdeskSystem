using System;
using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Application.Features.Users.DTOs
{
    public class ChangeRoleRequestDto
    {
        [Required]
        public Guid RoleId { get; set; }
    }
}
