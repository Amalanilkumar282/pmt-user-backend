using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Entities
{
    [Table("boards")]
    public class Board
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("project_id")]
        public Guid ProjectId { get; set; }

        [Column("team_id")]
        public int? TeamId { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [MaxLength(50)]
        [Column("type")]
        public string Type { get; set; } = "kanban";

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("metadata", TypeName = "jsonb")]
        public string? Metadata { get; set; }

        // Navigation properties
        [ForeignKey("ProjectId")]
        public Projects Project { get; set; }

        [ForeignKey("TeamId")]
        public Teams? Team { get; set; }

        [ForeignKey("CreatedBy")]
        public Users? Creator { get; set; }

        [ForeignKey("UpdatedBy")]
        public Users? Updater { get; set; }

        // Collection for board columns (not mapped to DB directly)
        [NotMapped]
        public List<BoardColumn> BoardColumns { get; set; } = new();
    }

}
