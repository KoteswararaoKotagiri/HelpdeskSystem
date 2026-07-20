using System.Collections.Generic;
using System.Threading.Tasks;
using Helpdesk.Application.Features.Dashboard.DTOs;

namespace Helpdesk.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetStatsAsync();

        Task<DashboardChartsDto> GetChartsAsync();

        Task<IReadOnlyList<ActivityItemDto>> GetRecentActivityAsync(int count = 20);

        Task<IReadOnlyList<RecentTicketDto>> GetRecentTicketsAsync(int count = 10);
    }
}
