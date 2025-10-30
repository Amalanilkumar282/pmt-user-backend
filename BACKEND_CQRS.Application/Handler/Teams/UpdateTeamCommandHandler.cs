using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Domain.Entities;
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
    public class UpdateTeamCommandHandler : IRequestHandler<UpdateTeamCommand, bool>
    {
        private readonly AppDbContext _context;

        public UpdateTeamCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateTeamCommand request, CancellationToken cancellationToken)
        {
            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (team == null)
                return false;

            var previousLeadId = team.LeadId;

            // 🔹 Update base details
            team.Name = request.Team.Name ?? team.Name;
            team.Description = request.Team.Description ?? team.Description;
            team.LeadId = request.Team.LeadId ?? team.LeadId;
            team.Label = request.Team.Label ?? team.Label;
            team.IsActive = request.Team.IsActive;
            team.UpdatedBy = request.Team.UpdatedBy;
            team.UpdatedAt = DateTime.UtcNow;

            // 🔹 Build new member list
            var updatedMemberIds = new HashSet<int>(request.Team.MemberIds ?? new List<int>());

            // ✅ Add the current lead (if any)
            if (team.LeadId.HasValue)
                updatedMemberIds.Add(team.LeadId.Value);

            // 🔹 Remove all existing members
            var existingMembers = _context.TeamMembers.Where(tm => tm.TeamId == team.Id);
            _context.TeamMembers.RemoveRange(existingMembers);

            // ✅ Add new members
            foreach (var memberId in updatedMemberIds)
            {
                _context.TeamMembers.Add(new TeamMember
                {
                    TeamId = team.Id,
                    ProjectMemberId = memberId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
