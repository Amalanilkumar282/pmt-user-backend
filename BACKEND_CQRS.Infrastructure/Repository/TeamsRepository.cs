using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
 

namespace BACKEND_CQRS.Infrastructure.Repository
{
    public class TeamRepository : GenericRepository<Teams>, ITeamRepository
    {
        private readonly AppDbContext _context;

        public TeamRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // 🔹 CREATE TEAM
        public async Task<int> CreateTeamAsync(Teams team)
        {
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            return team.Id;
        }

        // 🔹 ADD MEMBERS (lead + selected project members)
        public async Task AddMembersAsync(int teamId, List<int> memberIds)
        {
            if (memberIds == null || memberIds.Count == 0)
                return;

            var teamMembers = memberIds.Select(memberId => new TeamMember
            {
                TeamId = teamId,
                ProjectMemberId = memberId,
                CreatedAt = DateTime.UtcNow // ✅ Prevent null constraint errors
            }).ToList();

            _context.TeamMembers.AddRange(teamMembers);
            await _context.SaveChangesAsync();
        }

        // 🔹 GET TEAMS BY PROJECT
        

        public async Task<bool> DeleteTeamAsync(int teamId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
                return false;

            // 🔹 Hard delete
            _context.Teams.Remove(team);

            // 🔹 If you prefer soft delete instead:
            // team.IsActive = false;
            // _context.Teams.Update(team);

            await _context.SaveChangesAsync();
            return true;
        }
        //public async Task<int> GetTeamCountByProjectIdAsync(Guid projectId)
        //{
        //    return await _context.Teams
        //        .CountAsync(t => t.ProjectId == projectId );
        //}


        

    }
}
