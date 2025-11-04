using System;

namespace BACKEND_CQRS.Application.Dto
{
    public class CreateIssueCommentDto
    {
        public Guid Id { get; set; }
        public Guid IssueId { get; set; }
        public string Body { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
