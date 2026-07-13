using Helpdesk.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Support Engineer")]
    public class SlaController : ControllerBase
    {
        private readonly ISlaService _slaService;

        public SlaController(ISlaService slaService)
        {
            _slaService = slaService;
        }

        [HttpGet("tickets/{ticketId:guid}")]
        public async Task<IActionResult> GetTicketSla(Guid ticketId)
        {
            var sla = await _slaService.GetTicketSlaAsync(ticketId);
            return sla is null ? NotFound(new { error = "Ticket not found." }) : Ok(sla);
        }

        [HttpGet("breaches")]
        public async Task<IActionResult> GetBreaches()
        {
            return Ok(await _slaService.GetBreachesAsync());
        }

        [HttpGet("near")]
        public async Task<IActionResult> GetNearBreach()
        {
            return Ok(await _slaService.GetNearBreachAsync());
        }

        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdue()
        {
            return Ok(await _slaService.GetOverdueAsync());
        }

        [HttpGet("countdown")]
        public async Task<IActionResult> GetCountdown()
        {
            return Ok(await _slaService.GetCountdownAsync());
        }

        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformance()
        {
            return Ok(await _slaService.GetPerformanceAsync());
        }
    }
}
