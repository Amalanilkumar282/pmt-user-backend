using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;

namespace BACKEND_CQRS.Application.Command
{
    public class DeleteIssueCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }

        public DeleteIssueCommand(Guid id)
        {
            Id = id;
        }
    }
}
