using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;

namespace BACKEND_CQRS.Application.Command
{
    public class DeleteIssueCommentCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid Id { get; set; }

        public DeleteIssueCommentCommand(Guid id)
        {
            Id = id;
        }
    }
}
