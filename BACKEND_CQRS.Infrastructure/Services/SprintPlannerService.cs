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
            // 1. Validate team exists and belongs to project
            await ValidateTeamAsync(projectId, request.TeamId);

            // 2. Build context from database
            var context = await BuildSprintPlanningContextAsync(projectId, request);

            // 3. Log context being sent to Gemini
            _logger.LogInformation($"Sending context to Gemini AI for project {projectId}");
            _logger.LogDebug($"Context: {JsonSerializer.Serialize(context)}");

            // 4. Call Gemini AI Service
            var response = await _geminiAIService.GenerateSprintPlanAsync(context);

            // 5. Log response
            _logger.LogInformation($"Received response from Gemini AI for project {projectId}");
            _logger.LogDebug($"Response: {JsonSerializer.Serialize(response)}");

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
                TeamVelocity = await GetTeamVelocityAsync(projectId, request.TeamId),
                InProgressSprints = await GetInProgressSprintsAsync(projectId),
                PlannedSprints = await GetPlannedSprintsAsync(projectId)
            };

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
                    Name = p.Name
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
            var completedStatusNames = new[] { "Done", "Closed", "Completed" };
            var backlogStatusNames = new[] { "To Do", "Open", "Backlog" };

            var backlogIssuesQuery = await _context.Issues
                .Where(i => i.ProjectId == projectId
                    && i.SprintId == null
                    && _context.Statuses
                        .Where(s => backlogStatusNames.Contains(s.StatusName))
                        .Select(s => s.Id)
                        .Contains(i.StatusId ?? 0))
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

            var averageVelocity = historicalSprints.Any()
                ? historicalSprints.Average(s => s.CompletedPoints)
                : 0;

            var recentVelocityTrend = CalculateVelocityTrend(historicalSprints);

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
            var completedStatusNames = new[] { "Done", "Closed", "Completed" };

            var sprints = await _context.Sprints
                .Where(s => s.TeamId == teamId
                    && s.ProjectId == projectId
                    && (s.Status == "COMPLETED" || s.Status == "ACTIVE"))
                .OrderByDescending(s => s.CreatedAt)
                .Take(10)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Status,
                    s.StartDate,
                    s.DueDate,
                    Issues = _context.Issues
                        .Where(i => i.SprintId == s.Id)
                        .Select(i => new
                        {
                            i.StoryPoints,
                            StatusName = _context.Statuses
                                .Where(st => st.Id == i.StatusId)
                                .Select(st => st.StatusName)
                                .FirstOrDefault()
                        })
                        .ToList()
                })
                .ToListAsync();

            return sprints.Select(s =>
            {
                var plannedPoints = s.Issues.Sum(i => i.StoryPoints ?? 0);
                var completedPoints = s.Issues
                    .Where(i => completedStatusNames.Contains(i.StatusName ?? ""))
                    .Sum(i => i.StoryPoints ?? 0);

                return new HistoricalSprintDto
                {
                    SprintId = s.Id,
                    SprintName = s.Name,
                    Status = s.Status ?? "UNKNOWN",
                    DurationDays = s.StartDate.HasValue && s.DueDate.HasValue
                        ? (int)(s.DueDate.Value - s.StartDate.Value).TotalDays
                        : 0,
                    PlannedPoints = plannedPoints,
                    CompletedPoints = completedPoints,
                    CompletionRate = plannedPoints > 0
                        ? (completedPoints / (decimal)plannedPoints) * 100
                        : 0
                };
            }).ToList();
        }

        private async Task<List<MemberVelocityDto>> GetMemberVelocitiesAsync(Guid projectId, int teamId)
        {
            var completedStatusNames = new[] { "Done", "Closed", "Completed" };

            // Get team member IDs
            var teamMemberIds = await _context.TeamMembers
                .Where(tm => tm.TeamId == teamId)
                .Select(tm => tm.ProjectMemberId)
                .ToListAsync();

            // Get user IDs from project members
            var userIds = await _context.ProjectMembers
                .Where(pm => teamMemberIds.Contains(pm.Id))
                .Select(pm => pm.UserId)
                .ToListAsync();

            if (!userIds.Any())
            {
                return new List<MemberVelocityDto>();
            }

            // Get completed sprints for this team
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

            // Get member statistics
            var memberStats = await _context.Issues
                .Where(i => i.ProjectId == projectId
                    && completedSprintIds.Contains(i.SprintId ?? Guid.Empty)
                    && i.AssigneeId.HasValue
                    && userIds.Contains(i.AssigneeId.Value))
                .GroupBy(i => i.AssigneeId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalIssues = g.Count(),
                    CompletedIssues = g.Count(i => _context.Statuses
                        .Where(st => st.Id == i.StatusId && completedStatusNames.Contains(st.StatusName))
                        .Any()),
                    TotalPoints = g.Sum(i => i.StoryPoints ?? 0),
                    CompletedPoints = g
                        .Where(i => _context.Statuses
                            .Where(st => st.Id == i.StatusId && completedStatusNames.Contains(st.StatusName))
                            .Any())
                        .Sum(i => i.StoryPoints ?? 0),
                    IssueTypes = g.Select(i => i.Type).Distinct().ToList()
                })
                .ToListAsync();

            var result = new List<MemberVelocityDto>();

            foreach (var stat in memberStats)
            {
                if (!stat.UserId.HasValue) continue;

                var user = await _context.Users
                    .Where(u => u.Id == stat.UserId.Value)
                    .FirstOrDefaultAsync();

                if (user == null) continue;

                var sprintCount = completedSprintIds.Count;
                var avgPointsPerSprint = sprintCount > 0 ? (decimal)stat.CompletedPoints / sprintCount : 0;
                var completionRate = stat.TotalPoints > 0
                    ? ((decimal)stat.CompletedPoints / stat.TotalPoints) * 100
                    : 0;

                result.Add(new MemberVelocityDto
                {
                    UserId = stat.UserId.Value,
                    Name = user.Name,
                    AvgPointsPerSprint = avgPointsPerSprint,
                    CompletionRate = completionRate,
                    IssueTypesPreference = stat.IssueTypes
                });
            }

            return result;
        }

        private async Task<List<InProgressSprintDto>> GetInProgressSprintsAsync(Guid projectId)
        {
            var completedStatusNames = new[] { "Done", "Closed", "Completed" };

            var inProgressSprints = await _context.Sprints
                .Where(s => s.ProjectId == projectId && s.Status == "ACTIVE")
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.DueDate,
                    Issues = _context.Issues
                        .Where(i => i.SprintId == s.Id)
                        .Select(i => new
                        {
                            i.StoryPoints,
                            i.AssigneeId,
                            StatusName = _context.Statuses
                                .Where(st => st.Id == i.StatusId)
                                .Select(st => st.StatusName)
                                .FirstOrDefault()
                        })
                        .ToList()
                })
                .ToListAsync();

            return inProgressSprints.Select(s =>
            {
                var allocatedPoints = s.Issues.Sum(i => i.StoryPoints ?? 0);
                var remainingPoints = s.Issues
                    .Where(i => !completedStatusNames.Contains(i.StatusName ?? ""))
                    .Sum(i => i.StoryPoints ?? 0);

                return new InProgressSprintDto
                {
                    SprintId = s.Id,
                    SprintName = s.Name,
                    DueDate = s.DueDate,
                    AllocatedPoints = allocatedPoints,
                    RemainingPoints = remainingPoints,
                    TeamMemberIds = s.Issues
                        .Where(i => i.AssigneeId.HasValue)
                        .Select(i => i.AssigneeId!.Value)
                        .Distinct()
                        .ToList()
                };
            }).ToList();
        }

        private async Task<List<PlannedSprintDto>> GetPlannedSprintsAsync(Guid projectId)
        {
            var plannedSprints = await _context.Sprints
                .Where(s => s.ProjectId == projectId && s.Status == "PLANNED")
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.StartDate,
                    Issues = _context.Issues
                        .Where(i => i.SprintId == s.Id)
                        .Select(i => new
                        {
                            i.StoryPoints,
                            i.AssigneeId
                        })
                        .ToList()
                })
                .ToListAsync();

            return plannedSprints.Select(s => new PlannedSprintDto
            {
                SprintId = s.Id,
                SprintName = s.Name,
                StartDate = s.StartDate,
                AllocatedPoints = s.Issues.Sum(i => i.StoryPoints ?? 0),
                TeamMemberIds = s.Issues
                    .Where(i => i.AssigneeId.HasValue)
                    .Select(i => i.AssigneeId!.Value)
                    .Distinct()
                    .ToList()
            }).ToList();
        }

        private string CalculateVelocityTrend(List<HistoricalSprintDto> sprints)
        {
            if (sprints.Count < 6)
            {
                return "stable";
            }

            var recent3 = sprints.Take(3).Average(s => s.CompletedPoints);
            var previous3 = sprints.Skip(3).Take(3).Average(s => s.CompletedPoints);

            if (recent3 > previous3 * 1.1m) return "increasing";
            if (recent3 < previous3 * 0.9m) return "decreasing";
            return "stable";
        }
    }
}
