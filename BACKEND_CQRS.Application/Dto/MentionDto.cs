using System;

namespace BACKEND_CQRS.Application.Dto
{
    public class MentionDto
    {
        public Guid Id { get; set; }
        public int MentionUserId { get; set; }
        public string? MentionUserName { get; set; }
        public string? MentionUserEmail { get; set; }
    }
}
