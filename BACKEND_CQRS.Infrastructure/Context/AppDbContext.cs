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
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        public DbSet<TeamMember> TeamMembers { get; set; }

        public DbSet<Board> Boards { get; set; }
        public DbSet<BoardColumn> BoardColumns { get; set; }
        public DbSet<BoardBoardColumnMap> BoardBoardColumnMaps { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; } // ✅ Added RefreshToken

        public DbSet<Epic> Epic { get; set; }   

        public DbSet<Role> Roles { get; set; }
        
        public DbSet<Mention> Mentions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Explicitly configure ProjectMembers -> Users relationships to avoid ambiguity
            modelBuilder.Entity<ProjectMembers>(entity =>
            {
                // The navigation 'User' (FK: UserId) maps to Users.ProjectMembers collection
                entity.HasOne(pm => pm.User)
                      .WithMany(u => u.ProjectMembers)
                      .HasForeignKey(pm => pm.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // The navigation 'AddedByUser' (FK: AddedBy) is a separate relationship to Users (no inverse collection)
                entity.HasOne(pm => pm.AddedByUser)
                      .WithMany()
                      .HasForeignKey(pm => pm.AddedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Users self-referencing relationships
            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasOne(u => u.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(u => u.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.DeletedByUser)
                      .WithMany()
                      .HasForeignKey(u => u.DeletedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.UpdatedByUser)
                      .WithMany()
                      .HasForeignKey(u => u.UpdatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            //// Configure Projects relationships with Users
            //modelBuilder.Entity<Projects>(entity =>
            //{
            //    // ProjectManager relationship
            //    entity.HasOne(p => p.ProjectManager)
            //          .WithMany(u => u.ManagedProjects)
            //          .HasForeignKey(p => p.ProjectManagerId)
            //          .OnDelete(DeleteBehavior.Restrict);

            //    // Creator relationship (no inverse collection)
            //    entity.HasOne(p => p.Creator)
            //          .WithMany()
            //          .HasForeignKey(p => p.CreatedBy)
            //          .OnDelete(DeleteBehavior.Restrict);

            //    // Updater relationship (no inverse collection)
            //    entity.HasOne(p => p.Updater)
            //          .WithMany()
            //          .HasForeignKey(p => p.UpdatedBy)
            //          .OnDelete(DeleteBehavior.Restrict);
            //});

            modelBuilder.Entity<Projects>(entity =>
            {
                // ProjectManager relationship
                entity.HasOne(p => p.ProjectManager)
                      .WithMany(u => u.ManagedProjects)
                      .HasForeignKey(p => p.ProjectManagerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Creator relationship (no inverse collection)
                entity.HasOne(p => p.Creator)
                      .WithMany()
                      .HasForeignKey(p => p.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                // Updater relationship (no inverse collection)
                entity.HasOne(p => p.Updater)
                      .WithMany()
                      .HasForeignKey(p => p.UpdatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                // ✅ Explicitly configure Project → Teams relationship
                entity.HasMany(p => p.Teams)
                      .WithOne(t => t.Project)
                      .HasForeignKey(t => t.ProjectId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            // Configure Epic relationships with Users
            modelBuilder.Entity<Epic>(entity =>
            {
                // Assignee relationship
                entity.HasOne(e => e.Assignee)
                      .WithMany()
                      .HasForeignKey(e => e.AssigneeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Reporter relationship
                entity.HasOne(e => e.Reporter)
                      .WithMany()
                      .HasForeignKey(e => e.ReporterId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Creator relationship
                entity.HasOne(e => e.Creator)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                // Updater relationship
                entity.HasOne(e => e.Updater)
                      .WithMany()
                      .HasForeignKey(e => e.UpdatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Board relationships with Users
            modelBuilder.Entity<Board>(entity =>
            {
                // Creator relationship
                entity.HasOne(b => b.Creator)
                      .WithMany()
                      .HasForeignKey(b => b.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                // Updater relationship
                entity.HasOne(b => b.Updater)
                      .WithMany()
                      .HasForeignKey(b => b.UpdatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Issue relationships with Users
            modelBuilder.Entity<Issue>(entity =>
            {
                // Assignee relationship
                entity.HasOne(i => i.Assignee)
                      .WithMany()
                      .HasForeignKey(i => i.AssigneeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Reporter relationship
                entity.HasOne(i => i.Reporter)
                      .WithMany()
                      .HasForeignKey(i => i.ReporterId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Creator relationship
                entity.HasOne(i => i.Creator)
                      .WithMany()
                      .HasForeignKey(i => i.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                // Updater relationship
                entity.HasOne(i => i.Updater)
                      .WithMany()
                      .HasForeignKey(i => i.UpdatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Teams relationships with ProjectMembers
            modelBuilder.Entity<Teams>(entity =>
            {
                // Lead relationship
                entity.HasOne(t => t.Lead)
                      .WithMany()
                      .HasForeignKey(t => t.LeadId)
                      .OnDelete(DeleteBehavior.Restrict);

                // CreatedByMember relationship
                entity.HasOne(t => t.CreatedByMember)
                      .WithMany()
                      .HasForeignKey(t => t.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                // UpdatedByMember relationship
                entity.HasOne(t => t.UpdatedByMember)
                      .WithMany()
                      .HasForeignKey(t => t.UpdatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // CRITICAL FIX: Ignore the ProjectMembers collection navigation property
                // This prevents EF Core from inferring a TeamsId shadow property
                entity.Ignore(t => t.ProjectMembers);
            });

            // Configure Mention relationships with Users
            modelBuilder.Entity<Mention>(entity =>
            {
                // MentionedUser relationship
                entity.HasOne(m => m.MentionedUser)
                      .WithMany()
                      .HasForeignKey(m => m.MentionUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Creator relationship
                entity.HasOne(m => m.Creator)
                      .WithMany()
                      .HasForeignKey(m => m.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                // Updater relationship
                entity.HasOne(m => m.Updater)
                      .WithMany()
                      .HasForeignKey(m => m.UpdatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Message relationships with Users
            modelBuilder.Entity<Message>(entity =>
            {
                // MentionedUser relationship
                entity.HasOne(msg => msg.MentionedUser)
                      .WithMany()
                      .HasForeignKey(msg => msg.MentionUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Creator relationship
                entity.HasOne(msg => msg.Creator)
                      .WithMany()
                      .HasForeignKey(msg => msg.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                // Updater relationship
                entity.HasOne(msg => msg.Updater)
                      .WithMany()
                      .HasForeignKey(msg => msg.UpdatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // If other ambiguous navigations appear, they can be explicitly configured here similarly.
        }
    }
}
