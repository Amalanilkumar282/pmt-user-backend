using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Query
{
    public class GetUsersByProjectIdQuery : IRequest<ApiResponse<List<ProjectUserDto>>>
    {
        public Guid ProjectId { get; }

        public GetUsersByProjectIdQuery(Guid projectId)
        {
            ProjectId = projectId;
        }
    }
}
