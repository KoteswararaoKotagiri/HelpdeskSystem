using System;
using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Application.Features.Users.DTOs
{
    public class ChangeDepartmentRequestDto
    {
        [Required]
        public Guid DepartmentId { get; set; }
    }
}
