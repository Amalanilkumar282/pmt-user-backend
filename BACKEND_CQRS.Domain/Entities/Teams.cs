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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("project_id")]
        public Guid ProjectId { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        // 🔹 LeadId now references ProjectMembers.Id
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

        [Column("Label")]
        public List<string>? Label { get; set; }

        // 🔹 Lead is now a ProjectMember, not a User
        [ForeignKey("LeadId")]
        public ProjectMembers? Lead { get; set; }

        // ✅ These remain linked to the Users table
        [ForeignKey("CreatedBy")]
        public ProjectMembers? CreatedByMember { get; set; }


        [ForeignKey("UpdatedBy")]
        public ProjectMembers? UpdatedByMember { get; set; }

        [ForeignKey("ProjectId")]
        public Projects? Project { get; set; }

        [NotMapped]
        public int MemberCount { get; set; }

        [NotMapped]
        public int ActiveSprintCount { get; set; }

         //✅ Add this navigation property
        public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();


        public ICollection<Board> Boards { get; set; }
        public ICollection<ProjectMembers> ProjectMembers { get; set; }
        public ICollection<Channel> Channels { get; set; }
    }
}


