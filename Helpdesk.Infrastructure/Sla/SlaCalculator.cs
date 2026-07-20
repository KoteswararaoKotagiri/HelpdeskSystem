using System;
using System.Linq;
using Helpdesk.Application.Configuration;
using Helpdesk.Application.Features.Sla;
using Helpdesk.Application.Features.Sla.DTOs;
using Helpdesk.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Helpdesk.Infrastructure.Sla
{
    public class SlaCalculator : ISlaCalculator
    {
        private const int FallbackResolutionHours = 24;

        private readonly SlaSettings _settings;

        public SlaCalculator(IOptions<SlaSettings> settings)
        {
            _settings = settings.Value;
        }

        public SlaInfoDto Calculate(SlaTicketContext ticket)
        {
            var policy = ResolvePolicy(ticket);

            var resolutionHours =
                policy.ResolutionHours > 0 ? policy.ResolutionHours
                : ticket.PrioritySlaHours > 0 ? ticket.PrioritySlaHours
                : FallbackResolutionHours;

            var start = ticket.CreatedOn;

            DateTime? responseDue = policy.ResponseMinutes > 0
                ? AddDuration(start, TimeSpan.FromMinutes(policy.ResponseMinutes), policy.UseBusinessHours)
                : null;

            var resolutionDue = AddDuration(start, TimeSpan.FromHours(resolutionHours), policy.UseBusinessHours);

            var now = DateTime.UtcNow;
            var reference = ticket.IsClosed ? (ticket.ClosedAt ?? now) : now;

            var window = (resolutionDue - start).TotalMinutes;
            var elapsed = (reference - start).TotalMinutes;
            var elapsedPercent = window > 0 ? Math.Clamp(elapsed / window * 100, 0, 999) : 100;

            var status = ResolveStatus(ticket, reference, resolutionDue, elapsedPercent);
            var isOverdue = !ticket.IsClosed && now > resolutionDue;

            return new SlaInfoDto
            {
                StartTime = start,
                ResponseDueAt = responseDue,
                ResolutionDueAt = resolutionDue,
                RemainingMinutes = Math.Round((resolutionDue - now).TotalMinutes, 1),
                ElapsedPercent = Math.Round(elapsedPercent, 1),
                Status = status.ToString(),
                IsOverdue = isOverdue,
                EscalationLevel = ticket.IsClosed ? 0 : ResolveEscalationLevel(elapsedPercent)
            };
        }

        private SlaStatus ResolveStatus(
            SlaTicketContext ticket, DateTime reference, DateTime resolutionDue, double elapsedPercent)
        {
            if (ticket.IsClosed)
            {
                return reference <= resolutionDue ? SlaStatus.Met : SlaStatus.Breached;
            }

            if (DateTime.UtcNow > resolutionDue)
            {
                return SlaStatus.Breached;
            }

            return elapsedPercent >= _settings.WarningThresholdPercent ? SlaStatus.AtRisk : SlaStatus.OnTrack;
        }

        private int ResolveEscalationLevel(double elapsedPercent)
        {
            return _settings.Escalations
                .Where(e => elapsedPercent >= e.TriggerAtPercent)
                .Select(e => e.Level)
                .DefaultIfEmpty(0)
                .Max();
        }

        private SlaPolicySettings ResolvePolicy(SlaTicketContext ticket)
        {
            SlaPolicySettings? best = null;
            var bestScore = -1;

            foreach (var policy in _settings.Policies)
            {
                var score = 0;
                var matches = true;

                if (!string.IsNullOrWhiteSpace(policy.CategoryName))
                {
                    if (Eq(policy.CategoryName, ticket.CategoryName)) score += 4;
                    else matches = false;
                }

                if (matches && !string.IsNullOrWhiteSpace(policy.DepartmentName))
                {
                    if (Eq(policy.DepartmentName, ticket.DepartmentName)) score += 2;
                    else matches = false;
                }

                if (matches && !string.IsNullOrWhiteSpace(policy.PriorityCode))
                {
                    if (Eq(policy.PriorityCode, ticket.PriorityCode)) score += 1;
                    else matches = false;
                }

                if (matches && score > bestScore)
                {
                    best = policy;
                    bestScore = score;
                }
            }

            return best ?? _settings.Default;
        }

        // Adds a duration either as calendar time or within configured working hours.
        // ponytail: business-hours calc ignores public holidays; add a holiday calendar if needed.
        private DateTime AddDuration(DateTime start, TimeSpan duration, bool useBusinessHours)
        {
            if (!useBusinessHours)
            {
                return start.Add(duration);
            }

            var dayStart = TimeSpan.FromHours(_settings.WorkingHours.StartHour);
            var dayEnd = TimeSpan.FromHours(_settings.WorkingHours.EndHour);
            var remaining = duration;
            var cursor = start;

            while (remaining > TimeSpan.Zero)
            {
                if (!IsWorkingDay(cursor.DayOfWeek))
                {
                    cursor = cursor.Date.AddDays(1).Add(dayStart);
                    continue;
                }

                var timeOfDay = cursor.TimeOfDay;
                if (timeOfDay < dayStart)
                {
                    cursor = cursor.Date.Add(dayStart);
                    timeOfDay = dayStart;
                }

                if (timeOfDay >= dayEnd)
                {
                    cursor = cursor.Date.AddDays(1).Add(dayStart);
                    continue;
                }

                var availableToday = dayEnd - timeOfDay;
                if (remaining <= availableToday)
                {
                    return cursor.Add(remaining);
                }

                remaining -= availableToday;
                cursor = cursor.Date.AddDays(1).Add(dayStart);
            }

            return cursor;
        }

        private bool IsWorkingDay(DayOfWeek day) =>
            _settings.WorkingHours.WorkingDays.Contains((int)day);

        private static bool Eq(string a, string b) =>
            string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
