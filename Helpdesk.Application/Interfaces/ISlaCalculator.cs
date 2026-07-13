using Helpdesk.Application.Features.Sla;
using Helpdesk.Application.Features.Sla.DTOs;

namespace Helpdesk.Application.Interfaces
{
    // Pure SLA engine: resolves the applicable policy and computes due dates, status and escalation.
    // No data access — callers supply a SlaTicketContext.
    public interface ISlaCalculator
    {
        SlaInfoDto Calculate(SlaTicketContext ticket);
    }
}
