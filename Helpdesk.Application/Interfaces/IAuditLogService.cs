using System.Threading.Tasks;
using Helpdesk.Application.Common;
using Helpdesk.Application.Features.AuditLogs.DTOs;

namespace Helpdesk.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogQueryDto query);
    }
}
