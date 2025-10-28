using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Infrastructure.Repository
{

    public class IssueRepository : GenericRepository<Issue>, IIssueRepository
    {
        private readonly AppDbContext _dbContext;

        public IssueRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

    }
}
