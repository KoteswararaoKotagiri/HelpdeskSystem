using Helpdesk.Application.Features.Auth.DTOs;
using Helpdesk.Application.Interfaces;
using Helpdesk.Domain.Entities.Identity;
using Helpdesk.Infrastructure.Authentication;
using Helpdesk.Persistence.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly HelpdeskDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthController(
            HelpdeskDbContext context,
            IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            var exists = await _context.Users
                .AnyAsync(x => x.Email == request.Email);

            if (exists)
            {
                return BadRequest("Email already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = PasswordHasher.Hash(request.Password),
                RoleId = request.RoleId,
                DepartmentId = request.DepartmentId,
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var user = await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var validPassword = PasswordHasher.Verify(
                request.Password,
                user.PasswordHash);

            if (!validPassword)
            {
                return Unauthorized("Invalid email or password.");
            }

            var token = _jwtService.GenerateToken(
                user,
                user.Role.Name);

            var response = new LoginResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Role = user.Role.Name
            };

            return Ok(response);
        }
    }
}
