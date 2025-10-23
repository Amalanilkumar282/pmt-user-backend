using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Entities
{
    public class Users
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("email")]
        [Required]
        public string Email { get; set; }

        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("avatar_url")]
        public string AvatarUrl { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

        [Column("is_super_admin")]
        public bool? IsSuperAdmin { get; set; }

        [Column("last_login")]
        public DateTimeOffset? LastLogin { get; set; }

        [Column("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [Column("version")]
        public int? Version { get; set; }

        [Column("deleted_at")]
        public DateTimeOffset? DeletedAt { get; set; }

        [Column("jira_id")]
        public string JiraId { get; set; }

        [Column("type")]
        public string Type { get; set; }
    }
}
