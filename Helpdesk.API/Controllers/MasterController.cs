using Helpdesk.Persistence.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Helpdesk.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MasterController : ControllerBase
    {
        private readonly HelpdeskDbContext _context;

        public MasterController(HelpdeskDbContext context)
        {
            _context = context;
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles
                .Where(x => x.IsActive)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Code
                })
                .ToListAsync();

            return Ok(roles);
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments
                .Where(x => x.IsActive)
                .Select(x => new
                {
                    x.Id,
                    x.Name
                })
                .ToListAsync();

            return Ok(departments);
        }

        [HttpGet("ticket-statuses")]
        public async Task<IActionResult> GetTicketStatuses()
        {
            var statuses = await _context.TicketStatuses
                .OrderBy(x => x.Sequence)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Code,
                    x.Color
                })
                .ToListAsync();

            return Ok(statuses);
        }

        [HttpGet("ticket-priorities")]
        public async Task<IActionResult> GetTicketPriorities()
        {
            var priorities = await _context.TicketPriorities
                .OrderBy(x => x.SortOrder)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Code,
                    x.SlaHours
                })
                .ToListAsync();

            return Ok(priorities);
        }
    }
}
