using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Entities
{

    [Table("messeges")]
    public class Message
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("channel_id")]
        public Guid? ChannelId { get; set; }

        [Column("body")]
        public string? Body { get; set; }

        [Column("mention_user_id")]
        public int? MentionUserId { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Column("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        [ForeignKey("ChannelId")]
        public Channel? Channel { get; set; }

        [ForeignKey("MentionUserId")]
        public Users? MentionedUser { get; set; }

        [ForeignKey("CreatedBy")]
        public Users? Creator { get; set; }

        [ForeignKey("UpdatedBy")]
        public Users? Updater { get; set; }
    }

}
