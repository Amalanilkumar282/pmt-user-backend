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
        private readonly AppDbContext _context;

        public GetTeamCountByProjectIdHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TeamCountDto> Handle(GetTeamCountByProjectIdQuery request, CancellationToken cancellationToken)
        {
            // 1️⃣ Get all teams in the given project
            var teams = await _context.Teams
                .AsNoTracking()
                .Where(t => t.ProjectId == request.ProjectId)
                .ToListAsync(cancellationToken);

            // 2️⃣ Count total & active teams
            var totalTeams = teams.Count;
            var activeTeams = teams.Count(t => t.IsActive == true);

            // 3️⃣ Count unique assigned project members
            var assignedMembersCount = await _context.TeamMembers
                .Where(tm => _context.ProjectMembers
                    .Any(pm => pm.Id == tm.ProjectMemberId && pm.ProjectId == request.ProjectId))
                .Select(tm => tm.ProjectMemberId)
                .Distinct()
                .CountAsync(cancellationToken);

            // 4️⃣ Build and return DTO
            return new TeamCountDto
            {
                TotalTeams = totalTeams,
                ActiveTeams = activeTeams,
                AssignedMembersCount = assignedMembersCount
            };
        }
    }

}
