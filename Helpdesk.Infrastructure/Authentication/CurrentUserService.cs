using Helpdesk.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Infrastructure.Authentication
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId =>
            Guid.Parse(
                _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirstValue(ClaimTypes.NameIdentifier)
                ??
                throw new Exception("User Id not found"));

        public string Email =>
            _httpContextAccessor.HttpContext?
                .User?
                .FindFirstValue(ClaimTypes.Email)
            ?? string.Empty;

        public string Role =>
            _httpContextAccessor.HttpContext?
                .User?
                .FindFirstValue(ClaimTypes.Role)
            ?? string.Empty;
    }
}
