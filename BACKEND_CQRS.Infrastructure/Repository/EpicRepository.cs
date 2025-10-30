using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;


namespace BACKEND_CQRS.Infrastructure.Repository
{
    public class EpicRepository : IEpicRepository
    {
        private readonly AppDbContext _context;

        public EpicRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Epic>> GetEpicsByProjectIdAsync(Guid projectId)
        {
            return await _context.Epic
                .Include(e => e.Assignee)
                .Include(e => e.Reporter)
                .Include(e => e.Project)
                .Where(e => e.ProjectId == projectId)
                .AsNoTracking()
                .ToListAsync();
        }


    }
}
