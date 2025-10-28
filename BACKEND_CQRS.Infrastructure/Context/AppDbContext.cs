using BACKEND_CQRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BACKEND_CQRS.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ✅ DbSets for your entities
        public DbSet<Users> Users { get; set; }
        public DbSet<Projects> Projects { get; set; }
        public DbSet<ProjectMembers> ProjectMembers { get; set; } // ✅ Added ProjectMember
        public DbSet<Issue> Issues { get; set; } // Changed to singular Issue
        public DbSet<Sprint> Sprints { get; set; } // ✅ Added Sprint entity

        public DbSet<Teams> Teams { get; set; }
        public DbSet<Label> Labels { get; set; }

        public DbSet<TeamMember> TeamMembers { get; set; }

    }
}
