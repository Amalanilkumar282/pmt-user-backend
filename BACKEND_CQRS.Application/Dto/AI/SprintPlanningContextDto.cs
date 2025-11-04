using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Dto.AI
{
    public class SprintPlanningContextDto
    {
        public ProjectInfoDto Project { get; set; }
        public NewSprintDto NewSprint { get; set; }
        public List<BacklogIssueDto> BacklogIssues { get; set; }

        // Used when teamId is provided (Scenario 1)
        public TeamVelocityDto? TeamVelocity { get; set; }

        // Used when teamId is NOT provided (Scenario 2)
        public List<HistoricalSprintDto>? HistoricalSprints { get; set; }

        public List<InProgressSprintDto> InProgressSprints { get; set; }
        public List<PlannedSprintDto> PlannedSprints { get; set; }
    }

    public class ProjectInfoDto
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
    }

    public class NewSprintDto
    {
        public string? Name { get; set; }
        public string? Goal { get; set; }
        public int? TeamId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? TargetStoryPoints { get; set; }
    }

    public class BacklogIssueDto
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public int? StoryPoints { get; set; }
        public int? AssigneeId { get; set; }
        public Guid? EpicId { get; set; }
        public List<string> Labels { get; set; }
        public Guid? ParentIssueId { get; set; }
    }

    public class TeamVelocityDto
    {
        public int? TeamId { get; set; }
        public string TeamName { get; set; }
        public int MemberCount { get; set; }
        public List<HistoricalSprintDto> HistoricalSprints { get; set; }
        public decimal AverageVelocity { get; set; }
        public string RecentVelocityTrend { get; set; }
        public List<MemberVelocityDto> MemberVelocities { get; set; }
    }

    public class HistoricalSprintDto
    {
        public Guid SprintId { get; set; }
        public string SprintName { get; set; }
        public string Status { get; set; }
        public int DurationDays { get; set; }
        public decimal PlannedPoints { get; set; }
        public decimal CompletedPoints { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class MemberVelocityDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public decimal AvgPointsPerSprint { get; set; }
        public decimal CompletionRate { get; set; }
        public List<string> IssueTypesPreference { get; set; }
    }

    public class InProgressSprintDto
    {
        public Guid SprintId { get; set; }
        public string SprintName { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal AllocatedPoints { get; set; }
        public decimal RemainingPoints { get; set; }
    }

    public class PlannedSprintDto
    {
        public Guid SprintId { get; set; }
        public string SprintName { get; set; }
        public DateTime? StartDate { get; set; }
        public decimal AllocatedPoints { get; set; }
    }
}
