using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Entities
{
    [Table("channel")]
    public class Channel
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("team_id")]
        public int? TeamId { get; set; }
        [Column("channel_name")]
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("TeamId")]
        public Teams? Team { get; set; }

        public ICollection<Message> Messages { get; set; }
    }

}
