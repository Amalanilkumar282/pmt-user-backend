using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Command
{
    public class CreateIssueCommentCommand : IRequest<ApiResponse<CreateIssueCommentDto>>
    {
        public Guid IssueId { get; set; }
        public string Body { get; set; }
        public int AuthorId { get; set; }
        public List<int> MentionedUserIds { get; set; } = new List<int>();
    }
}
