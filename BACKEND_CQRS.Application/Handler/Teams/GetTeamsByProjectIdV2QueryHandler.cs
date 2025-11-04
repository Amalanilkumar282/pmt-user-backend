using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Teams;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Teams
{
    /// <summary>
    /// V2 handler that properly handles nullable emails for team members
    /// Fixes: System.InvalidCastException when email is NULL in database
    /// </summary>
    public class GetTeamsByProjectIdV2QueryHandler
        : IRequestHandler<GetTeamsByProjectIdV2Query, List<TeamDetailsV2Dto>>
    {
        private readonly AppDbContext _context;

        public GetTeamsByProjectIdV2QueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TeamDetailsV2Dto>> Handle(GetTeamsByProjectIdV2Query request, CancellationToken cancellationToken)
        {
            // Get all active teams for the project
            var teams = await _context.Teams
                .AsNoTracking()
                .Where(t => t.ProjectId == request.ProjectId && t.IsActive == true)
                .Select(t => new
                {
                    t.Id,
                    t.Name
                })
                .ToListAsync(cancellationToken);

            if (teams.Count == 0)
            {
                return new List<TeamDetailsV2Dto>();
            }

            var teamDetailsList = new List<TeamDetailsV2Dto>();

            foreach (var team in teams)
            {
                // Use LINQ with proper nullable handling instead of raw SQL
                var members = await (
                    from tm in _context.TeamMembers
                    join pm in _context.ProjectMembers on tm.ProjectMemberId equals pm.Id
                    join u in _context.Users on pm.UserId equals u.Id
                    join r in _context.Roles on pm.RoleId equals r.Id
                    where tm.TeamId == team.Id
                    select new TeamDetailsV2Dto.TeamMemberV2Dto
                    {
                        Id = u.Id,
                        Name = u.Name ?? "Unknown",
                        Email = u.Email, // Properly handles NULL
                        Role = r.Name ?? "Member"
                    }
                ).ToListAsync(cancellationToken);

                teamDetailsList.Add(new TeamDetailsV2Dto
                {
                    Id = team.Id,
                    Name = team.Name,
                    Members = members
                });
            }

            return teamDetailsList;
        }
    }
}
