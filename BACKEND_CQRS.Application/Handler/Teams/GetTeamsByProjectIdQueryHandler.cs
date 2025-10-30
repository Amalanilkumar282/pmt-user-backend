using AutoMapper;
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

namespace BACKEND_CQRS.Application.Handler.Teams
{
    public class GetTeamsByProjectIdQueryHandler
       : IRequestHandler<GetTeamsByProjectIdQuery, List<TeamDetailsDto>>
    {
        private readonly AppDbContext _context;

        public GetTeamsByProjectIdQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TeamDetailsDto>> Handle(GetTeamsByProjectIdQuery request, CancellationToken cancellationToken)
        {
            var teams = await _context.Teams
                .AsNoTracking()
                .Where(t => t.ProjectId == request.ProjectId)
                .ToListAsync(cancellationToken);

            if (teams.Count == 0)
                throw new Exception($"No teams found for Project ID: {request.ProjectId}");

            var projectName = await _context.Projects
                .Where(p => p.Id == request.ProjectId)
                .Select(p => p.Name)
                .FirstOrDefaultAsync(cancellationToken);

            var teamDetailsList = new List<TeamDetailsDto>();

            foreach (var team in teams)
            {
                // ✅ Get Lead Info
                TeamDetailsDto.LeadDto leadInfo = null;
                if (team.LeadId != null)
                {
                    var leadSql = @"
                        SELECT 
                            u.name AS ""Name"",
                            u.email AS ""Email"",
                            r.name AS ""Role""
                        FROM project_members pm
                        INNER JOIN users u ON pm.user_id = u.id
                        INNER JOIN roles r ON pm.role_id = r.id
                        WHERE pm.id = {0}";

                    var lead = await _context.Database
                        .SqlQueryRaw<LeadDtoRaw>(leadSql, team.LeadId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (lead != null)
                    {
                        leadInfo = new TeamDetailsDto.LeadDto
                        {
                            Name = lead.Name,
                            Email = lead.Email,
                            Role = lead.Role
                        };
                    }
                }

                // ✅ Get Members
                var membersSql = @"
                    SELECT 
                        u.name as Name,
                        u.email as Email,
                        r.name as Role
                    FROM team_members tm
                    INNER JOIN project_members pm ON tm.project_member_id = pm.id
                    INNER JOIN users u ON pm.user_id = u.id
                    INNER JOIN roles r ON pm.role_id = r.id
                    WHERE tm.team_id = {0}";

                var memberResults = await _context.Database
                    .SqlQueryRaw<TeamMemberDtoRaw>(membersSql, team.Id)
                    .ToListAsync(cancellationToken);

                var members = memberResults.Select(m => new TeamDetailsDto.TeamMemberDto
                {
                    Name = m.Name,
                    Email = m.Email,
                    Role = m.Role
                }).ToList();

                // ✅ Sprint counts
                var activeSprints = await _context.Sprints
                    .CountAsync(s => s.TeamId == team.Id && s.Status == "ACTIVE", cancellationToken);

                var completedSprints = await _context.Sprints
                    .CountAsync(s => s.TeamId == team.Id && s.Status == "COMPLETED", cancellationToken);

                teamDetailsList.Add(new TeamDetailsDto
                {
                    TeamName = team.Name,
                    ProjectName = projectName,
                    Description = team.Description,
                    IsActive = team.IsActive,
                    Tags = team.Label,
                    CreatedAt = team.CreatedAt,
                    UpdatedAt = team.UpdatedAt,
                    Lead = leadInfo,
                    Members = members,
                    MemberCount = members.Count,
                    ActiveSprints = activeSprints,
                    CompletedSprints = completedSprints
                });
            }

            return teamDetailsList;
        }

        public class LeadDtoRaw
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
        }

        public class TeamMemberDtoRaw
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
        }
    }
}
