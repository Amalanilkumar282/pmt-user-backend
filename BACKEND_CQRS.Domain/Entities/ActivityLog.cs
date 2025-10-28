using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Entities
{
    [Table("activity_log")]
    public class ActivityLog
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("entity_type")]
        public string EntityType { get; set; }

        [Required]
        [Column("entity_id")]
        public Guid EntityId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("action")]
        public string Action { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public Users? User { get; set; }

        public ICollection<Notification> Notifications { get; set; }
    }

}
