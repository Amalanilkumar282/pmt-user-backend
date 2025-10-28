using BACKEND_CQRS.Domain.Entities;

namespace BACKEND_CQRS.Domain.Persistance
{
    public interface IBoardRepository : IGenericRepository<Board>
    {
        /// <summary>
        /// Fetch all boards for a given project with their associated columns
        /// </summary>
        Task<List<Board>> GetBoardsByProjectIdWithColumnsAsync(Guid projectId);
    }
}
