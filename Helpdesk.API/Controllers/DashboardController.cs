using Helpdesk.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            return Ok(await _dashboardService.GetStatsAsync());
        }

        [HttpGet("charts")]
        public async Task<IActionResult> GetCharts()
        {
            return Ok(await _dashboardService.GetChartsAsync());
        }

        [HttpGet("activity")]
        public async Task<IActionResult> GetActivity()
        {
            return Ok(await _dashboardService.GetRecentActivityAsync());
        }

        [HttpGet("recent-tickets")]
        public async Task<IActionResult> GetRecentTickets()
        {
            return Ok(await _dashboardService.GetRecentTicketsAsync());
        }
    }
}
