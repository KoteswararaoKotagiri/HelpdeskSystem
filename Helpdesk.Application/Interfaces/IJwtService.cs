using Helpdesk.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user, string role);
    }
}
