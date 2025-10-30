using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Teams;
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
    public class GetProjectMemberCountQueryHandler : IRequestHandler<GetProjectMemberCountQuery, ProjectMemberCountDto>
    {
        private readonly AppDbContext _context;

        public GetProjectMemberCountQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProjectMemberCountDto> Handle(GetProjectMemberCountQuery request, CancellationToken cancellationToken)
        {
            var projectId = request.ProjectId;

            // ✅ Total project members
            var totalMembers = await _context.ProjectMembers
                .CountAsync(pm => pm.ProjectId == projectId, cancellationToken);

            // ✅ Active project members (joined with Users)
            var activeMembers = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId)
                .Join(_context.Users,
                      pm => pm.UserId,
                      u => u.Id,
                      (pm, u) => new { u.IsActive })
                .CountAsync(x => x.IsActive == true, cancellationToken);

            // ✅ Unassigned project members (not in any team)
            var unassignedMembers = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId &&
                             !_context.TeamMembers.Any(tm => tm.ProjectMemberId == pm.Id))
                .CountAsync(cancellationToken);

            return new ProjectMemberCountDto
            {
                TotalMembers = totalMembers,
                ActiveMembers = activeMembers,
                UnassignedMembers = unassignedMembers
            };
        }
    }
}
