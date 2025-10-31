using PmtAdmin.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Entities
{
    [Table("users")]
    public class Users
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("email")]
        public string Email { get; set; }

        [MaxLength(1024)]
        [Column("password_hash")]
        public string? PasswordHash { get; set; }

        [MaxLength(150)]
        [Column("name")]
        public string? Name { get; set; }

        [MaxLength(1000)]
        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; } = true;

        [Column("is_super_admin")]
        public bool? IsSuperAdmin { get; set; } = false;

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [MaxLength(1024)]
        [Column("jira_id")]
        public string? JiraId { get; set; }

        [Column("type")]
        public string? Type { get; set; }

        // Audit fields - now exist in the database
        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("deleted_by")]
        public int? DeletedBy { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        // Self-referencing navigation properties
        [ForeignKey("CreatedBy")]
        public Users? CreatedByUser { get; set; }

        [ForeignKey("DeletedBy")]
        public Users? DeletedByUser { get; set; }

        [ForeignKey("UpdatedBy")]
        public Users? UpdatedByUser { get; set; }

        public ICollection<ProjectMembers> ProjectMembers { get; set; }
        public ICollection<Projects> ManagedProjects { get; set; }
        public ICollection<Teams> LeadTeams { get; set; }
        public ICollection<DeliveryUnit> ManagedDeliveryUnits { get; set; }

        public ICollection<StarredProjects>? StarredProjects { get; set; }





    }
}
