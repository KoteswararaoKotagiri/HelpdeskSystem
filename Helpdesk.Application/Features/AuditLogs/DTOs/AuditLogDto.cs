using System;

namespace Helpdesk.Application.Features.AuditLogs.DTOs
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string EntityName { get; set; } = string.Empty;

        public Guid EntityId { get; set; }

        // The current schema stores a single change payload, mapped to NewValue.
        // OldValue / IpAddress have no column yet, so they stay null until the schema tracks them.
        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public string? IpAddress { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
