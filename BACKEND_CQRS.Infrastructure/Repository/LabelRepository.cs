using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Infrastructure.Repository
{
    public class LabelRepository : GenericRepository<Label>, ILabelRepository
    {
        private readonly AppDbContext _context;

        public LabelRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Label> AddLabelAsync(Label label)
        {
            var result = await _context.Labels.AddAsync(label);
            await _context.SaveChangesAsync();
            return result.Entity;
        }
    }
}
