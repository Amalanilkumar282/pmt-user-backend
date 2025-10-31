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
        /// Get a single board by ID with all related data (project, team, columns, etc.)
        /// </summary>
        Task<Board?> GetBoardByIdWithColumnsAsync(int boardId, bool includeInactive = false);

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
        /// Get a specific board column by ID
        /// </summary>
        Task<BoardColumn?> GetBoardColumnByIdAsync(Guid columnId);

        /// <summary>
        /// Shift positions of columns when inserting a new column at a specific position
        /// </summary>
        Task ShiftColumnPositionsAsync(int boardId, int fromPosition);

        /// <summary>
        /// Reorder columns after deleting a column at a specific position
        /// </summary>
        Task ReorderColumnPositionsAfterDeleteAsync(int boardId, int deletedPosition);

        /// <summary>
        /// Create a new board column and map it to the board
        /// </summary>
        Task<BoardColumn> CreateBoardColumnAsync(int boardId, BoardColumn boardColumn);

        /// <summary>
        /// Delete a board column and its mapping
        /// </summary>
        Task<bool> DeleteBoardColumnAsync(Guid columnId, int boardId);

        /// <summary>
        /// Soft-delete a board by setting IsActive to false
        /// </summary>
        Task<bool> SoftDeleteBoardAsync(int boardId, int? deletedBy = null);

        /// <summary>
        /// Update a board's properties
        /// </summary>
        Task<Board> UpdateBoardAsync(int boardId, Board updatedBoard);

        /// <summary>
        /// Update a board column's properties
        /// </summary>
        Task<BoardColumn> UpdateBoardColumnAsync(Guid columnId, BoardColumn updatedColumn);

        /// <summary>
        /// Shift column positions when moving a column from one position to another
        /// </summary>
        Task ShiftColumnPositionsForMoveAsync(int boardId, int oldPosition, int newPosition);
    }
}
