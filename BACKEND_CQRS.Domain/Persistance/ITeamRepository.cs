using BACKEND_CQRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Persistance
{
    public interface ITeamRepository : IGenericRepository<Teams>
    {
        // Custom query methods specific to Teams
        Task<List<Teams>> GetTeamsByProjectIdAsync(Guid projectId);
        //Task<List<Team>> GetTeamsByUserIdAsync(int userId);
    }
}
