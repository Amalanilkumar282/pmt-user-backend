using BACKEND_CQRS.Domain.Entities;

namespace BACKEND_CQRS.Domain.Persistance
{
    public interface IBoardRepository : IGenericRepository<Board>
    {
        /// <summary>
        /// Fetch all boards for a given project with their associated columns
        /// </summary>
        Task<List<Board>> GetBoardsByProjectIdWithColumnsAsync(Guid projectId);

        /// <summary>
        /// Check if a board exists and is active
        /// </summary>
        Task<bool> BoardExistsAsync(int boardId);

        /// <summary>
        /// Get a board by ID (including inactive boards)
        /// </summary>
        Task<Board?> GetBoardByIdAsync(int boardId);

        /// <summary>
        /// Get all columns for a specific board ordered by position
        /// </summary>
        Task<List<BoardColumn>> GetBoardColumnsAsync(int boardId);

        /// <summary>
        /// Shift positions of columns when inserting a new column at a specific position
        /// </summary>
        Task ShiftColumnPositionsAsync(int boardId, int fromPosition);

        /// <summary>
        /// Create a new board column and map it to the board
        /// </summary>
        Task<BoardColumn> CreateBoardColumnAsync(int boardId, BoardColumn boardColumn);

        /// <summary>
        /// Soft-delete a board by setting IsActive to false
        /// </summary>
        Task<bool> SoftDeleteBoardAsync(int boardId, int? deletedBy = null);
    }
}
