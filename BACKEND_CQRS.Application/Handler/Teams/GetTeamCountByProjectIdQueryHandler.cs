using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Teams;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        private readonly AppDbContext _context; // 👈 inject context for extra query

        public GetTeamCountByProjectIdHandler(ITeamRepository teamRepository, AppDbContext context)
        {
            _teamRepository = teamRepository;
            _context = context;
        }

        public async Task<TeamCountDto> Handle(GetTeamCountByProjectIdQuery request, CancellationToken cancellationToken)
        {
            var teams = await _teamRepository.GetTeamsByProjectIdAsync(request.ProjectId);

            // 1️⃣ Count total & active teams
            var totalTeams = teams.Count;
            var activeTeams = teams.Count(t => t.IsActive == true);

            // 2️⃣ Count assigned members — using TeamMembers + ProjectMembers
            var assignedMembersCount = await _context.TeamMembers
   .Where(tm => _context.ProjectMembers
       .Any(pm => pm.Id == tm.ProjectMemberId && pm.ProjectId == request.ProjectId))
   .Select(tm => tm.ProjectMemberId)
   .Distinct()
   .CountAsync();


            // 3️⃣ Return combined DTO
            return new TeamCountDto
            {
                TotalTeams = totalTeams,
                ActiveTeams = activeTeams,
                AssignedMembersCount = assignedMembersCount
            };
        }
    }

}
