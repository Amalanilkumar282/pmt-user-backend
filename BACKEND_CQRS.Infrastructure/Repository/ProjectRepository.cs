using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Repository;
using BACKEND_CQRS.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Infrastructure.Repository
{

    public class ProjectRepository : GenericRepository<Projects>, IProjectRepository
    {
        private readonly AppDbContext _dbContext;

        public ProjectRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

    }
}

