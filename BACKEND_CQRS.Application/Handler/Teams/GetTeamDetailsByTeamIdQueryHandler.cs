using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Teams;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Teams
{


    public class GetTeamDetailsByTeamIdQueryHandler : IRequestHandler<GetTeamDetailsByTeamIdQuery, TeamDetailsDto>
    {
        private readonly AppDbContext _context;

        public GetTeamDetailsByTeamIdQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TeamDetailsDto> Handle(GetTeamDetailsByTeamIdQuery request, CancellationToken cancellationToken)
        {
            // ✅ Fetch Team WITHOUT eager loading Project
            var team = await _context.Teams
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken);

            if (team == null)
                throw new Exception($"Team with ID {request.TeamId} not found.");

             // 🔍 DEBUG: Check LeadId value
    Console.WriteLine($"Team LeadId: {team.LeadId}");

            // ✅ Fetch Project separately if needed
            string projectName = null;
            if (team.ProjectId != null && team.ProjectId != Guid.Empty)
            {
                var project = await _context.Projects
                    .AsNoTracking()
                    .Where(p => p.Id == team.ProjectId)
                    .Select(p => p.Name)
                    .FirstOrDefaultAsync(cancellationToken);

                projectName = project;
            }

            // ✅ Fetch Lead info using raw SQL
            var leadInfoSql = @"
    SELECT 
        u.name AS ""Name"",
        u.email AS ""Email"",
        r.name AS ""Role""
    FROM project_members pm
    INNER JOIN users u ON pm.user_id = u.id
    INNER JOIN roles r ON pm.role_id = r.id
    WHERE pm.id = {0}";


            TeamDetailsDto.LeadDto leadInfo = null;

            try
            {
                var leadResult = await _context.Database
                    .SqlQueryRaw<LeadDtoRaw>(leadInfoSql, team.LeadId)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);

                if (leadResult != null)
                {
                    leadInfo = new TeamDetailsDto.LeadDto
                    {
                        Name = leadResult.Name,
                        Email = leadResult.Email,
                        Role = leadResult.Role
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching lead info: {ex.Message}");
            }

            // ✅ Fetch Team Members using raw SQL
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

            var members = new List<TeamDetailsDto.TeamMemberDto>();

            try
            {
                var membersResult = await _context.Database
                    .SqlQueryRaw<TeamMemberDtoRaw>(membersSql, team.Id)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                members = membersResult.Select(m => new TeamDetailsDto.TeamMemberDto
                {
                    Name = m.Name,
                    Email = m.Email,
                    Role = m.Role
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching team members: {ex.Message}");
            }

            // ✅ Count Members
            var memberCount = members.Count;

            // ✅ Count Active and Completed Sprints
            var activeSprints = await _context.Sprints
                .CountAsync(s => s.TeamId == team.Id && s.Status == "ACTIVE", cancellationToken);

            var completedSprints = await _context.Sprints
                .CountAsync(s => s.TeamId == team.Id && s.Status == "COMPLETED", cancellationToken);

            // ✅ Build Response
            return new TeamDetailsDto
            {
                TeamId = team.Id,
                TeamName = team.Name,
                ProjectName = projectName,
                Description = team.Description,
                IsActive = team.IsActive,
                Tags = team.Label,
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt,
                Lead = leadInfo,
                Members = members,
                MemberCount = memberCount,
                ActiveSprints = activeSprints,
                CompletedSprints = completedSprints
            };
        }

        // Helper classes (add at the bottom of the file)
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

