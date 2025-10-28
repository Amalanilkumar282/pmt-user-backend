using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Entities
{

    [Table("notification")]
    public class Notification
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("recipient_id")]
        public int RecipientId { get; set; }

        [Column("actor_id")]
        public int? ActorId { get; set; }

        [Column("activity_id")]
        public Guid? ActivityId { get; set; }

        [Required]
        [Column("message")]
        public string Message { get; set; }

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Column("read_at")]
        public DateTimeOffset? ReadAt { get; set; }

        // Navigation properties
        [ForeignKey("RecipientId")]
        public Users Recipient { get; set; }

        [ForeignKey("ActorId")]
        public Users? Actor { get; set; }

        [ForeignKey("ActivityId")]
        public ActivityLog? Activity { get; set; }
    }

}
