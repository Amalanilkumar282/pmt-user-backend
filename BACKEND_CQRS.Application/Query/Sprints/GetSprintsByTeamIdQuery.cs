using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Query.Sprints
{
    public class GetSprintsByTeamIdQuery : IRequest<ApiResponse<List<SprintDto>>>
    {
        public int TeamId { get; set; }

        public GetSprintsByTeamIdQuery(int teamId)
        {
            TeamId = teamId;
        }
    }
}