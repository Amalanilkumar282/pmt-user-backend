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


        // 🔹 Create a new team and return the generated TeamId
        Task<int> CreateTeamAsync(Teams team);

        // 🔹 Add members (lead + selected members) to team_members table
        Task AddMembersAsync(int teamId, List<int> memberIds);

        // 🔹 Get a team by ID (with related data if needed)
        //Task<Teams?> GetTeamByIdAsync(int teamId);

        // Custom query methods specific to Teams
        Task<List<Teams>> GetTeamsByProjectIdAsync(Guid projectId);
        //Task<List<Team>> GetTeamsByUserIdAsync(int userId);


        // 🔹 Update team details (e.g., name, description, label)
        //Task UpdateTeamAsync(Teams team);

        // 🔹 Delete or deactivate a team
        Task<bool> DeleteTeamAsync(int teamId);


        Task<int> GetTeamCountByProjectIdAsync(Guid projectId);


    }
}
