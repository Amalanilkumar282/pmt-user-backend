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
    }
}
