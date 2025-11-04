# AI Sprint Planner - Issue Title Field Implementation

## Summary

Added support for including issue titles in the AI Sprint Planner response. The `SelectedIssueDto` now includes a `Title` field that displays the issue title alongside the issue key, making it easier for frontend to display comprehensive issue information.

## Changes Made

### 1. DTO Updates

#### Domain Layer

- **File**: `BACKEND_CQRS.Domain\Dto\AI\GeminiSprintPlanResponseDto.cs`
- **Change**: Added `Title` property to `SelectedIssueDto` class
  ```csharp
  public string Title { get; set; } = string.Empty;
  ```

#### Application Layer

- **File**: `BACKEND_CQRS.Application\Dto\AI\GeminiSprintPlanResponseDto.cs`
- **Change**: Added `Title` property to `SelectedIssueDto` class
  ```csharp
  public string Title { get; set; } = string.Empty;
  ```

### 2. Gemini AI Service Updates

#### Prompt Enhancement

- **File**: `BACKEND_CQRS.Infrastructure\Services\GeminiAIService.cs`
- **Change**: Updated the JSON schema in the prompt to instruct Gemini to include the title field
  ```json
  {
    "issueId": "exact-uuid-from-backlog-list",
    "issueKey": "string like PHX-201",
    "title": "exact title from the backlog issue",
    "storyPoints": number,
    "suggestedAssigneeId": number or null,
    "rationale": "brief explanation why this issue was selected"
  }
  ```

#### Title Enrichment Logic

- **File**: `BACKEND_CQRS.Infrastructure\Services\GeminiAIService.cs`
- **Change**: Added logic to enrich missing titles from backlog data if Gemini doesn't provide them
  ```csharp
  // Enrich missing titles from backlog data
  foreach (var selectedIssue in response.SprintPlan.SelectedIssues)
  {
      if (string.IsNullOrEmpty(selectedIssue.Title) && backlogIssueLookup.TryGetValue(selectedIssue.IssueId, out var backlogIssue))
      {
          selectedIssue.Title = backlogIssue.Title;
          _logger.LogDebug($"Enriched missing title for issue {selectedIssue.IssueKey}: {backlogIssue.Title}");
      }
  }
  ```

#### Fallback Rule-Based Planner

- **File**: `BACKEND_CQRS.Infrastructure\Services\GeminiAIService.cs`
- **Change**: Updated the fallback rule-based planner to include title when creating `SelectedIssueDto` objects
  ```csharp
  selectedIssues.Add(new SelectedIssueDto
  {
      IssueId = issue.Id,
      IssueKey = issue.Key,
      Title = issue.Title,
      StoryPoints = issuePoints,
      SuggestedAssigneeId = issue.AssigneeId,
      Rationale = $"{issue.Priority ?? "MEDIUM"} priority {issue.Type} issue selected for sprint"
  });
  ```

### 3. Documentation Updates

#### AI Sprint Planner API Documentation

- **File**: `AI_SPRINT_PLANNER_API.md`
- **Change**: Added `title` field to the example response
  ```json
  {
    "issueId": "a1b2c3d4-e002-4e11-a9a1-010101010102",
    "issueKey": "PHX-202",
    "title": "Integrate Notification API",
    "storyPoints": 13,
    "suggestedAssigneeId": 1005,
    "rationale": "..."
  }
  ```

#### Combined API Documentation

- **File**: `API_SPRINT_PLANNING_COMBINED_DOC.md`
- **Change**: Added `title` field to the example response for `/api/sprints/projects/{projectId}/ai-plan` endpoint

## Response Format

The `selectedIssues` array now includes the following fields:

| Field               | Type    | Description                                |
| ------------------- | ------- | ------------------------------------------ |
| issueId             | string  | UUID of the issue                          |
| issueKey            | string  | Human-readable issue key (e.g., "PHX-202") |
| **title**           | string  | **Title/name of the issue**                |
| storyPoints         | integer | Story points for the issue                 |
| suggestedAssigneeId | integer | Suggested assignee user ID (nullable)      |
| rationale           | string  | Explanation for why the issue was selected |

## Example Response

```json
{
  "status": 200,
  "data": {
    "sprintPlan": {
      "selectedIssues": [
        {
          "issueId": "a1b2c3d4-e002-4e11-a9a1-010101010102",
          "issueKey": "PHX-202",
          "title": "Integrate Notification API",
          "storyPoints": 13,
          "suggestedAssigneeId": 1005,
          "rationale": "Highest priority CRITICAL issue that aligns with the sprint goal"
        }
      ],
      "totalStoryPoints": 36,
      "summary": "Sprint plan focuses on...",
      "recommendations": [...],
      "capacityAnalysis": {...}
    }
  },
  "message": "Sprint plan generated successfully"
}
```

## Frontend Integration Notes

1. **Display**: The `title` field can now be displayed alongside the `issueKey` for better user experience
2. **Fallback**: If Gemini doesn't provide a title, the enrichment logic ensures it's populated from backlog data
3. **Consistency**: Both AI-generated and rule-based fallback plans include the title field

## Testing

- ✅ Build successful with no compilation errors
- ✅ DTOs updated in both Domain and Application layers
- ✅ Gemini prompt includes title instruction
- ✅ Enrichment logic ensures titles are always populated
- ✅ Documentation updated for frontend integration

## Related Files

- `BACKEND_CQRS.Domain\Dto\AI\GeminiSprintPlanResponseDto.cs`
- `BACKEND_CQRS.Application\Dto\AI\GeminiSprintPlanResponseDto.cs`
- `BACKEND_CQRS.Infrastructure\Services\GeminiAIService.cs`
- `AI_SPRINT_PLANNER_API.md`
- `API_SPRINT_PLANNING_COMBINED_DOC.md`

---

**Implementation Date**: November 2, 2025
**Status**: ✅ Complete and Ready for Testing
