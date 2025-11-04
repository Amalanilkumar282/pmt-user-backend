using BACKEND_CQRS.Domain.Dto.AI;
using BACKEND_CQRS.Domain.Services;
using BACKEND_CQRS.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Infrastructure.Services
{
    public class SprintPlannerService : ISprintPlannerService
    {
        private readonly AppDbContext _context;
        private readonly IGeminiAIService _geminiAIService;
        private readonly ILogger<SprintPlannerService> _logger;

        public SprintPlannerService(
            AppDbContext context,
            IGeminiAIService geminiAIService,
            ILogger<SprintPlannerService> logger)
        {
            _context = context;
            _geminiAIService = geminiAIService;
            _logger = logger;
        }

        public async Task<GeminiSprintPlanResponseDto> PlanSprintWithAIAsync(
            Guid projectId,
            PlanSprintRequestDto request,
            int userId)
        {
            // 1. Validate team exists and belongs to project (only if team is provided)
            if (request.TeamId.HasValue)
            {
                await ValidateTeamAsync(projectId, request.TeamId.Value);
            }

            // 2. Build context from database
            var context = await BuildSprintPlanningContextAsync(projectId, request);

            // 3. Log context being sent to Gemini
            _logger.LogInformation($"Sending context to Gemini AI for project {projectId}");

            // Log the full context JSON for debugging
            var contextJson = JsonSerializer.Serialize(context, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            _logger.LogInformation($"===== CONTEXT SENT TO GEMINI =====");
            _logger.LogInformation($"{contextJson}");
            _logger.LogInformation($"===== END CONTEXT =====");

            // 4. Call Gemini AI Service
            var response = await _geminiAIService.GenerateSprintPlanAsync(context);

            // 5. Log response
            _logger.LogInformation($"Received response from Gemini AI for project {projectId}");
            _logger.LogInformation($"===== GEMINI RESPONSE =====");
            var responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            _logger.LogInformation($"{responseJson}");
            _logger.LogInformation($"===== END RESPONSE =====");

            return response;
        }

        private async Task ValidateTeamAsync(Guid projectId, int teamId)
        {
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == teamId && t.ProjectId == projectId && t.IsActive == true);

            if (team == null)
            {
                throw new InvalidOperationException($"Team {teamId} not found or does not belong to project {projectId}");
            }
        }

        private async Task<SprintPlanningContextDto> BuildSprintPlanningContextAsync(
            Guid projectId,
            PlanSprintRequestDto request)
        {
            var context = new SprintPlanningContextDto
            {
                Project = await GetProjectInfoAsync(projectId),
                NewSprint = new NewSprintDto
                {
                    Name = request.SprintName,
                    Goal = request.SprintGoal,
                    TeamId = request.TeamId,
                    StartDate = request.StartDate,
                    DueDate = request.DueDate,
                    TargetStoryPoints = request.TargetStoryPoints
                },
                BacklogIssues = await GetBacklogIssuesAsync(projectId),
                InProgressSprints = await GetInProgressSprintsAsync(projectId, request.TeamId),
                PlannedSprints = await GetPlannedSprintsAsync(projectId, request.TeamId)
            };

            // Scenario 1: TeamId is provided - use team_velocity structure
            if (request.TeamId.HasValue)
            {
                context.TeamVelocity = await GetTeamVelocityAsync(projectId, request.TeamId.Value);
                context.HistoricalSprints = null; // Don't include root-level historical sprints
            }
            // Scenario 2: TeamId is NOT provided - use root-level historical_sprints
            else
            {
                context.TeamVelocity = null; // Don't include team velocity
                context.HistoricalSprints = await GetAllHistoricalSprintsAsync(projectId);
            }

            return context;
        }

        private async Task<ProjectInfoDto> GetProjectInfoAsync(Guid projectId)
        {
            var project = await _context.Projects
                .Where(p => p.Id == projectId)
                .Select(p => new ProjectInfoDto
                {
                    Id = p.Id,
                    Key = p.Key ?? "",
                    Name = p.Name ?? ""
                })
                .FirstOrDefaultAsync();

            if (project == null)
            {
                throw new InvalidOperationException($"Project {projectId} not found");
            }

            return project;
        }

        private async Task<List<BacklogIssueDto>> GetBacklogIssuesAsync(Guid projectId)
        {
            // Fetch ALL backlog issues where sprint_id IS NULL
            // Don't filter by status - let Gemini decide based on priority and other factors
            var backlogIssuesQuery = await _context.Issues
                .Where(i => i.ProjectId == projectId && i.SprintId == null)
                .OrderByDescending(i => i.Priority)
                .ThenBy(i => i.CreatedAt)
                .Select(i => new
                {
                    i.Id,
                    Key = i.Key ?? "",
                    i.Title,
                    i.Type,
                    Priority = i.Priority ?? "MEDIUM",
                    i.StoryPoints,
                    i.AssigneeId,
                    i.EpicId,
                    i.Labels,
                    i.ParentIssueId
                })
                .ToListAsync();

            var backlogIssues = backlogIssuesQuery.Select(i => new BacklogIssueDto
            {
                Id = i.Id,
                Key = i.Key,
                Title = i.Title,
                Type = i.Type,
                Priority = i.Priority,
                StoryPoints = i.StoryPoints,
                AssigneeId = i.AssigneeId,
                EpicId = i.EpicId,
                Labels = string.IsNullOrEmpty(i.Labels)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(i.Labels) ?? new List<string>(),
                ParentIssueId = i.ParentIssueId
            }).ToList();

            return backlogIssues;
        }

        private async Task<TeamVelocityDto> GetTeamVelocityAsync(Guid projectId, int teamId)
        {
            var team = await _context.Teams
                .Where(t => t.Id == teamId)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    MemberCount = _context.TeamMembers
                        .Count(tm => tm.TeamId == t.Id)
                })
                .FirstOrDefaultAsync();

            if (team == null)
            {
                throw new InvalidOperationException($"Team {teamId} not found");
            }

            var historicalSprints = await GetHistoricalSprintsAsync(projectId, teamId);
            var memberVelocities = await GetMemberVelocitiesAsync(projectId, teamId);

            // Calculate average velocity only from COMPLETED sprints
            var completedSprints = historicalSprints
                .Where(s => s.Status == "COMPLETED")
                .ToList();

            var averageVelocity = completedSprints.Any()
                ? completedSprints.Average(s => s.CompletedPoints)
                : 0;

            var recentVelocityTrend = CalculateVelocityTrend(completedSprints);

            return new TeamVelocityDto
            {
                TeamId = team.Id,
                TeamName = team.Name,
                MemberCount = team.MemberCount,
                HistoricalSprints = historicalSprints,
                AverageVelocity = averageVelocity,
                RecentVelocityTrend = recentVelocityTrend,
                MemberVelocities = memberVelocities
            };
        }

        private async Task<List<HistoricalSprintDto>> GetHistoricalSprintsAsync(Guid projectId, int teamId)
        {
            var sprints = await _context.Sprints
                .Where(s => s.TeamId == teamId && s.ProjectId == projectId && s.Status == "COMPLETED")
                .OrderByDescending(s => s.StartDate)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Status,
                    s.StartDate,
                    s.DueDate,
                    s.StoryPoint,
                    // Sum ALL issues in the sprint (no status filter) - matches SQL query
                    CompletedPoints = _context.Issues
                        .Where(i => i.SprintId == s.Id)
                        .Sum(i => (decimal?)i.StoryPoints) ?? 0
                })
                .ToListAsync();

            return sprints.Select(s =>
            {
                var plannedPoints = s.StoryPoint ?? 0;
                var completedPoints = s.CompletedPoints;

                return new HistoricalSprintDto
                {
                    SprintId = s.Id,
                    SprintName = s.Name,
                    Status = s.Status ?? "UNKNOWN",
                    DurationDays = s.StartDate.HasValue && s.DueDate.HasValue
                        ? (int)(s.DueDate.Value - s.StartDate.Value).TotalDays
                        : 0,
                    PlannedPoints = (int)plannedPoints,
                    CompletedPoints = (int)completedPoints,
                    CompletionRate = plannedPoints > 0
                        ? Math.Round((completedPoints / plannedPoints) * 100, 2)
                        : 0
                };
            }).ToList();
        }

        // Scenario 2: Get all historical sprints across all teams (when teamId is NOT provided)
        private async Task<List<HistoricalSprintDto>> GetAllHistoricalSprintsAsync(Guid projectId)
        {
            var sprints = await _context.Sprints
                .Where(s => s.ProjectId == projectId && s.Status == "COMPLETED")
                .OrderByDescending(s => s.StartDate)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Status,
                    s.StartDate,
                    s.DueDate,
                    s.StoryPoint,
                    // Sum ALL issues in the sprint (no status filter) - matches SQL query
                    CompletedPoints = _context.Issues
                        .Where(i => i.SprintId == s.Id)
                        .Sum(i => (decimal?)i.StoryPoints) ?? 0
                })
                .ToListAsync();

            return sprints.Select(s =>
            {
                var plannedPoints = s.StoryPoint ?? 0;
                var completedPoints = s.CompletedPoints;

                return new HistoricalSprintDto
                {
                    SprintId = s.Id,
                    SprintName = s.Name,
                    Status = s.Status ?? "UNKNOWN",
                    DurationDays = s.StartDate.HasValue && s.DueDate.HasValue
                        ? (int)(s.DueDate.Value - s.StartDate.Value).TotalDays
                        : 0,
                    PlannedPoints = (int)plannedPoints,
                    CompletedPoints = (int)completedPoints,
                    CompletionRate = plannedPoints > 0
                        ? Math.Round((completedPoints / plannedPoints) * 100, 2)
                        : 0
                };
            }).ToList();
        }

        private async Task<List<MemberVelocityDto>> GetMemberVelocitiesAsync(Guid projectId, int teamId)
        {
            var completedStatusNames = new[] { "Done", "Closed", "Completed" };

            // Get team member user IDs
            var teamMemberUserIds = await _context.TeamMembers
                .Where(tm => tm.TeamId == teamId)
                .Join(_context.ProjectMembers,
                    tm => tm.ProjectMemberId,
                    pm => pm.Id,
                    (tm, pm) => pm.UserId)
                .ToListAsync();

            if (!teamMemberUserIds.Any())
            {
                return new List<MemberVelocityDto>();
            }

            // Get completed sprint IDs for this team
            var completedSprintIds = await _context.Sprints
                .Where(s => s.TeamId == teamId
                    && s.ProjectId == projectId
                    && s.Status == "COMPLETED")
                .Select(s => s.Id)
                .ToListAsync();

            if (!completedSprintIds.Any())
            {
                return new List<MemberVelocityDto>();
            }

            // Calculate member statistics
            var memberStats = await _context.Issues
                .Where(i => completedSprintIds.Contains(i.SprintId ?? Guid.Empty)
                    && i.AssigneeId.HasValue
                    && teamMemberUserIds.Contains(i.AssigneeId.Value))
                .GroupBy(i => i.AssigneeId!.Value)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalIssues = g.Count(),
                    CompletedIssues = g.Count(i => _context.Statuses
                        .Where(st => st.Id == i.StatusId && completedStatusNames.Contains(st.StatusName))
                        .Any()),
                    AvgStoryPoints = g.Average(i => (decimal?)i.StoryPoints) ?? 0,
                    IssueTypes = g.Select(i => i.Type).Distinct().ToList()
                })
                .ToListAsync();

            var result = new List<MemberVelocityDto>();

            foreach (var stat in memberStats)
            {
                var user = await _context.Users
                    .Where(u => u.Id == stat.UserId)
                    .FirstOrDefaultAsync();

                if (user == null) continue;

                var sprintCount = completedSprintIds.Count;
                var avgPointsPerSprint = sprintCount > 0 ? stat.AvgStoryPoints : 0;
                var completionRate = stat.TotalIssues > 0
                    ? Math.Round(((decimal)stat.CompletedIssues / stat.TotalIssues) * 100, 1)
                    : 0;

                result.Add(new MemberVelocityDto
                {
                    UserId = stat.UserId,
                    Name = user.Name ?? "Unknown",
                    AvgPointsPerSprint = avgPointsPerSprint,
                    CompletionRate = completionRate,
                    IssueTypesPreference = stat.IssueTypes
                });
            }

            return result;
        }

        private async Task<List<InProgressSprintDto>> GetInProgressSprintsAsync(Guid projectId, int? teamId)
        {
            var query = _context.Sprints
                .Where(s => s.ProjectId == projectId && s.Status == "ACTIVE");

            // Filter by team only if teamId is provided
            if (teamId.HasValue)
            {
                query = query.Where(s => s.TeamId == teamId.Value);
            }

            var inProgressSprints = await query
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.DueDate,
                    s.StoryPoint,
                    // Sum ALL issues in the sprint (no status filter) - matches SQL query
                    CompletedPoints = _context.Issues
                        .Where(i => i.SprintId == s.Id)
                        .Sum(i => (decimal?)i.StoryPoints) ?? 0
                })
                .ToListAsync();

            return inProgressSprints.Select(s =>
            {
                var allocatedPoints = (int)(s.StoryPoint ?? 0);
                var completedPoints = (int)s.CompletedPoints;
                var remainingPoints = Math.Max(allocatedPoints - completedPoints, 0);

                return new InProgressSprintDto
                {
                    SprintId = s.Id,
                    SprintName = s.Name,
                    DueDate = s.DueDate,
                    AllocatedPoints = allocatedPoints,
                    RemainingPoints = remainingPoints
                };
            }).ToList();
        }

        private async Task<List<PlannedSprintDto>> GetPlannedSprintsAsync(Guid projectId, int? teamId)
        {
            var query = _context.Sprints
                .Where(s => s.ProjectId == projectId && s.Status == "PLANNED");

            // Filter by team only if teamId is provided
            if (teamId.HasValue)
            {
                query = query.Where(s => s.TeamId == teamId.Value);
            }

            var plannedSprints = await query
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.StartDate,
                    s.StoryPoint
                })
                .ToListAsync();

            return plannedSprints.Select(s => new PlannedSprintDto
            {
                SprintId = s.Id,
                SprintName = s.Name,
                StartDate = s.StartDate,
                AllocatedPoints = (int)(s.StoryPoint ?? 0)
            }).ToList();
        }

        private string CalculateVelocityTrend(List<HistoricalSprintDto> sprints)
        {
            if (sprints.Count < 3)
            {
                return "stable";
            }

            // Get recent 3 sprints
            var recentSprints = sprints.Take(3).ToList();
            var maxCompleted = recentSprints.Max(s => s.CompletedPoints);
            var minCompleted = recentSprints.Min(s => s.CompletedPoints);

            if ((maxCompleted - minCompleted) > 5)
            {
                return "increasing";
            }
            else if ((minCompleted - maxCompleted) > 5)
            {
                return "decreasing";
            }

            return "stable";
        }
    }
}
