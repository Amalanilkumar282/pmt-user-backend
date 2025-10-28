using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Entities
{

    [Table("jira_authorizations")]
    public class JiraAuthorization
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("project_id")]
        public Guid? ProjectId { get; set; }

        [MaxLength(500)]
        [Column("base_url")]
        public string? BaseUrl { get; set; }

        [MaxLength(2000)]
        [Column("access_token")]
        public string? AccessToken { get; set; }

        [MaxLength(2000)]
        [Column("refresh_token")]
        public string? RefreshToken { get; set; }

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        [MaxLength(500)]
        [Column("scopes")]
        public string? Scopes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public Users? User { get; set; }

        [ForeignKey("ProjectId")]
        public Projects? Project { get; set; }
    }
}
