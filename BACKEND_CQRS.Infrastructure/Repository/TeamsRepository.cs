using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<List<Teams>> GetTeamsByProjectIdAsync(Guid projectId)
        {
            return await _context.Teams
                .Include(t => t.Project)
                .Include(t => t.Lead)
                .Where(t => t.ProjectId == projectId && (t.IsActive ?? true))
                .Select(t => new Teams
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    Name = t.Name,
                    Description = t.Description,
                    LeadId = t.LeadId,
                    IsActive = t.IsActive,
                    Label = t.Label,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    Project = t.Project,
                    Lead = t.Lead,

                    // 👇 Number of users in each team (from projectmembers)
                    MemberCount = _context.ProjectMembers
                        .Count(pm => pm.TeamId == t.Id && pm.ProjectId == projectId),

                    // 👇 Number of *active* sprints in each team (from sprints)
                    ActiveSprintCount = _context.Sprints
                        .Count(s => s.TeamId == t.Id && s.ProjectId == projectId && s.Status == "ACTIVE")
                })
                .AsNoTracking()
                .ToListAsync();
        }


        //public async Task<List<Team>> GetTeamsByUserIdAsync(int userId)
        //{
        //    return await _context.Teams
        //        .AsNoTracking()
        //        .Where(t => t.LeadId == userId
        //                 || t.CreatedBy == userId
        //                 || t.UpdatedBy == userId)
        //        .ToListAsync();
        //}
    }


}
