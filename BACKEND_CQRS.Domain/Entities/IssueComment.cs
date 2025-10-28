using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace BACKEND_CQRS.Domain.Entities
{
    [Table("issue_comments")]
    public class IssueComment
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("issue_id")]
        public Guid? IssueId { get; set; }

        [Required]
        [Column("author_id")]
        public int AuthorId { get; set; }

        [Required]
        [Column("mention_id")]
        public int MentionId { get; set; }

        [Required]
        [Column("body")]
        public string Body { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Column("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        [ForeignKey("IssueId")]
        public Issue Issue { get; set; }

        [ForeignKey("AuthorId")]
        public Users Author { get; set; }

        [ForeignKey("MentionId")]
        public Users MentionedUser { get; set; }

        [ForeignKey("CreatedBy")]
        public Users? Creator { get; set; }

        [ForeignKey("UpdatedBy")]
        public Users? Updater { get; set; }

        public ICollection<Mention> Mentions { get; set; }
    }
}
