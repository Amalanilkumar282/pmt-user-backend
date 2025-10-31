using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BACKEND_CQRS.Domain.Entities
{
    [Table("sprints")]
    public class Sprint
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("project_id")]
        public Guid? ProjectId { get; set; } // Now optional

        [Column("name")]
        [Required]
        public string Name { get; set; } 

        [Column("sprint_goal")]
        public string? SprintGoal { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Column("status")]
        public string? Status { get; set; } = "PLANNED";

        [Column("story_point")]
        public decimal? StoryPoint { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [Column("team_id")]
        public int? TeamId { get; set; } // Navigation properties


          [ForeignKey("ProjectId")]
           public Projects Project { get; set; }

          [ForeignKey("CreatedBy")]
          public Users? Creator { get; set; }

          [ForeignKey("UpdatedBy")]
           public Users? Updater { get; set; }

        [ForeignKey(nameof(TeamId))]
        public Teams? Team { get; set; }


        public ICollection<Issue> Issues { get; set; }


    }
}
