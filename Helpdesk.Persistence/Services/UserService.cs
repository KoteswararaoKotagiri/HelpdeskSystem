using System;
using System.Linq;
using System.Threading.Tasks;
using Helpdesk.Application.Common;
using Helpdesk.Application.Features.Users.DTOs;
using Helpdesk.Application.Interfaces;
using Helpdesk.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Persistence.Services
{
    // User management operations. Contract (IUserService) lives in the Application layer;
    // this implementation works against HelpdeskDbContext and reuses BCrypt for hashing,
    // matching the existing PasswordHasher / DbInitializer approach.
    public class UserService : IUserService
    {
        private const int MinPasswordLength = 6;

        private readonly HelpdeskDbContext _context;

        public UserService(HelpdeskDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<UserListItemDto>>> GetUsersAsync(UserListQueryDto query)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 100 ? 10 : query.PageSize;

            var usersQuery = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim();
                usersQuery = usersQuery.Where(u =>
                    u.FirstName.Contains(term) ||
                    u.LastName.Contains(term) ||
                    u.Email.Contains(term));
            }

            if (query.RoleId.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.RoleId == query.RoleId.Value);
            }

            if (query.DepartmentId.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.DepartmentId == query.DepartmentId.Value);
            }

            if (query.IsActive.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.IsActive == query.IsActive.Value);
            }

            usersQuery = (query.SortBy?.Trim().ToLower()) switch
            {
                "name" => query.SortDescending
                    ? usersQuery.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName)
                    : usersQuery.OrderBy(u => u.FirstName).ThenBy(u => u.LastName),
                "createddate" => query.SortDescending
                    ? usersQuery.OrderByDescending(u => u.CreatedOn)
                    : usersQuery.OrderBy(u => u.CreatedOn),
                _ => usersQuery.OrderByDescending(u => u.CreatedOn)
            };

            var totalCount = await usersQuery.CountAsync();

            var items = await usersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListItemDto
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    Role = u.Role.Name,
                    Department = u.Department.Name,
                    IsActive = u.IsActive,
                    CreatedOn = u.CreatedOn
                })
                .ToListAsync();

            return Result<PagedResult<UserListItemDto>>.Success(new PagedResult<UserListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        public async Task<Result<UserDetailDto>> GetUserByIdAsync(Guid id)
        {
            var user = await LoadDetailAsync(id);

            return user is null
                ? Result<UserDetailDto>.NotFound("User not found.")
                : Result<UserDetailDto>.Success(user);
        }

        public async Task<Result<UserDetailDto>> UpdateUserAsync(Guid id, UpdateUserRequestDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null)
            {
                return Result<UserDetailDto>.NotFound("User not found.");
            }

            var emailInUse = await _context.Users
                .AnyAsync(u => u.Email == request.Email && u.Id != id);
            if (emailInUse)
            {
                return Result<UserDetailDto>.Conflict("Email is already in use by another user.");
            }

            var departmentExists = await _context.Departments
                .AnyAsync(d => d.Id == request.DepartmentId);
            if (!departmentExists)
            {
                return Result<UserDetailDto>.Invalid("Department not found.");
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.DepartmentId = request.DepartmentId;
            user.IsActive = request.IsActive;
            user.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(id);
        }

        public async Task<Result<UserDetailDto>> SetActiveStatusAsync(Guid id, bool isActive)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null)
            {
                return Result<UserDetailDto>.NotFound("User not found.");
            }

            user.IsActive = isActive;
            user.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(id);
        }

        public async Task<Result<UserDetailDto>> ChangeRoleAsync(Guid id, Guid roleId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null)
            {
                return Result<UserDetailDto>.NotFound("User not found.");
            }

            var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);
            if (!roleExists)
            {
                return Result<UserDetailDto>.Invalid("Role not found.");
            }

            user.RoleId = roleId;
            user.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(id);
        }

        public async Task<Result<UserDetailDto>> ChangeDepartmentAsync(Guid id, Guid departmentId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null)
            {
                return Result<UserDetailDto>.NotFound("User not found.");
            }

            var departmentExists = await _context.Departments.AnyAsync(d => d.Id == departmentId);
            if (!departmentExists)
            {
                return Result<UserDetailDto>.Invalid("Department not found.");
            }

            user.DepartmentId = departmentId;
            user.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(id);
        }

        public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                return Result.Invalid("New password and confirmation do not match.");
            }

            if (request.NewPassword.Length < MinPasswordLength)
            {
                return Result.Invalid($"New password must be at least {MinPasswordLength} characters.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
            {
                return Result.NotFound("User not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return Result.Invalid("Current password is incorrect.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public Task<Result<UserDetailDto>> GetProfileAsync(Guid userId) => GetUserByIdAsync(userId);

        public async Task<Result<UserDetailDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
            {
                return Result<UserDetailDto>.NotFound("User not found.");
            }

            var emailInUse = await _context.Users
                .AnyAsync(u => u.Email == request.Email && u.Id != userId);
            if (emailInUse)
            {
                return Result<UserDetailDto>.Conflict("Email is already in use by another user.");
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(userId);
        }

        // Single source of truth for the detail projection, reused after every mutation.
        private Task<UserDetailDto?> LoadDetailAsync(Guid id)
        {
            return _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserDetailDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    RoleId = u.RoleId,
                    RoleName = u.Role.Name,
                    DepartmentId = u.DepartmentId,
                    DepartmentName = u.Department.Name,
                    IsActive = u.IsActive,
                    CreatedOn = u.CreatedOn,
                    LastUpdated = u.ModifiedOn,
                    LastLoginAt = null
                })
                .FirstOrDefaultAsync();
        }
    }
}
