using System;
using System.Threading.Tasks;
using Helpdesk.Application.Common;
using Helpdesk.Application.Features.Users.DTOs;

namespace Helpdesk.Application.Interfaces
{
    public interface IUserService
    {
        Task<Result<PagedResult<UserListItemDto>>> GetUsersAsync(UserListQueryDto query);

        Task<Result<UserDetailDto>> GetUserByIdAsync(Guid id);

        Task<Result<UserDetailDto>> UpdateUserAsync(Guid id, UpdateUserRequestDto request);

        Task<Result<UserDetailDto>> SetActiveStatusAsync(Guid id, bool isActive);

        Task<Result<UserDetailDto>> ChangeRoleAsync(Guid id, Guid roleId);

        Task<Result<UserDetailDto>> ChangeDepartmentAsync(Guid id, Guid departmentId);

        Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);

        Task<Result<UserDetailDto>> GetProfileAsync(Guid userId);

        Task<Result<UserDetailDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request);
    }
}
