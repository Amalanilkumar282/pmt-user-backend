using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Teams;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Teams
{
    public class GetTeamsByProjectIdQueryHandler : IRequestHandler<GetTeamsByProjectIdQuery, List<TeamDto>>
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IMapper _mapper;

        public GetTeamsByProjectIdQueryHandler(ITeamRepository teamRepository, IMapper mapper)
        {
            _teamRepository = teamRepository;
            _mapper = mapper;
        }

        public async Task<List<TeamDto>> Handle(GetTeamsByProjectIdQuery request, CancellationToken cancellationToken)
        {
            var teams = await _teamRepository.GetTeamsByProjectIdAsync(request.ProjectId);
            return _mapper.Map<List<TeamDto>>(teams);
        }
    }
}
