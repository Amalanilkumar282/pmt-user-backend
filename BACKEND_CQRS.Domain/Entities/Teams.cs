using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Entities
{


    [Table("teams")]
    public class Teams
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("project_id")]
        public Guid ProjectId { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("lead_id")]
        public int? LeadId { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; } = true;

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // 👇 The only column that’s capitalized in DB — must match exactly
        [Column("Label")]
        public List<string>? Label { get; set; }

        // No "virtual" — CQRS prefers explicit loading
        [ForeignKey("LeadId")]
        public Users? Lead { get; set; }

        [ForeignKey("CreatedBy")]
        public Users? CreatedByUser { get; set; }

        [ForeignKey("UpdatedBy")]
        public Users? UpdatedByUser { get; set; }

        [ForeignKey("ProjectId")]
        public Projects? Project { get; set; }

        [NotMapped]
        public int MemberCount { get; set; }

        [NotMapped]
        public int ActiveSprintCount { get; set; }
    } 
    }


