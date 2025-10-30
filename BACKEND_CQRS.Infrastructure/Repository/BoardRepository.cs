using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Infrastructure.Repository
{
    public class BoardRepository : GenericRepository<Board>, IBoardRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BoardRepository>? _logger;

        public BoardRepository(AppDbContext context, ILogger<BoardRepository>? logger = null) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        public async Task<List<Board>> GetBoardsByProjectIdWithColumnsAsync(Guid projectId)
        {
            try
            {
                _logger?.LogInformation("Fetching boards for project {ProjectId}", projectId);

                // Step 1: Validate that the project exists
                var projectExists = await _context.Projects
                    .AnyAsync(p => p.Id == projectId);

                if (!projectExists)
                {
                    _logger?.LogWarning("Project {ProjectId} does not exist", projectId);
                    throw new KeyNotFoundException($"Project with ID {projectId} does not exist");
                }

                // Step 2: Fetch boards with navigation properties using a single efficient query
                // This avoids the N+1 problem by using a single query with joins
                var boards = await _context.Boards
                    .Where(b => b.ProjectId == projectId && b.IsActive)
                    .Include(b => b.Project)
                    .Include(b => b.Team)
                    .Include(b => b.Creator)
                    .Include(b => b.Updater)
                    .AsNoTracking()
                    .ToListAsync();

                if (boards == null || boards.Count == 0)
                {
                    _logger?.LogInformation("No active boards found for project {ProjectId}", projectId);
                    return new List<Board>();
                }

                // Step 3: Get all board IDs to fetch columns in a single query
                var boardIds = boards.Select(b => b.Id).ToList();

                // Step 4: Fetch all board-column mappings with columns in ONE query
                var boardColumnMappings = await _context.BoardBoardColumnMaps
                    .Where(m => boardIds.Contains(m.BoardId!.Value))
                    .Include(m => m.BoardColumn)
                        .ThenInclude(bc => bc!.Status)
                    .AsNoTracking()
                    .ToListAsync();

                // Step 5: Group columns by board ID and assign to boards
                var columnsGroupedByBoard = boardColumnMappings
                    .Where(m => m.BoardColumn != null) // Filter out null columns
                    .GroupBy(m => m.BoardId)
                    .ToDictionary(
                        g => g.Key!.Value,
                        g => g.Select(m => m.BoardColumn!)
                              .OrderBy(bc => bc.Position ?? int.MaxValue) // Handle null positions
                              .ToList()
                    );

                // Step 6: Assign columns to each board
                foreach (var board in boards)
                {
                    board.BoardColumns = columnsGroupedByBoard.TryGetValue(board.Id, out var columns)
                        ? columns
                        : new List<BoardColumn>();
                }

                _logger?.LogInformation("Successfully fetched {BoardCount} boards with columns for project {ProjectId}", 
                    boards.Count, projectId);

                return boards;
            }
            catch (KeyNotFoundException)
            {
                // Re-throw domain exceptions without wrapping
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching boards for project {ProjectId}", projectId);
                throw new InvalidOperationException(
                    $"An error occurred while fetching boards for project {projectId}. See inner exception for details.", 
                    ex);
            }
        }

        public async Task<bool> BoardExistsAsync(int boardId)
        {
            try
            {
                return await _context.Boards.AnyAsync(b => b.Id == boardId && b.IsActive);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking if board {BoardId} exists", boardId);
                throw new InvalidOperationException($"An error occurred while checking board existence", ex);
            }
        }

        public async Task<Board?> GetBoardByIdAsync(int boardId)
        {
            try
            {
                _logger?.LogInformation("Fetching board {BoardId}", boardId);

                var board = await _context.Boards
                    .Include(b => b.Project)
                    .Include(b => b.Team)
                    .Include(b => b.Creator)
                    .Include(b => b.Updater)
                    .FirstOrDefaultAsync(b => b.Id == boardId);

                if (board != null)
                {
                    _logger?.LogInformation("Found board {BoardId}, IsActive: {IsActive}", boardId, board.IsActive);
                }
                else
                {
                    _logger?.LogWarning("Board {BoardId} not found", boardId);
                }

                return board;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching board {BoardId}", boardId);
                throw new InvalidOperationException($"An error occurred while fetching board", ex);
            }
        }

        public async Task<bool> SoftDeleteBoardAsync(int boardId, int? deletedBy = null)
        {
            try
            {
                _logger?.LogInformation("Soft-deleting board {BoardId}", boardId);

                var board = await _context.Boards.FindAsync(boardId);

                if (board == null)
                {
                    _logger?.LogWarning("Board {BoardId} not found for deletion", boardId);
                    return false;
                }

                if (!board.IsActive)
                {
                    _logger?.LogWarning("Board {BoardId} is already inactive", boardId);
                    return false;
                }

                // Soft delete: set IsActive to false
                board.IsActive = false;
                board.UpdatedAt = DateTime.UtcNow;
                
                if (deletedBy.HasValue)
                {
                    board.UpdatedBy = deletedBy.Value;
                }

                await _context.SaveChangesAsync();

                _logger?.LogInformation("Successfully soft-deleted board {BoardId}", boardId);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error soft-deleting board {BoardId}", boardId);
                throw new InvalidOperationException($"An error occurred while deleting board", ex);
            }
        }

        public async Task<List<BoardColumn>> GetBoardColumnsAsync(int boardId)
        {
            try
            {
                _logger?.LogInformation("Fetching columns for board {BoardId}", boardId);

                var columns = await _context.BoardBoardColumnMaps
                    .Where(m => m.BoardId == boardId)
                    .Include(m => m.BoardColumn)
                        .ThenInclude(bc => bc!.Status)
                    .Select(m => m.BoardColumn!)
                    .Where(bc => bc != null)
                    .OrderBy(bc => bc.Position ?? int.MaxValue)
                    .ToListAsync();

                _logger?.LogInformation("Found {ColumnCount} columns for board {BoardId}", columns.Count, boardId);

                return columns;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching columns for board {BoardId}", boardId);
                throw new InvalidOperationException($"An error occurred while fetching board columns", ex);
            }
        }

        public async Task<BoardColumn?> GetBoardColumnByIdAsync(Guid columnId)
        {
            try
            {
                _logger?.LogInformation("Fetching board column {ColumnId}", columnId);

                var column = await _context.BoardColumns
                    .Include(bc => bc.Status)
                    .FirstOrDefaultAsync(bc => bc.Id == columnId);

                if (column != null)
                {
                    _logger?.LogInformation("Found board column {ColumnId}: {ColumnName}", columnId, column.BoardColumnName);
                }
                else
                {
                    _logger?.LogWarning("Board column {ColumnId} not found", columnId);
                }

                return column;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching board column {ColumnId}", columnId);
                throw new InvalidOperationException($"An error occurred while fetching board column", ex);
            }
        }

        public async Task ShiftColumnPositionsAsync(int boardId, int fromPosition)
        {
            try
            {
                _logger?.LogInformation("Shifting column positions for board {BoardId} from position {Position}", 
                    boardId, fromPosition);

                // Get all columns that need to be shifted (position >= fromPosition)
                var columnsToShift = await _context.BoardBoardColumnMaps
                    .Where(m => m.BoardId == boardId)
                    .Include(m => m.BoardColumn)
                    .Where(m => m.BoardColumn!.Position >= fromPosition)
                    .Select(m => m.BoardColumn!)
                    .ToListAsync();

                // Shift each column's position by 1
                foreach (var column in columnsToShift)
                {
                    column.Position = (column.Position ?? 0) + 1;
                }

                await _context.SaveChangesAsync();

                _logger?.LogInformation("Successfully shifted {Count} columns for board {BoardId}", 
                    columnsToShift.Count, boardId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error shifting column positions for board {BoardId}", boardId);
                throw new InvalidOperationException($"An error occurred while shifting column positions", ex);
            }
        }

        public async Task ReorderColumnPositionsAfterDeleteAsync(int boardId, int deletedPosition)
        {
            try
            {
                _logger?.LogInformation("Reordering columns for board {BoardId} after deletion at position {Position}", 
                    boardId, deletedPosition);

                // Get all columns with position greater than the deleted position
                var columnsToReorder = await _context.BoardBoardColumnMaps
                    .Where(m => m.BoardId == boardId)
                    .Include(m => m.BoardColumn)
                    .Where(m => m.BoardColumn!.Position > deletedPosition)
                    .Select(m => m.BoardColumn!)
                    .ToListAsync();

                // Decrease each column's position by 1 to fill the gap
                foreach (var column in columnsToReorder)
                {
                    column.Position = (column.Position ?? 0) - 1;
                }

                await _context.SaveChangesAsync();

                _logger?.LogInformation("Successfully reordered {Count} columns for board {BoardId}", 
                    columnsToReorder.Count, boardId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error reordering column positions for board {BoardId}", boardId);
                throw new InvalidOperationException($"An error occurred while reordering column positions", ex);
            }
        }

        public async Task<bool> DeleteBoardColumnAsync(Guid columnId, int boardId)
        {
            try
            {
                _logger?.LogInformation("Deleting board column {ColumnId} from board {BoardId}", columnId, boardId);

                // Use the execution strategy to handle retries with transactions
                var strategy = _context.Database.CreateExecutionStrategy();
                
                return await strategy.ExecuteAsync(async () =>
                {
                    // Use a transaction to ensure atomicity
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        // Step 1: Find the board-column mapping
                        var mapping = await _context.BoardBoardColumnMaps
                            .Include(m => m.BoardColumn)
                            .FirstOrDefaultAsync(m => m.BoardColumnId == columnId && m.BoardId == boardId);

                        if (mapping == null)
                        {
                            _logger?.LogWarning("Board-column mapping not found for column {ColumnId} and board {BoardId}", 
                                columnId, boardId);
                            return false;
                        }

                        var deletedPosition = mapping.BoardColumn?.Position ?? 0;

                        // Step 2: Delete the mapping
                        _context.BoardBoardColumnMaps.Remove(mapping);
                        await _context.SaveChangesAsync();

                        _logger?.LogInformation("Deleted board-column mapping for column {ColumnId}", columnId);

                        // Step 3: Delete the BoardColumn itself
                        var boardColumn = await _context.BoardColumns.FindAsync(columnId);
                        if (boardColumn != null)
                        {
                            _context.BoardColumns.Remove(boardColumn);
                            await _context.SaveChangesAsync();

                            _logger?.LogInformation("Deleted board column {ColumnId}", columnId);
                        }

                        // Step 4: Reorder remaining columns
                        await ReorderColumnPositionsAfterDeleteAsync(boardId, deletedPosition);

                        // Commit the transaction
                        await transaction.CommitAsync();

                        _logger?.LogInformation("Successfully deleted board column {ColumnId} and reordered remaining columns", 
                            columnId);

                        return true;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting board column {ColumnId} from board {BoardId}", columnId, boardId);
                throw new InvalidOperationException($"An error occurred while deleting board column", ex);
            }
        }

        public async Task<BoardColumn> CreateBoardColumnAsync(int boardId, BoardColumn boardColumn)
        {
            try
            {
                _logger?.LogInformation("Creating board column for board {BoardId} at position {Position}", 
                    boardId, boardColumn.Position);

                // Use the execution strategy to handle retries with transactions
                var strategy = _context.Database.CreateExecutionStrategy();
                
                return await strategy.ExecuteAsync(async () =>
                {
                    // Use a transaction to ensure atomicity
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        // Step 1: Add the BoardColumn
                        await _context.BoardColumns.AddAsync(boardColumn);
                        await _context.SaveChangesAsync();

                        _logger?.LogInformation("Created board column with ID {ColumnId}", boardColumn.Id);

                        // Step 2: Create the mapping between board and column
                        var mapping = new BoardBoardColumnMap
                        {
                            BoardId = boardId,
                            BoardColumnId = boardColumn.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _context.BoardBoardColumnMaps.AddAsync(mapping);
                        await _context.SaveChangesAsync();

                        _logger?.LogInformation("Created board-column mapping with ID {MappingId}", mapping.Id);

                        // Commit the transaction
                        await transaction.CommitAsync();

                        _logger?.LogInformation("Successfully created board column and mapping");

                        return boardColumn;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating board column for board {BoardId}. Transaction rolled back.", boardId);
                throw new InvalidOperationException($"An error occurred while creating board column", ex);
            }
        }

        public async Task<BoardColumn> UpdateBoardColumnAsync(Guid columnId, BoardColumn updatedColumn)
        {
            try
            {
                _logger?.LogInformation("Updating board column {ColumnId}", columnId);

                var existingColumn = await _context.BoardColumns
                    .Include(bc => bc.Status)
                    .FirstOrDefaultAsync(bc => bc.Id == columnId);

                if (existingColumn == null)
                {
                    _logger?.LogWarning("Board column {ColumnId} not found for update", columnId);
                    throw new KeyNotFoundException($"Board column with ID {columnId} not found");
                }

                // Update only the properties that are provided (not null)
                if (!string.IsNullOrWhiteSpace(updatedColumn.BoardColumnName))
                {
                    existingColumn.BoardColumnName = updatedColumn.BoardColumnName.Trim();
                }

                if (!string.IsNullOrWhiteSpace(updatedColumn.BoardColor))
                {
                    existingColumn.BoardColor = updatedColumn.BoardColor.ToUpper();
                }

                if (updatedColumn.Position.HasValue)
                {
                    existingColumn.Position = updatedColumn.Position.Value;
                }

                if (updatedColumn.StatusId.HasValue)
                {
                    existingColumn.StatusId = updatedColumn.StatusId.Value;
                }

                await _context.SaveChangesAsync();

                _logger?.LogInformation("Successfully updated board column {ColumnId}", columnId);

                // Reload to get the updated status navigation property
                await _context.Entry(existingColumn).Reference(bc => bc.Status).LoadAsync();

                return existingColumn;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating board column {ColumnId}", columnId);
                throw new InvalidOperationException($"An error occurred while updating board column", ex);
            }
        }

        public async Task ShiftColumnPositionsForMoveAsync(int boardId, int oldPosition, int newPosition)
        {
            try
            {
                _logger?.LogInformation(
                    "Shifting column positions for board {BoardId} from position {OldPosition} to {NewPosition}",
                    boardId, oldPosition, newPosition);

                if (oldPosition == newPosition)
                {
                    // No shift needed
                    return;
                }

                // Get all columns for this board except the one being moved
                var columnsToShift = await _context.BoardBoardColumnMaps
                    .Where(m => m.BoardId == boardId)
                    .Include(m => m.BoardColumn)
                    .Select(m => m.BoardColumn!)
                    .Where(bc => bc != null)
                    .ToListAsync();

                if (oldPosition < newPosition)
                {
                    // Moving down: shift columns between oldPosition and newPosition down by 1
                    // Example: moving from position 3 to 8
                    // Columns at positions 4,5,6,7,8 should move to 3,4,5,6,7
                    foreach (var column in columnsToShift.Where(c => c.Position > oldPosition && c.Position <= newPosition))
                    {
                        column.Position = (column.Position ?? 0) - 1;
                    }
                }
                else
                {
                    // Moving up: shift columns between newPosition and oldPosition up by 1
                    // Example: moving from position 8 to 3
                    // Columns at positions 3,4,5,6,7 should move to 4,5,6,7,8
                    foreach (var column in columnsToShift.Where(c => c.Position >= newPosition && c.Position < oldPosition))
                    {
                        column.Position = (column.Position ?? 0) + 1;
                    }
                }

                await _context.SaveChangesAsync();

                _logger?.LogInformation("Successfully shifted columns for board {BoardId}", boardId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error shifting column positions for board {BoardId}", boardId);
                throw new InvalidOperationException($"An error occurred while shifting column positions", ex);
            }
        }
    }
}
