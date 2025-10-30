using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Entities
{
    [Table("epics")]
    public class Epic
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("project_id")]
        public Guid? ProjectId { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Column("assignee_id")]
        public int? AssigneeId { get; set; }

        [Column("reporter_id")]
        public int? ReporterId { get; set; }

        [Column("labels")]
        public List<string>? Labels { get; set; } = new();


        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Column("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        [ForeignKey("ProjectId")]
        public Projects Project { get; set; }

        [ForeignKey("AssigneeId")]
        public Users? Assignee { get; set; }

        [ForeignKey("ReporterId")]
        public Users? Reporter { get; set; }

        [ForeignKey("CreatedBy")]
        public Users? Creator { get; set; }

        [ForeignKey("UpdatedBy")]
        public Users? Updater { get; set; }

        public ICollection<Issue> Issues { get; set; }
    }

}
