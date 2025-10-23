using BACKEND_CQRS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Persistance
{
    public interface IProjectRepository : IGenericRepository<Projects>
    {
       
    }
}
