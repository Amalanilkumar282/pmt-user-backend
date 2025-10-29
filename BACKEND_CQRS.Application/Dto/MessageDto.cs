using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Dto
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid? ChannelId { get; set; }
        public string? Body { get; set; }
        public int? MentionUserId { get; set; }
        public string? MentionedUserName { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatorName { get; set; }
        public int? UpdatedBy { get; set; }
        public string? UpdaterName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
