using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Application.Handler.Boards
{
    /// <summary>
    /// Handler for creating a new board
    /// </summary>
    public class CreateBoardCommandHandler : IRequestHandler<CreateBoardCommand, ApiResponse<CreateBoardResponseDto>>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IGenericRepository<Domain.Entities.Projects> _projectRepository;
        private readonly IGenericRepository<Domain.Entities.Teams> _teamRepository;
        private readonly IGenericRepository<Domain.Entities.Users> _userRepository;
        private readonly ILogger<CreateBoardCommandHandler> _logger;

        public CreateBoardCommandHandler(
            IBoardRepository boardRepository,
            IGenericRepository<Domain.Entities.Projects> projectRepository,
            IGenericRepository<Domain.Entities.Teams> teamRepository,
            IGenericRepository<Domain.Entities.Users> userRepository,
            ILogger<CreateBoardCommandHandler> logger)
        {
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<CreateBoardResponseDto>> Handle(
            CreateBoardCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Processing create board request - Project: {ProjectId}, Name: {Name}, Type: {Type}, TeamId: {TeamId}",
                    request.ProjectId, request.Name, request.Type, request.TeamId);

                // Step 1: Validate project exists
                var project = await _projectRepository.FindAsync(p => p.Id == request.ProjectId);
                if (project == null || !project.Any())
                {
                    _logger.LogWarning("Project {ProjectId} does not exist", request.ProjectId);
                    return ApiResponse<CreateBoardResponseDto>.Fail(
                        $"Project with ID {request.ProjectId} does not exist");
                }

                var projectEntity = project.First();

                // Step 2: Validate board type and team consistency
                var validationResult = ValidateBoardTypeAndTeam(request);
                if (!validationResult.isValid)
                {
                    _logger.LogWarning("Validation failed: {Error}", validationResult.errorMessage);
                    return ApiResponse<CreateBoardResponseDto>.Fail(validationResult.errorMessage!);
                }

                // Step 3: If team-based, validate team exists and belongs to the project
                Domain.Entities.Teams? teamEntity = null;
                if (request.TeamId.HasValue)
                {
                    var teams = await _teamRepository.FindAsync(t => t.Id == request.TeamId.Value);
                    teamEntity = teams.FirstOrDefault();

                    if (teamEntity == null)
                    {
                        _logger.LogWarning("Team {TeamId} does not exist", request.TeamId.Value);
                        return ApiResponse<CreateBoardResponseDto>.Fail(
                            $"Team with ID {request.TeamId.Value} does not exist");
                    }

                    if (teamEntity.ProjectId != request.ProjectId)
                    {
                        _logger.LogWarning(
                            "Team {TeamId} does not belong to project {ProjectId}. Team belongs to project {TeamProjectId}",
                            request.TeamId.Value, request.ProjectId, teamEntity.ProjectId);
                        return ApiResponse<CreateBoardResponseDto>.Fail(
                            $"Team with ID {request.TeamId.Value} does not belong to project {request.ProjectId}");
                    }

                    // Check if team is active
                    if (teamEntity.IsActive == false)
                    {
                        _logger.LogWarning("Team {TeamId} is not active", request.TeamId.Value);
                        return ApiResponse<CreateBoardResponseDto>.Fail(
                            $"Team with ID {request.TeamId.Value} is not active");
                    }
                }

                // Step 4: Validate creator user exists (if provided)
                Domain.Entities.Users? creatorUser = null;
                if (request.CreatedBy.HasValue)
                {
                    var users = await _userRepository.FindAsync(u => u.Id == request.CreatedBy.Value);
                    creatorUser = users.FirstOrDefault();

                    if (creatorUser == null)
                    {
                        _logger.LogWarning("User {UserId} does not exist", request.CreatedBy.Value);
                        return ApiResponse<CreateBoardResponseDto>.Fail(
                            $"User with ID {request.CreatedBy.Value} does not exist");
                    }
                }

                // Step 5: Validate board name uniqueness within the project
                var existingBoards = await _boardRepository.FindAsync(
                    b => b.ProjectId == request.ProjectId && 
                         b.Name.ToLower() == request.Name.ToLower() && 
                         b.IsActive);

                if (existingBoards.Any())
                {
                    _logger.LogWarning(
                        "Board with name '{Name}' already exists in project {ProjectId}",
                        request.Name, request.ProjectId);
                    return ApiResponse<CreateBoardResponseDto>.Fail(
                        $"A board with the name '{request.Name}' already exists in this project");
                }

                // Step 6: Sanitize metadata - convert empty string to null for PostgreSQL JSON compatibility
                string? sanitizedMetadata = string.IsNullOrWhiteSpace(request.Metadata) ? null : request.Metadata;

                // Step 7: Create the board entity
                var board = new Board
                {
                    ProjectId = request.ProjectId,
                    TeamId = request.TeamId,
                    Name = request.Name.Trim(),
                    Description = request.Description?.Trim(),
                    Type = request.Type.ToLower(),
                    IsActive = true,
                    CreatedBy = request.CreatedBy,
                    UpdatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Metadata = sanitizedMetadata
                };

                _logger.LogInformation("Creating board '{Name}' in project {ProjectId}", board.Name, board.ProjectId);

                // Step 8: Save the board to the database
                var createdBoard = await _boardRepository.CreateAsync(board);

                _logger.LogInformation(
                    "Successfully created board {BoardId} - '{Name}' in project {ProjectId}",
                    createdBoard.Id, createdBoard.Name, createdBoard.ProjectId);

                // Step 9: Prepare response
                var response = new CreateBoardResponseDto
                {
                    BoardId = createdBoard.Id,
                    ProjectId = createdBoard.ProjectId,
                    ProjectName = projectEntity.Name,
                    TeamId = createdBoard.TeamId,
                    TeamName = teamEntity?.Name,
                    Name = createdBoard.Name,
                    Description = createdBoard.Description,
                    Type = createdBoard.Type,
                    IsTeamBased = createdBoard.TeamId.HasValue,
                    IsActive = createdBoard.IsActive,
                    CreatedBy = createdBoard.CreatedBy,
                    CreatedByName = creatorUser?.Name,
                    CreatedAt = createdBoard.CreatedAt,
                    Metadata = createdBoard.Metadata
                };

                var message = createdBoard.TeamId.HasValue
                    ? $"Team-based board '{createdBoard.Name}' created successfully for team '{teamEntity?.Name}'"
                    : $"Custom board '{createdBoard.Name}' created successfully";

                return ApiResponse<CreateBoardResponseDto>.Created(response, message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Database error while creating board for project {ProjectId}", request.ProjectId);
                return ApiResponse<CreateBoardResponseDto>.Fail(
                    "A database error occurred while creating the board. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating board for project {ProjectId}", request.ProjectId);
                return ApiResponse<CreateBoardResponseDto>.Fail(
                    "An unexpected error occurred while creating the board. Please contact support if the issue persists.");
            }
        }

        /// <summary>
        /// Validates the board type and team ID consistency
        /// </summary>
        private (bool isValid, string? errorMessage) ValidateBoardTypeAndTeam(CreateBoardCommand request)
        {
            // Rule 1: Team-based boards MUST have a TeamId
            if (request.Type.ToLower() == "team" && !request.TeamId.HasValue)
            {
                return (false, "Team-based boards must have a TeamId. Please provide a valid TeamId or change the board type to 'custom'.");
            }

            // Rule 2: Custom boards MUST NOT have a TeamId
            if (request.Type.ToLower() == "custom" && request.TeamId.HasValue)
            {
                return (false, "Custom boards cannot have a TeamId. Please remove the TeamId or change the board type to 'team'.");
            }

            // Rule 3: If TeamId is provided, board type should be 'team' (or kanban/scrum with team)
            if (request.TeamId.HasValue && request.Type.ToLower() == "custom")
            {
                return (false, "A TeamId is provided but board type is 'custom'. Please use type 'team' for team-based boards.");
            }

            // Rule 4: Validate metadata format if provided (and not empty string)
            if (!string.IsNullOrWhiteSpace(request.Metadata))
            {
                try
                {
                    System.Text.Json.JsonDocument.Parse(request.Metadata);
                }
                catch
                {
                    return (false, "Metadata must be valid JSON format");
                }
            }

            return (true, null);
        }
    }
}
