using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Helpdesk.Application.Features.Sla.DTOs;

namespace Helpdesk.Application.Interfaces
{
    public interface ISlaService
    {
        Task<SlaInfoDto?> GetTicketSlaAsync(Guid ticketId);

        Task<IReadOnlyList<SlaTicketDto>> GetBreachesAsync();

        Task<IReadOnlyList<SlaTicketDto>> GetNearBreachAsync();

        Task<IReadOnlyList<SlaTicketDto>> GetOverdueAsync();

        Task<IReadOnlyList<SlaTicketDto>> GetCountdownAsync();

        Task<SlaPerformanceDto> GetPerformanceAsync();
    }
}
