using BACKEND_CQRS.Domain.Entities;

namespace BACKEND_CQRS.Domain.Persistance
{
    public interface IStatusRepository : IGenericRepository<Status>
    {
        /// <summary>
        /// Find status by name (case-insensitive)
        /// </summary>
        Task<Status?> GetStatusByNameAsync(string statusName);

        /// <summary>
        /// Get status by ID
        /// </summary>
        Task<Status?> GetStatusByIdAsync(int statusId);

        /// <summary>
        /// Create a new status
        /// </summary>
        Task<Status> CreateStatusAsync(string statusName);
    }
}
