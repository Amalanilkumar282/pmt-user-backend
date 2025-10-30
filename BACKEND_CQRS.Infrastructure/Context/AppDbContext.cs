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
        public DbSet<Role> Roles { get; set; } // ✅ Fixed: Changed from Role to Roles
        public DbSet<Label> Labels { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        public DbSet<TeamMember> TeamMembers { get; set; }

        public DbSet<Board> Boards { get; set; }
        public DbSet<BoardColumn> BoardColumns { get; set; }
        public DbSet<BoardBoardColumnMap> BoardBoardColumnMaps { get; set; }
        public DbSet<Status> Statuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Role -> Projects relationship properly
            // This tells EF Core to use project_manager_role_id, not create a phantom RoleId column
            modelBuilder.Entity<Role>()
                .HasMany(r => r.ProjectManagerRoles)
                .WithOne(p => p.ProjectManagerRole)
                .HasForeignKey(p => p.ProjectManagerRoleId)
                .IsRequired(false);

            // Configure ProjectMembers -> Role relationship
            modelBuilder.Entity<ProjectMembers>()
                .HasOne(pm => pm.Role)
                .WithMany(r => r.ProjectMembers)
                .HasForeignKey(pm => pm.RoleId)
                .IsRequired(false);
        }
    }
}
