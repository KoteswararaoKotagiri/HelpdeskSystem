using System;
using System.Linq;
using System.Threading.Tasks;
using Helpdesk.Application.Common;
using Helpdesk.Application.Features.AuditLogs.DTOs;
using Helpdesk.Application.Interfaces;
using Helpdesk.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Persistence.Services
{
    // Read-only, paginated access to the audit trail. Contract lives in the Application layer;
    // this queries HelpdeskDbContext directly and resolves user names in one batched follow-up
    // query (no Include, no per-row lookup).
    public class AuditLogService : IAuditLogService
    {
        private readonly HelpdeskDbContext _context;

        public AuditLogService(HelpdeskDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogQueryDto query)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

            var logsQuery = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim();
                logsQuery = logsQuery.Where(a =>
                    a.Action.Contains(term) ||
                    a.EntityName.Contains(term) ||
                    a.Changes.Contains(term));
            }

            if (query.UserId.HasValue)
            {
                logsQuery = logsQuery.Where(a => a.PerformedByUserId == query.UserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Action))
            {
                logsQuery = logsQuery.Where(a => a.Action == query.Action);
            }

            if (!string.IsNullOrWhiteSpace(query.EntityName))
            {
                logsQuery = logsQuery.Where(a => a.EntityName == query.EntityName);
            }

            if (query.FromDate.HasValue)
            {
                logsQuery = logsQuery.Where(a => a.CreatedOn >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                logsQuery = logsQuery.Where(a => a.CreatedOn <= query.ToDate.Value);
            }

            var totalCount = await logsQuery.CountAsync();

            // Newest first. Project only the columns we need for the page.
            var pageRows = await logsQuery
                .OrderByDescending(a => a.CreatedOn)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    a.PerformedByUserId,
                    a.Action,
                    a.EntityName,
                    a.EntityId,
                    a.Changes,
                    a.CreatedOn
                })
                .ToListAsync();

            // Resolve user names for just this page in a single query.
            var userIds = pageRows.Select(r => r.PerformedByUserId).Distinct().ToList();
            var namesById = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName })
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            var items = pageRows
                .Select(r => new AuditLogDto
                {
                    Id = r.Id,
                    UserId = r.PerformedByUserId,
                    UserName = namesById.TryGetValue(r.PerformedByUserId, out var name) ? name : string.Empty,
                    Action = r.Action,
                    EntityName = r.EntityName,
                    EntityId = r.EntityId,
                    OldValue = null,
                    NewValue = r.Changes,
                    IpAddress = null,
                    CreatedOn = r.CreatedOn
                })
                .ToList();

            return new PagedResult<AuditLogDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }
    }
}
