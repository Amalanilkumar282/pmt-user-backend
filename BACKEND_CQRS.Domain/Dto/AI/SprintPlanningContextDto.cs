using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Domain.Dto.AI
{
    public class SprintPlanningContextDto
    {
        public ProjectInfoDto Project { get; set; } = new ProjectInfoDto();
        public NewSprintDto NewSprint { get; set; } = new NewSprintDto();
        public List<BacklogIssueDto> BacklogIssues { get; set; } = new List<BacklogIssueDto>();
        public TeamVelocityDto TeamVelocity { get; set; } = new TeamVelocityDto();
        public List<InProgressSprintDto> InProgressSprints { get; set; } = new List<InProgressSprintDto>();
        public List<PlannedSprintDto> PlannedSprints { get; set; } = new List<PlannedSprintDto>();
    }

    public class ProjectInfoDto
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class NewSprintDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Goal { get; set; }
        public int TeamId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? TargetStoryPoints { get; set; }
    }

    public class BacklogIssueDto
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public int? StoryPoints { get; set; }
        public int? AssigneeId { get; set; }
        public Guid? EpicId { get; set; }
        public List<string> Labels { get; set; } = new List<string>();
        public Guid? ParentIssueId { get; set; }
    }

    public class TeamVelocityDto
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public List<HistoricalSprintDto> HistoricalSprints { get; set; } = new List<HistoricalSprintDto>();
        public decimal AverageVelocity { get; set; }
        public string RecentVelocityTrend { get; set; } = string.Empty;
        public List<MemberVelocityDto> MemberVelocities { get; set; } = new List<MemberVelocityDto>();
    }

    public class HistoricalSprintDto
    {
        public Guid SprintId { get; set; }
        public string SprintName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public decimal PlannedPoints { get; set; }
        public decimal CompletedPoints { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class MemberVelocityDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal AvgPointsPerSprint { get; set; }
        public decimal CompletionRate { get; set; }
        public List<string> IssueTypesPreference { get; set; } = new List<string>();
    }

    public class InProgressSprintDto
    {
        public Guid SprintId { get; set; }
        public string SprintName { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public decimal AllocatedPoints { get; set; }
        public decimal RemainingPoints { get; set; }
    }

    public class PlannedSprintDto
    {
        public Guid SprintId { get; set; }
        public string SprintName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public decimal AllocatedPoints { get; set; }
    }
}
