using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Command
{
    public class UpdateIssueCommentCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid Id { get; set; }
        public string Body { get; set; }
        public int UpdatedBy { get; set; }
        public List<int> MentionedUserIds { get; set; } = new List<int>();
    }
}
