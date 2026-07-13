using System.Collections.Generic;

namespace Helpdesk.Application.Features.Dashboard.DTOs
{
    public class DashboardChartsDto
    {
        public List<ChartPointDto> TicketsByStatus { get; set; } = new();

        public List<ChartPointDto> TicketsByPriority { get; set; } = new();

        public List<ChartPointDto> TicketsByDepartment { get; set; } = new();

        public List<ChartPointDto> WeeklyTrend { get; set; } = new();

        public List<ChartPointDto> MonthlyTrend { get; set; } = new();
    }

    // A single label/value pair. Frontend chart libraries (ApexCharts/Chart.js) map directly onto this.
    public class ChartPointDto
    {
        public string Label { get; set; } = string.Empty;

        public int Value { get; set; }
    }
}
