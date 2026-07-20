using System;

namespace Helpdesk.Application.Features.AuditLogs.DTOs
{
    public class AuditLogQueryDto
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        // Free-text match against action, entity name and change payload.
        public string? Search { get; set; }

        public Guid? UserId { get; set; }

        public string? Action { get; set; }

        public string? EntityName { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}
