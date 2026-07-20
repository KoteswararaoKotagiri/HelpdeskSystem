using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpdesk.Application.Features.Dashboard.DTOs;
using Helpdesk.Application.Interfaces;
using Helpdesk.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Persistence.Services
{
    // Read-only aggregation queries for the dashboard. Kept in the Persistence layer because
    // it works directly against HelpdeskDbContext; the contract (IDashboardService) lives in
    // the Application layer, matching the existing IJwtService / ICurrentUserService pattern.
    public class DashboardService : IDashboardService
    {
        // Status / priority codes match the seed data (DbInitializer) and the existing TicketController usage.
        private const string StatusOpen = "OPEN";
        private const string StatusAssigned = "ASSIGNED";
        private const string StatusInProgress = "IN_PROGRESS";
        private const string StatusResolved = "RESOLVED";
        private const string StatusClosed = "CLOSED";
        private const string PriorityCritical = "CRITICAL";

        private readonly HelpdeskDbContext _context;

        public DashboardService(HelpdeskDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetStatsAsync()
        {
            // One grouped query for all status counts, then map by code in memory.
            var statusCounts = await _context.Tickets
                .GroupBy(t => t.Status.Code)
                .Select(g => new { Code = g.Key, Count = g.Count() })
                .ToListAsync();

            int CountFor(string code) =>
                statusCounts.FirstOrDefault(x => x.Code == code)?.Count ?? 0;

            var criticalTickets = await _context.Tickets
                .CountAsync(t => t.Priority.Code == PriorityCritical);

            // SLA breach: still open (not closed) and elapsed hours since creation exceed the priority SLA.
            var now = DateTime.UtcNow;
            var slaBreaches = await _context.Tickets
                .CountAsync(t =>
                    !t.Status.IsClosed &&
                    EF.Functions.DateDiffHour(t.CreatedOn, now) > t.Priority.SlaHours);

            return new DashboardStatsDto
            {
                TotalTickets = statusCounts.Sum(x => x.Count),
                OpenTickets = CountFor(StatusOpen),
                AssignedTickets = CountFor(StatusAssigned),
                InProgressTickets = CountFor(StatusInProgress),
                ResolvedTickets = CountFor(StatusResolved),
                ClosedTickets = CountFor(StatusClosed),
                CriticalTickets = criticalTickets,
                SlaBreaches = slaBreaches
            };
        }

        public async Task<DashboardChartsDto> GetChartsAsync()
        {
            var byStatus = await _context.Tickets
                .GroupBy(t => t.Status.Name)
                .Select(g => new ChartPointDto { Label = g.Key, Value = g.Count() })
                .ToListAsync();

            var byPriority = await _context.Tickets
                .GroupBy(t => t.Priority.Name)
                .Select(g => new ChartPointDto { Label = g.Key, Value = g.Count() })
                .ToListAsync();

            // Tickets have no department of their own; group by the requester's (creator's) department.
            var byDepartment = await (
                from ticket in _context.Tickets
                join user in _context.Users on ticket.CreatedByUserId equals user.Id
                group ticket by user.Department.Name into g
                select new ChartPointDto { Label = g.Key, Value = g.Count() })
                .ToListAsync();

            return new DashboardChartsDto
            {
                TicketsByStatus = byStatus,
                TicketsByPriority = byPriority,
                TicketsByDepartment = byDepartment,
                WeeklyTrend = await GetWeeklyTrendAsync(),
                MonthlyTrend = await GetMonthlyTrendAsync()
            };
        }

        public async Task<IReadOnlyList<ActivityItemDto>> GetRecentActivityAsync(int count = 20)
        {
            var created = await _context.Tickets
                .OrderByDescending(t => t.CreatedOn)
                .Take(count)
                .Select(t => new ActivityItemDto
                {
                    ActivityType = "Ticket Created",
                    TicketNumber = t.TicketNumber,
                    UserName = _context.Users
                        .Where(u => u.Id == t.CreatedByUserId)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault(),
                    Description = "Ticket \"" + t.Title + "\" was created",
                    Timestamp = t.CreatedOn
                })
                .ToListAsync();

            var comments = await _context.TicketComments
                .OrderByDescending(c => c.CreatedOn)
                .Take(count)
                .Select(c => new ActivityItemDto
                {
                    ActivityType = "Comment Added",
                    TicketNumber = c.Ticket.TicketNumber,
                    UserName = c.User.FirstName + " " + c.User.LastName,
                    Description = "A comment was added",
                    Timestamp = c.CreatedOn
                })
                .ToListAsync();

            var attachments = await _context.TicketAttachments
                .OrderByDescending(a => a.CreatedOn)
                .Take(count)
                .Select(a => new ActivityItemDto
                {
                    ActivityType = "Attachment Uploaded",
                    TicketNumber = a.Ticket.TicketNumber,
                    UserName = a.UploadedByUser.FirstName + " " + a.UploadedByUser.LastName,
                    Description = a.OriginalFileName + " was uploaded",
                    Timestamp = a.CreatedOn
                })
                .ToListAsync();

            // Assignment and status-change events are sourced from the audit log
            // (EntityName = "Ticket"). Comment audit rows are excluded to avoid double counting.
            var audits = await _context.AuditLogs
                .Where(a => a.EntityName == "Ticket" && a.Action != "Comment Added")
                .OrderByDescending(a => a.CreatedOn)
                .Take(count)
                .Select(a => new ActivityItemDto
                {
                    ActivityType = a.Action,
                    TicketNumber = _context.Tickets
                        .Where(t => t.Id == a.EntityId)
                        .Select(t => t.TicketNumber)
                        .FirstOrDefault(),
                    UserName = _context.Users
                        .Where(u => u.Id == a.PerformedByUserId)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault(),
                    Description = a.Changes,
                    Timestamp = a.CreatedOn
                })
                .ToListAsync();

            return created
                .Concat(comments)
                .Concat(attachments)
                .Concat(audits)
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToList();
        }

        public async Task<IReadOnlyList<RecentTicketDto>> GetRecentTicketsAsync(int count = 10)
        {
            return await _context.Tickets
                .OrderByDescending(t => t.CreatedOn)
                .Take(count)
                .Select(t => new RecentTicketDto
                {
                    TicketNumber = t.TicketNumber,
                    Title = t.Title,
                    Status = t.Status.Name,
                    Priority = t.Priority.Name,
                    Requester = _context.Users
                        .Where(u => u.Id == t.CreatedByUserId)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault(),
                    Assignee = _context.Users
                        .Where(u => u.Id == t.AssignedToUserId)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault(),
                    CreatedDate = t.CreatedOn
                })
                .ToListAsync();
        }

        private async Task<List<ChartPointDto>> GetWeeklyTrendAsync()
        {
            // Tickets created per day for the last 7 days, zero-filled for days with no tickets.
            var weekStart = DateTime.UtcNow.Date.AddDays(-6);

            var raw = await _context.Tickets
                .Where(t => t.CreatedOn >= weekStart)
                .GroupBy(t => t.CreatedOn.Date)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToListAsync();

            return Enumerable.Range(0, 7)
                .Select(offset => weekStart.AddDays(offset))
                .Select(day => new ChartPointDto
                {
                    Label = day.ToString("ddd"),
                    Value = raw.FirstOrDefault(x => x.Day == day)?.Count ?? 0
                })
                .ToList();
        }

        private async Task<List<ChartPointDto>> GetMonthlyTrendAsync()
        {
            // Tickets created per month for the last 12 months, zero-filled for empty months.
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
                .AddMonths(-11);

            var raw = await _context.Tickets
                .Where(t => t.CreatedOn >= monthStart)
                .GroupBy(t => new { t.CreatedOn.Year, t.CreatedOn.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();

            return Enumerable.Range(0, 12)
                .Select(offset => monthStart.AddMonths(offset))
                .Select(month => new ChartPointDto
                {
                    Label = month.ToString("MMM yyyy"),
                    Value = raw.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month)?.Count ?? 0
                })
                .ToList();
        }
    }
}
