using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;

namespace BACKEND_CQRS.Application.Query.IssueComments
{
    public class GetCommentByIdQuery : IRequest<ApiResponse<IssueCommentDto>>
    {
        public Guid Id { get; set; }

        public GetCommentByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
