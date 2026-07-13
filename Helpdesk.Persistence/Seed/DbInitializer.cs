using Helpdesk.Domain.Entities.Identity;
using Helpdesk.Domain.Entities.Masters;
using Helpdesk.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Persistence.Seed
{

    public static class DbInitializer
    {
        public static async Task SeedAsync(HelpdeskDbContext context)
        {
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "Admin",
                        Code = "ADMIN",
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow
                    },
                    new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "Support Engineer",
                        Code = "SUPPORT_ENGINEER",
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow
                    },
                    new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "Employee",
                        Code = "EMPLOYEE",
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow
                    }
                );
            }

            if (!context.TicketStatuses.Any())
            {
                context.TicketStatuses.AddRange(
                    new TicketStatus
                    {
                        Id = Guid.NewGuid(),
                        Name = "Open",
                        Code = "OPEN",
                        Color = "#ff9800",
                        Sequence = 1,
                        IsClosed = false,
                        CreatedOn = DateTime.UtcNow
                    },
                    new TicketStatus
                    {
                        Id = Guid.NewGuid(),
                        Name = "Assigned",
                        Code = "ASSIGNED",
                        Color = "#9c27b0",
                        Sequence = 2,
                        IsClosed = false,
                        CreatedOn = DateTime.UtcNow
                    },
                    new TicketStatus
                    {
                        Id = Guid.NewGuid(),
                        Name = "In Progress",
                        Code = "IN_PROGRESS",
                        Color = "#2196f3",
                        Sequence = 3,
                        IsClosed = false,
                        CreatedOn = DateTime.UtcNow
                    },
                    new TicketStatus
                    {
                        Id = Guid.NewGuid(),
                        Name = "Resolved",
                        Code = "RESOLVED",
                        Color = "#4caf50",
                        Sequence = 4,
                        IsClosed = true,
                        CreatedOn = DateTime.UtcNow
                    },
                    new TicketStatus
                    {
                        Id = Guid.NewGuid(),
                        Name = "Closed",
                        Code = "CLOSED",
                        Color = "#607d8b",
                        Sequence = 5,
                        IsClosed = true,
                        CreatedOn = DateTime.UtcNow
                    }
                );
            }

            if (!context.TicketPriorities.Any())
            {
                context.TicketPriorities.AddRange(
                    new TicketPriority
                    {
                        Id = Guid.NewGuid(),
                        Name = "Low",
                        Code = "LOW",
                        SlaHours = 48,
                        SortOrder = 1,
                        CreatedOn = DateTime.UtcNow
                    },
                    new TicketPriority
                    {
                        Id = Guid.NewGuid(),
                        Name = "Medium",
                        Code = "MEDIUM",
                        SlaHours = 24,
                        SortOrder = 2,
                        CreatedOn = DateTime.UtcNow
                    },
                    new TicketPriority
                    {
                        Id = Guid.NewGuid(),
                        Name = "High",
                        Code = "HIGH",
                        SlaHours = 8,
                        SortOrder = 3,
                        CreatedOn = DateTime.UtcNow
                    },
                    new TicketPriority
                    {
                        Id = Guid.NewGuid(),
                        Name = "Critical",
                        Code = "CRITICAL",
                        SlaHours = 4,
                        SortOrder = 4,
                        CreatedOn = DateTime.UtcNow
                    }
                );
            }
            #region Departments

            if (!context.Departments.Any())
            {
                context.Departments.AddRange(
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = "IT Support",
                        Description = "Handles technical issues",
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow
                    },

                    new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = "Human Resources",
                        Description = "Handles HR operations",
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow
                    },

                    new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = "Finance",
                        Description = "Handles finance operations",
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow
                    }
                );
            }

            #endregion
            #region Ticket Categories

            if (!context.TicketCategories.Any())
            {
                context.TicketCategories.AddRange(
                    new TicketCategory
                    {
                        Id = Guid.NewGuid(),
                        Name = "Software",
                        Description = "Software related issues",
                        CreatedOn = DateTime.UtcNow
                    },

                    new TicketCategory
                    {
                        Id = Guid.NewGuid(),
                        Name = "Hardware",
                        Description = "Hardware related issues",
                        CreatedOn = DateTime.UtcNow
                    },

                    new TicketCategory
                    {
                        Id = Guid.NewGuid(),
                        Name = "Network",
                        Description = "Network related issues",
                        CreatedOn = DateTime.UtcNow
                    }
                );
            }

            #endregion
            await context.SaveChangesAsync();

            #region Admin User

            // Seed a default admin so the app is usable immediately after first run.
            // Credentials: admin@helpdesk.com / password
            if (!context.Users.Any())
            {
                var adminRole = await context.Roles
                    .FirstAsync(r => r.Code == "ADMIN");

                var department = await context.Departments
                    .FirstAsync();

                context.Users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "System",
                    LastName = "Administrator",
                    Email = "admin@helpdesk.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                    RoleId = adminRole.Id,
                    DepartmentId = department.Id,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }

            #endregion
        }
    }
}
