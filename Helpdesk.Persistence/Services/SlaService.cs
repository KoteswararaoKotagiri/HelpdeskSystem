using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpdesk.Application.Configuration;
using Helpdesk.Application.Features.Sla;
using Helpdesk.Application.Features.Sla.DTOs;
using Helpdesk.Application.Interfaces;
using Helpdesk.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Helpdesk.Persistence.Services
{
    // Loads ticket data and applies the pure SLA engine to power the SLA dashboard.
    // Open-ticket views load only non-closed tickets; performance/breaches include a recent window.
    public class SlaService : ISlaService
    {
        private readonly HelpdeskDbContext _context;
        private readonly ISlaCalculator _calculator;
        private readonly SlaSettings _settings;

        public SlaService(
            HelpdeskDbContext context,
            ISlaCalculator calculator,
            IOptions<SlaSettings> settings)
        {
            _context = context;
            _calculator = calculator;
            _settings = settings.Value;
        }

        public async Task<SlaInfoDto?> GetTicketSlaAsync(Guid ticketId)
        {
            var row = await ProjectRows(_context.Tickets.Where(t => t.Id == ticketId))
                .FirstOrDefaultAsync();

            return row is null ? null : _calculator.Calculate(ToContext(row));
        }

        public async Task<IReadOnlyList<SlaTicketDto>> GetBreachesAsync()
        {
            var evaluated = await EvaluateAsync(includeRecentlyClosed: true);
            return evaluated
                .Where(x => x.Info.Status == nameof(SlaStatus.Breached))
                .OrderByDescending(x => x.Info.ElapsedPercent)
                .Select(x => x.Dto)
                .ToList();
        }

        public async Task<IReadOnlyList<SlaTicketDto>> GetNearBreachAsync()
        {
            var evaluated = await EvaluateAsync(includeRecentlyClosed: false);
            return evaluated
                .Where(x => x.Info.Status == nameof(SlaStatus.AtRisk))
                .OrderBy(x => x.Info.RemainingMinutes)
                .Select(x => x.Dto)
                .ToList();
        }

        public async Task<IReadOnlyList<SlaTicketDto>> GetOverdueAsync()
        {
            var evaluated = await EvaluateAsync(includeRecentlyClosed: false);
            return evaluated
                .Where(x => x.Info.IsOverdue)
                .OrderBy(x => x.Info.RemainingMinutes)
                .Select(x => x.Dto)
                .ToList();
        }

        public async Task<IReadOnlyList<SlaTicketDto>> GetCountdownAsync()
        {
            var evaluated = await EvaluateAsync(includeRecentlyClosed: false);
            return evaluated
                .OrderBy(x => x.Info.RemainingMinutes)
                .Select(x => x.Dto)
                .ToList();
        }

        public async Task<SlaPerformanceDto> GetPerformanceAsync()
        {
            var evaluated = await EvaluateAsync(includeRecentlyClosed: true);
            var infos = evaluated.Select(x => x.Info).ToList();

            var met = infos.Count(i => i.Status == nameof(SlaStatus.Met));
            var breached = infos.Count(i => i.Status == nameof(SlaStatus.Breached));
            var concluded = met + breached;

            return new SlaPerformanceDto
            {
                TotalEvaluated = infos.Count,
                Met = met,
                Breached = breached,
                AtRisk = infos.Count(i => i.Status == nameof(SlaStatus.AtRisk)),
                OnTrack = infos.Count(i => i.Status == nameof(SlaStatus.OnTrack)),
                Overdue = infos.Count(i => i.IsOverdue),
                CompliancePercent = concluded == 0 ? 100 : Math.Round(met / (double)concluded * 100, 1),
                WindowDays = _settings.PerformanceWindowDays
            };
        }

        private async Task<List<(SlaTicketDto Dto, SlaInfoDto Info)>> EvaluateAsync(bool includeRecentlyClosed)
        {
            var windowStart = DateTime.UtcNow.AddDays(-_settings.PerformanceWindowDays);

            var query = includeRecentlyClosed
                ? _context.Tickets.Where(t => !t.Status.IsClosed || t.CreatedOn >= windowStart)
                : _context.Tickets.Where(t => !t.Status.IsClosed);

            var rows = await ProjectRows(query).ToListAsync();

            return rows.Select(row =>
            {
                var info = _calculator.Calculate(ToContext(row));
                var dto = new SlaTicketDto
                {
                    TicketId = row.Id,
                    TicketNumber = row.TicketNumber,
                    Title = row.Title,
                    Status = row.StatusName,
                    Priority = row.PriorityName,
                    Assignee = row.AssigneeName ?? "Unassigned",
                    ResolutionDueAt = info.ResolutionDueAt,
                    RemainingMinutes = info.RemainingMinutes,
                    SlaStatus = info.Status,
                    IsOverdue = info.IsOverdue,
                    EscalationLevel = info.EscalationLevel
                };
                return (dto, info);
            }).ToList();
        }

        // Department is taken from the requester's department (tickets have no department of their own).
        private IQueryable<SlaRow> ProjectRows(IQueryable<Domain.Entities.Tickets.Ticket> query)
        {
            return query.Select(t => new SlaRow
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                Title = t.Title,
                StatusName = t.Status.Name,
                StatusIsClosed = t.Status.IsClosed,
                PriorityName = t.Priority.Name,
                PriorityCode = t.Priority.Code,
                PrioritySlaHours = t.Priority.SlaHours,
                CategoryName = t.Category.Name,
                DepartmentName = _context.Users
                    .Where(u => u.Id == t.CreatedByUserId)
                    .Select(u => u.Department.Name)
                    .FirstOrDefault(),
                AssigneeName = _context.Users
                    .Where(u => u.Id == t.AssignedToUserId)
                    .Select(u => u.FirstName + " " + u.LastName)
                    .FirstOrDefault(),
                CreatedOn = t.CreatedOn,
                ModifiedOn = t.ModifiedOn
            });
        }

        private static SlaTicketContext ToContext(SlaRow row) => new()
        {
            CreatedOn = row.CreatedOn,
            PriorityCode = row.PriorityCode,
            PrioritySlaHours = row.PrioritySlaHours,
            DepartmentName = row.DepartmentName ?? string.Empty,
            CategoryName = row.CategoryName,
            IsClosed = row.StatusIsClosed,
            ClosedAt = row.StatusIsClosed ? row.ModifiedOn : null
        };

        private sealed class SlaRow
        {
            public Guid Id { get; set; }
            public string TicketNumber { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string StatusName { get; set; } = string.Empty;
            public bool StatusIsClosed { get; set; }
            public string PriorityName { get; set; } = string.Empty;
            public string PriorityCode { get; set; } = string.Empty;
            public int PrioritySlaHours { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public string? DepartmentName { get; set; }
            public string? AssigneeName { get; set; }
            public DateTime CreatedOn { get; set; }
            public DateTime? ModifiedOn { get; set; }
        }
    }
}
