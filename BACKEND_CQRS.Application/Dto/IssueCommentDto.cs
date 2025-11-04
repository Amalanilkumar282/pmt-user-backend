using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Dto
{
    public class IssueCommentDto
    {
        public Guid Id { get; set; }
        public Guid IssueId { get; set; }
        public int AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorAvatarUrl { get; set; }
        public string Body { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public List<MentionDto> Mentions { get; set; } = new List<MentionDto>();
    }
}
