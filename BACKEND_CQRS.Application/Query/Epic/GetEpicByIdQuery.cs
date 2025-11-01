using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;

namespace BACKEND_CQRS.Application.Query.Epic
{
    public class GetEpicByIdQuery : IRequest<ApiResponse<EpicDto>>
    {
        public Guid EpicId { get; }

        public GetEpicByIdQuery(Guid epicId)
        {
            EpicId = epicId;
        }
    }
}
