using Helpdesk.Domain.Entities.Identity;
using Helpdesk.Domain.Entities.Masters;
using Helpdesk.Domain.Entities.Tickets;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Persistence.Contexts
{
    public class HelpdeskDbContext : DbContext
    {
        public HelpdeskDbContext(DbContextOptions<HelpdeskDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<Role> Roles => Set<Role>();

        public DbSet<Department> Departments => Set<Department>();

        public DbSet<Ticket> Tickets => Set<Ticket>();

        public DbSet<TicketStatus> TicketStatuses => Set<TicketStatus>();

        public DbSet<TicketPriority> TicketPriorities => Set<TicketPriority>();

        public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();
    }
}
