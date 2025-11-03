using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Domain.Dto.AI
{
    public class GeminiSprintPlanResponseDto
    {
        public SprintPlanDto SprintPlan { get; set; } = new SprintPlanDto();
    }

    public class SprintPlanDto
    {
        public List<SelectedIssueDto> SelectedIssues { get; set; } = new List<SelectedIssueDto>();
        public decimal TotalStoryPoints { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<RecommendationDto> Recommendations { get; set; } = new List<RecommendationDto>();
        public CapacityAnalysisDto CapacityAnalysis { get; set; } = new CapacityAnalysisDto();
    }

    public class SelectedIssueDto
    {
        public Guid IssueId { get; set; }
        public string IssueKey { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int StoryPoints { get; set; }
        public int? SuggestedAssigneeId { get; set; }
        public string Rationale { get; set; } = string.Empty;
    }

    public class RecommendationDto
    {
        public string Type { get; set; } = string.Empty; // capacity|priority|risk|dependency|team_balance
        public string Severity { get; set; } = string.Empty; // info|warning|critical
        public string Message { get; set; } = string.Empty;
    }

    public class CapacityAnalysisDto
    {
        public decimal TeamCapacityUtilization { get; set; }
        public decimal EstimatedCompletionProbability { get; set; }
        public List<string> RiskFactors { get; set; } = new List<string>();
    }
}
