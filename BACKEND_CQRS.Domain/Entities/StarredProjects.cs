using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BACKEND_CQRS.Domain.Entities
{
    [Table("starred_projects")]
    public class StarredProjects
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("project_id")]
        public Guid ProjectId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Column("updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("ProjectId")]
        public Projects? Project { get; set; }

        [ForeignKey("UserId")]
        public Users? User { get; set; }
    }
}
