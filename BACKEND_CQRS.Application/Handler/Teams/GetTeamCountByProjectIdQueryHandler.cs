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
    public class GetTeamCountByProjectIdQueryHandler : IRequestHandler<GetTeamCountByProjectIdQuery, int>
    {
        private readonly ITeamRepository _teamRepository;

        public GetTeamCountByProjectIdQueryHandler(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<int> Handle(GetTeamCountByProjectIdQuery request, CancellationToken cancellationToken)
        {
            return await _teamRepository.GetTeamCountByProjectIdAsync(request.ProjectId);
        }
    }
}
