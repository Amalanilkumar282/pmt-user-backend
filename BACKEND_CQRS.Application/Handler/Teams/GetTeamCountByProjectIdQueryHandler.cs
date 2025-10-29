using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Teams;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handlers.TeamHandlers
{

    public class GetTeamCountByProjectIdHandler : IRequestHandler<GetTeamCountByProjectIdQuery, TeamCountDto>
    {
        private readonly ITeamRepository _teamRepository;

        public GetTeamCountByProjectIdHandler(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<TeamCountDto> Handle(GetTeamCountByProjectIdQuery request, CancellationToken cancellationToken)
        {
            var teams = await _teamRepository.GetTeamsByProjectIdAsync(request.ProjectId);

            return new TeamCountDto
            {
                TotalTeams = teams.Count,
                ActiveTeams = teams.Count(t => t.IsActive == true)
            };
        }
    }
}
