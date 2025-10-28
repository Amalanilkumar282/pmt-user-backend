using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Entities
{
    [Table("audit_logs")]
    public class AuditLog
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [MaxLength(200)]
        [Column("action")]
        public string? Action { get; set; }

        [MaxLength(100)]
        [Column("entity_type")]
        public string? EntityType { get; set; }

        [Column("entity_id")]
        public int? EntityId { get; set; }

        [Column("details", TypeName = "jsonb")]
        public string? Details { get; set; }

        [MaxLength(50)]
        [Column("ip_address")]
        public string? IpAddress { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public Users? User { get; set; }
    }

}
