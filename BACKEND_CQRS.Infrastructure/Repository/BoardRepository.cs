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

        public async Task<BoardColumn> CreateBoardColumnAsync(int boardId, BoardColumn boardColumn)
        {
            // Use a transaction to ensure atomicity
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger?.LogInformation("Creating board column for board {BoardId} at position {Position}", 
                    boardId, boardColumn.Position);

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
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger?.LogError(ex, "Error creating board column for board {BoardId}. Transaction rolled back.", boardId);
                throw new InvalidOperationException($"An error occurred while creating board column", ex);
            }
        }
    }
}
