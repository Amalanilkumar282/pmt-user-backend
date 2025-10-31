using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Domain.Dto.AI;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Sprints
{
    public class PlanSprintWithAICommandHandler : IRequestHandler<PlanSprintWithAICommand, ApiResponse<GeminiSprintPlanResponseDto>>
    {
        private readonly ISprintPlannerService _sprintPlannerService;
        private readonly ILogger<PlanSprintWithAICommandHandler> _logger;

        public PlanSprintWithAICommandHandler(
            ISprintPlannerService sprintPlannerService,
            ILogger<PlanSprintWithAICommandHandler> logger)
        {
            _sprintPlannerService = sprintPlannerService;
            _logger = logger;
        }

        public async Task<ApiResponse<GeminiSprintPlanResponseDto>> Handle(
            PlanSprintWithAICommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Planning sprint with AI for project {request.ProjectId}");

                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.SprintName))
                {
                    return ApiResponse<GeminiSprintPlanResponseDto>.Fail("Sprint name is required");
                }

                if (request.TeamId <= 0)
                {
                    return ApiResponse<GeminiSprintPlanResponseDto>.Fail("Valid team ID is required");
                }

                // Create request DTO
                var planRequest = new PlanSprintRequestDto
                {
                    SprintName = request.SprintName,
                    SprintGoal = request.SprintGoal,
                    TeamId = request.TeamId,
                    StartDate = request.StartDate,
                    DueDate = request.DueDate,
                    TargetStoryPoints = request.TargetStoryPoints
                };

                // Call service to generate sprint plan
                var result = await _sprintPlannerService.PlanSprintWithAIAsync(
                    request.ProjectId,
                    planRequest,
                    request.UserId);

                _logger.LogInformation($"Successfully generated sprint plan for project {request.ProjectId}");

                return ApiResponse<GeminiSprintPlanResponseDto>.Success(
                    result,
                    "Sprint plan generated successfully");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Validation error while planning sprint: {ex.Message}");
                return ApiResponse<GeminiSprintPlanResponseDto>.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating sprint plan for project {request.ProjectId}");
                return ApiResponse<GeminiSprintPlanResponseDto>.Fail(
                    "An error occurred while generating the sprint plan. Please try again.");
            }
        }
    }
}
