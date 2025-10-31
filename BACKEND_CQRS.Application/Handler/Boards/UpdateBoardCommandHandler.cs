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
    /// Handler for updating board properties
    /// </summary>
    public class UpdateBoardCommandHandler 
        : IRequestHandler<UpdateBoardCommand, ApiResponse<UpdateBoardResponseDto>>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IGenericRepository<Domain.Entities.Teams> _teamRepository;
        private readonly IGenericRepository<Domain.Entities.Users> _userRepository;
        private readonly ILogger<UpdateBoardCommandHandler> _logger;

        public UpdateBoardCommandHandler(
            IBoardRepository boardRepository,
            IGenericRepository<Domain.Entities.Teams> teamRepository,
            IGenericRepository<Domain.Entities.Users> userRepository,
            ILogger<UpdateBoardCommandHandler> logger)
        {
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<UpdateBoardResponseDto>> Handle(
            UpdateBoardCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating board {BoardId}", request.BoardId);

                // Step 1: Validate that at least one field is being updated
                if (!HasAnyUpdateFields(request))
                {
                    _logger.LogWarning("No fields to update for board {BoardId}", request.BoardId);
                    return ApiResponse<UpdateBoardResponseDto>.Fail(
                        "At least one field must be provided for update (Name, Description, Type, TeamId, IsActive, or Metadata)");
                }

                // Step 2: Get the existing board
                var existingBoard = await _boardRepository.GetBoardByIdAsync(request.BoardId);
                if (existingBoard == null)
                {
                    _logger.LogWarning("Board {BoardId} not found", request.BoardId);
                    return ApiResponse<UpdateBoardResponseDto>.Fail(
                        $"Board with ID {request.BoardId} does not exist");
                }

                // Track previous values and updated fields
                var previousValues = new Dictionary<string, string>();
                var updatedFields = new List<string>();
                bool teamAssociationRemoved = false;
                bool teamAssociationAdded = false;

                // Step 3: Validate name uniqueness if name is being changed
                if (!string.IsNullOrWhiteSpace(request.Name) && 
                    request.Name.Trim() != existingBoard.Name)
                {
                    var duplicateBoards = await _boardRepository.FindAsync(
                        b => b.ProjectId == existingBoard.ProjectId &&
                             b.Name.ToLower() == request.Name.Trim().ToLower() &&
                             b.Id != request.BoardId &&
                             b.IsActive);

                    if (duplicateBoards.Any())
                    {
                        _logger.LogWarning(
                            "Board with name '{Name}' already exists in project {ProjectId}",
                            request.Name, existingBoard.ProjectId);
                        return ApiResponse<UpdateBoardResponseDto>.Fail(
                            $"A board with the name '{request.Name}' already exists in this project");
                    }

                    previousValues["Name"] = existingBoard.Name;
                    updatedFields.Add($"Name (from '{existingBoard.Name}' to '{request.Name.Trim()}')");
                }

                // Step 4: Validate and handle team changes
                Domain.Entities.Teams? newTeam = null;
                if (request.RemoveTeamAssociation == true || 
                    (request.TeamId.HasValue && request.TeamId == 0))
                {
                    // Remove team association
                    if (existingBoard.TeamId.HasValue)
                    {
                        previousValues["TeamId"] = existingBoard.TeamId.Value.ToString();
                        previousValues["TeamName"] = existingBoard.Team?.Name ?? "Unknown";
                        updatedFields.Add($"Team association removed (was '{existingBoard.Team?.Name}')");
                        teamAssociationRemoved = true;
                    }
                }
                else if (request.TeamId.HasValue && request.TeamId > 0)
                {
                    // Validate new team
                    var teams = await _teamRepository.FindAsync(t => t.Id == request.TeamId.Value);
                    newTeam = teams.FirstOrDefault();

                    if (newTeam == null)
                    {
                        _logger.LogWarning("Team {TeamId} not found", request.TeamId.Value);
                        return ApiResponse<UpdateBoardResponseDto>.Fail(
                            $"Team with ID {request.TeamId.Value} does not exist");
                    }

                    // Validate team belongs to same project
                    if (newTeam.ProjectId != existingBoard.ProjectId)
                    {
                        _logger.LogWarning(
                            "Team {TeamId} does not belong to project {ProjectId}",
                            request.TeamId.Value, existingBoard.ProjectId);
                        return ApiResponse<UpdateBoardResponseDto>.Fail(
                            $"Team with ID {request.TeamId.Value} does not belong to the same project as the board");
                    }

                    // Validate team is active
                    if (newTeam.IsActive == false)
                    {
                        _logger.LogWarning("Team {TeamId} is not active", request.TeamId.Value);
                        return ApiResponse<UpdateBoardResponseDto>.Fail(
                            $"Team with ID {request.TeamId.Value} is not active");
                    }

                    // Track change
                    if (existingBoard.TeamId != request.TeamId.Value)
                    {
                        if (existingBoard.TeamId.HasValue)
                        {
                            previousValues["TeamId"] = existingBoard.TeamId.Value.ToString();
                            previousValues["TeamName"] = existingBoard.Team?.Name ?? "Unknown";
                            updatedFields.Add($"Team (from '{existingBoard.Team?.Name}' to '{newTeam.Name}')");
                        }
                        else
                        {
                            updatedFields.Add($"Team association added ('{newTeam.Name}')");
                            teamAssociationAdded = true;
                        }
                    }
                }

                // Step 5: Validate type and team consistency
                string effectiveType = request.Type ?? existingBoard.Type;
                int? effectiveTeamId = request.RemoveTeamAssociation == true ? null :
                                       (request.TeamId ?? existingBoard.TeamId);

                var validationResult = ValidateTypeAndTeam(effectiveType, effectiveTeamId);
                if (!validationResult.isValid)
                {
                    _logger.LogWarning("Type/Team validation failed: {Error}", validationResult.errorMessage);
                    return ApiResponse<UpdateBoardResponseDto>.Fail(validationResult.errorMessage!);
                }

                // Step 6: Track other field changes
                TrackFieldChanges(request, existingBoard, previousValues, updatedFields);

                // Step 7: Validate metadata if provided
                if (!string.IsNullOrWhiteSpace(request.Metadata))
                {
                    try
                    {
                        System.Text.Json.JsonDocument.Parse(request.Metadata);
                    }
                    catch
                    {
                        return ApiResponse<UpdateBoardResponseDto>.Fail("Metadata must be valid JSON format");
                    }
                }

                // Step 8: Validate updater user if provided
                Users? updaterUser = null;
                if (request.UpdatedBy.HasValue)
                {
                    var users = await _userRepository.FindAsync(u => u.Id == request.UpdatedBy.Value);
                    updaterUser = users.FirstOrDefault();

                    if (updaterUser == null)
                    {
                        _logger.LogWarning("User {UserId} not found", request.UpdatedBy.Value);
                        return ApiResponse<UpdateBoardResponseDto>.Fail(
                            $"User with ID {request.UpdatedBy.Value} does not exist");
                    }
                }

                // Step 9: Build update object
                var boardUpdate = new Board
                {
                    Id = request.BoardId,
                    Name = request.Name,
                    Description = request.Description,
                    Type = request.Type,
                    TeamId = request.RemoveTeamAssociation == true ? null : request.TeamId,
                    IsActive = request.IsActive ?? existingBoard.IsActive,
                    Metadata = string.IsNullOrWhiteSpace(request.Metadata) ? null : request.Metadata,
                    UpdatedBy = request.UpdatedBy
                };

                // Step 10: Update the board
                var updatedBoard = await _boardRepository.UpdateBoardAsync(request.BoardId, boardUpdate);

                _logger.LogInformation(
                    "Successfully updated board {BoardId}. Updated fields: {UpdatedFields}",
                    request.BoardId, string.Join(", ", updatedFields));

                // Step 11: Prepare response
                var response = new UpdateBoardResponseDto
                {
                    BoardId = updatedBoard.Id,
                    ProjectId = updatedBoard.ProjectId,
                    ProjectName = updatedBoard.Project?.Name ?? "Unknown",
                    TeamId = updatedBoard.TeamId,
                    TeamName = updatedBoard.Team?.Name,
                    Name = updatedBoard.Name,
                    Description = updatedBoard.Description,
                    Type = updatedBoard.Type,
                    IsTeamBased = updatedBoard.TeamId.HasValue,
                    IsActive = updatedBoard.IsActive,
                    UpdatedBy = updatedBoard.UpdatedBy,
                    UpdatedByName = updatedBoard.Updater?.Name,
                    UpdatedAt = updatedBoard.UpdatedAt,
                    Metadata = updatedBoard.Metadata,
                    PreviousValues = previousValues,
                    UpdatedFields = updatedFields,
                    TeamAssociationRemoved = teamAssociationRemoved,
                    TeamAssociationAdded = teamAssociationAdded
                };

                var message = $"Board '{updatedBoard.Name}' updated successfully";
                if (updatedFields.Count > 0)
                {
                    message += $". Updated: {string.Join(", ", updatedFields.Select(f => f.Split('(')[0].Trim()))}";
                }

                return ApiResponse<UpdateBoardResponseDto>.Success(response, message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found while updating board {BoardId}", request.BoardId);
                return ApiResponse<UpdateBoardResponseDto>.Fail(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Database error while updating board {BoardId}", request.BoardId);
                return ApiResponse<UpdateBoardResponseDto>.Fail(
                    "A database error occurred while updating the board. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating board {BoardId}", request.BoardId);
                return ApiResponse<UpdateBoardResponseDto>.Fail(
                    "An unexpected error occurred while updating the board. Please contact support if the issue persists.");
            }
        }

        private bool HasAnyUpdateFields(UpdateBoardCommand request)
        {
            return !string.IsNullOrWhiteSpace(request.Name) ||
                   request.Description != null ||
                   !string.IsNullOrWhiteSpace(request.Type) ||
                   request.TeamId.HasValue ||
                   request.RemoveTeamAssociation == true ||
                   request.IsActive.HasValue ||
                   request.Metadata != null;
        }

        private void TrackFieldChanges(
            UpdateBoardCommand request,
            Board existingBoard,
            Dictionary<string, string> previousValues,
            List<string> updatedFields)
        {
            if (!string.IsNullOrWhiteSpace(request.Description) && 
                request.Description != existingBoard.Description)
            {
                previousValues["Description"] = existingBoard.Description ?? "(empty)";
                var newDesc = string.IsNullOrWhiteSpace(request.Description) ? "(empty)" : 
                             (request.Description.Length > 50 ? request.Description.Substring(0, 50) + "..." : request.Description);
                updatedFields.Add($"Description updated");
            }

            if (!string.IsNullOrWhiteSpace(request.Type) && 
                request.Type.ToLower() != existingBoard.Type.ToLower())
            {
                previousValues["Type"] = existingBoard.Type;
                updatedFields.Add($"Type (from '{existingBoard.Type}' to '{request.Type.ToLower()}')");
            }

            if (request.IsActive.HasValue && 
                request.IsActive.Value != existingBoard.IsActive)
            {
                previousValues["IsActive"] = existingBoard.IsActive.ToString();
                updatedFields.Add($"IsActive (from {existingBoard.IsActive} to {request.IsActive.Value})");
            }

            if (!string.IsNullOrWhiteSpace(request.Metadata) && 
                request.Metadata != existingBoard.Metadata)
            {
                previousValues["Metadata"] = existingBoard.Metadata ?? "(empty)";
                updatedFields.Add("Metadata updated");
            }
        }

        private (bool isValid, string? errorMessage) ValidateTypeAndTeam(string type, int? teamId)
        {
            type = type.ToLower();

            // Team-based boards should have a team
            if (type == "team" && !teamId.HasValue)
            {
                return (false, "Team-based boards must have a TeamId");
            }

            // Custom boards should not have a team
            if (type == "custom" && teamId.HasValue)
            {
                return (false, "Custom boards cannot have a TeamId");
            }

            return (true, null);
        }
    }
}
