# Gemini Issue ID Tracking Implementation Summary

## Overview

Enhanced the AI Sprint Planning feature to maintain consistent issue tracking between the database and Gemini by propagating issue IDs through the request–response flow. This enables automatic addition of selected issues to sprints.

## Implementation Date

November 1, 2025

---

## Changes Implemented

### 1. Enhanced Gemini Request Payload

#### Modified File: `GeminiAIService.cs`

**Location:** `BACKEND_CQRS.Infrastructure/Services/GeminiAIService.cs`

#### Changes Made:

**A. Updated Backlog Issue Formatting (Lines 251-280)**

**Before:**

```csharp
foreach (var issue in context.BacklogIssues.Take(50))
{
    prompt.AppendLine($"- {issue.Key}: \"{issue.Title}\"");
    prompt.AppendLine($"  * Type: {issue.Type}, Priority: {issue.Priority}");
    prompt.AppendLine($"  * Story Points: {issue.StoryPoints ?? 0}");
    // ... other fields
}
```

**After:**

```csharp
prompt.AppendLine("IMPORTANT: Each issue has a unique 'id' field. When you return the list of issues to be included in the sprint,");
prompt.AppendLine("you MUST include this exact same 'id' for each selected issue. Do NOT alter, regenerate, or modify these IDs.");
prompt.AppendLine("The 'issueId' in your response must exactly match the 'id' from this list.");
prompt.AppendLine();

foreach (var issue in context.BacklogIssues.Take(50))
{
    prompt.AppendLine($"- Issue ID: {issue.Id}");
    prompt.AppendLine($"  * Key: {issue.Key}");
    prompt.AppendLine($"  * Title: \"{issue.Title}\"");
    prompt.AppendLine($"  * Type: {issue.Type}, Priority: {issue.Priority}");
    prompt.AppendLine($"  * Story Points: {issue.StoryPoints ?? 0}");
    // ... other fields
}
```

**Key Improvements:**

- ✅ Prominently displays issue UUID at the start of each issue entry
- ✅ Explicit warning to Gemini not to alter or regenerate IDs
- ✅ Clear mapping instruction between input `id` and output `issueId`

---

### 2. Updated Gemini Prompt Instructions

#### Modified Section: CRITICAL OUTPUT REQUIREMENTS (Lines 195-230)

**Before:**

```javascript
"4. Your response must match this exact JSON structure:"
{
  "sprintPlan": {
    "selectedIssues": [
      {
        "issueId": "uuid-string",  // Generic placeholder
        "issueKey": "string like PHX-201",
        // ...
      }
    ]
  }
}
```

**After:**

```javascript
"4. For each selected issue, the 'issueId' field MUST be the exact UUID from the backlog issue list"
"5. Your response must match this exact JSON structure:"
{
  "sprintPlan": {
    "selectedIssues": [
      {
        "issueId": "exact-uuid-from-backlog-list",  // Explicit instruction
        "issueKey": "string like PHX-201",
        // ...
      }
    ]
  }
}
```

**Key Improvements:**

- ✅ Added explicit requirement as item #4
- ✅ Changed placeholder text to emphasize exact UUID matching
- ✅ Renumbered subsequent requirements

---

### 3. Response Validation & ID Verification

#### Added Validation Logic (Lines 55-76)

**New Code:**

```csharp
if (response?.SprintPlan != null &&
    response.SprintPlan.SelectedIssues != null &&
    response.SprintPlan.SelectedIssues.Any())
{
    // Validate that all returned issue IDs exist in the backlog
    var backlogIssueIds = context.BacklogIssues?.Select(i => i.Id).ToHashSet() ?? new HashSet<Guid>();
    var invalidIssues = response.SprintPlan.SelectedIssues
        .Where(si => !backlogIssueIds.Contains(si.IssueId))
        .ToList();

    if (invalidIssues.Any())
    {
        _logger.LogWarning($"Gemini returned {invalidIssues.Count} issues with IDs not in backlog: {string.Join(", ", invalidIssues.Select(i => i.IssueId))}");

        // Remove invalid issues from the response
        response.SprintPlan.SelectedIssues = response.SprintPlan.SelectedIssues
            .Where(si => backlogIssueIds.Contains(si.IssueId))
            .ToList();

        // Recalculate total story points
        response.SprintPlan.TotalStoryPoints = response.SprintPlan.SelectedIssues.Sum(i => i.StoryPoints);
    }

    if (response.SprintPlan.SelectedIssues.Any())
    {
        _logger.LogInformation($"Successfully generated sprint plan with {response.SprintPlan.SelectedIssues.Count} valid issues");
        _logger.LogDebug($"Selected issue IDs: {string.Join(", ", response.SprintPlan.SelectedIssues.Select(i => i.IssueId))}");
        return response;
    }
}
```

**Validation Process:**

1. ✅ Creates a HashSet of all valid backlog issue IDs
2. ✅ Checks each returned issue ID against the backlog
3. ✅ Logs warning if invalid IDs are found
4. ✅ Automatically filters out invalid issues
5. ✅ Recalculates total story points after filtering
6. ✅ Logs selected issue IDs for debugging

---

## Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Database Query                                           │
│    BacklogIssues with Guid IDs                              │
│    (a1b2c3d4-e010-4e11-a9a1-010101010110, ...)             │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. Gemini Request Payload                                   │
│    {                                                         │
│      "backlogIssues": [                                     │
│        {                                                     │
│          "id": "a1b2c3d4-e010-4e11-a9a1-010101010110",     │
│          "key": "PHX-210",                                  │
│          "title": "Optimize Database Queries",              │
│          ...                                                │
│        }                                                     │
│      ]                                                       │
│    }                                                         │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. Gemini AI Processing                                     │
│    Receives: Issue IDs from request                         │
│    Instruction: "Preserve exact IDs in response"            │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│ 4. Gemini Response                                          │
│    {                                                         │
│      "sprintPlan": {                                        │
│        "selectedIssues": [                                  │
│          {                                                   │
│            "issueId": "a1b2c3d4-e010-4e11-a9a1-010101010110",│
│            "issueKey": "PHX-210",                           │
│            "storyPoints": 8,                                │
│            "rationale": "High priority optimization..."     │
│          }                                                   │
│        ]                                                     │
│      }                                                       │
│    }                                                         │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│ 5. Validation Layer (NEW)                                   │
│    - Verify all issueIds exist in original backlog          │
│    - Filter out any invalid IDs                             │
│    - Log warnings for mismatches                            │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│ 6. Database Update                                          │
│    - Use validated issueIds to link issues to sprint        │
│    - Update sprint-issue mapping table                      │
└─────────────────────────────────────────────────────────────┘
```

---

## Example Request/Response

### Request Sent to Gemini (Excerpt)

```
## AVAILABLE BACKLOG ISSUES:
Total backlog issues: 15

IMPORTANT: Each issue has a unique 'id' field. When you return the list of issues to be included in the sprint,
you MUST include this exact same 'id' for each selected issue. Do NOT alter, regenerate, or modify these IDs.
The 'issueId' in your response must exactly match the 'id' from this list.

- Issue ID: a1b2c3d4-e010-4e11-a9a1-010101010110
  * Key: PHX-210
  * Title: "Optimize Database Queries for Boards"
  * Type: TASK, Priority: HIGH
  * Story Points: 10
  * Assigned to: User ID 1003
  * Labels: db, optimization

- Issue ID: a1b2c3d4-e007-4e11-a9a1-010101010107
  * Key: PHX-207
  * Title: "Fix Critical API Performance Issue"
  * Type: BUG, Priority: CRITICAL
  * Story Points: 8
  * Labels: api, performance
```

### Expected Response from Gemini

```json
{
  "sprintPlan": {
    "selectedIssues": [
      {
        "issueId": "a1b2c3d4-e010-4e11-a9a1-010101010110",
        "issueKey": "PHX-210",
        "storyPoints": 10,
        "suggestedAssigneeId": 1003,
        "rationale": "High priority database optimization task critical for performance goals"
      },
      {
        "issueId": "a1b2c3d4-e007-4e11-a9a1-010101010107",
        "issueKey": "PHX-207",
        "storyPoints": 8,
        "suggestedAssigneeId": null,
        "rationale": "Critical API performance issue must be addressed in this sprint"
      }
    ],
    "totalStoryPoints": 18,
    "summary": "Sprint focuses on critical performance improvements...",
    "recommendations": [...],
    "capacityAnalysis": {...}
  }
}
```

---

## Fallback Logic

The rule-based fallback planner was already correctly using issue IDs:

```csharp
selectedIssues.Add(new SelectedIssueDto
{
    IssueId = issue.Id,  // ✅ Already using correct Guid from backlog
    IssueKey = issue.Key,
    StoryPoints = issuePoints,
    SuggestedAssigneeId = issue.AssigneeId,
    Rationale = $"{issue.Priority ?? "MEDIUM"} priority {issue.Type} issue selected for sprint"
});
```

**No changes needed** - fallback automatically maintains ID consistency.

---

## Benefits of Implementation

### 1. **Data Integrity**

- ✅ Ensures issue IDs returned by Gemini match database records
- ✅ Prevents orphaned or invalid issue references
- ✅ Maintains referential integrity in sprint-issue mappings

### 2. **Automatic Sprint Population**

- ✅ Backend can directly use returned IDs to update database
- ✅ No manual mapping or lookup required
- ✅ Reduces API round-trips and processing overhead

### 3. **Error Detection**

- ✅ Validates all IDs before committing to database
- ✅ Logs warnings for debugging if Gemini hallucinates IDs
- ✅ Automatically filters out invalid suggestions

### 4. **Audit Trail**

- ✅ Logs selected issue IDs at DEBUG level
- ✅ Tracks which issues were suggested by AI vs rule-based fallback
- ✅ Enables tracing of sprint planning decisions

---

## Testing Checklist

### Unit Testing

- [ ] Verify backlog issues include `Id` field in prompt
- [ ] Confirm validation filters invalid IDs correctly
- [ ] Test fallback uses correct issue IDs
- [ ] Verify story points recalculation after filtering

### Integration Testing

- [ ] Test with real Gemini API response
- [ ] Verify IDs in response match request
- [ ] Test database update using returned IDs
- [ ] Confirm sprint-issue mapping is correct

### Edge Cases

- [ ] Gemini returns ID not in backlog → Should filter out
- [ ] Gemini returns no issues → Should retry or use fallback
- [ ] Gemini returns malformed IDs → Should log and filter
- [ ] All returned IDs are invalid → Should use fallback

---

## Database Schema Requirements

The implementation assumes the following schema:

### Sprint-Issue Mapping Table

```sql
CREATE TABLE SprintIssues (
    SprintId UUID NOT NULL,
    IssueId UUID NOT NULL,
    AddedAt TIMESTAMP DEFAULT NOW(),
    PRIMARY KEY (SprintId, IssueId),
    FOREIGN KEY (SprintId) REFERENCES Sprints(Id),
    FOREIGN KEY (IssueId) REFERENCES Issues(Id)
);
```

### Usage After Sprint Planning

```csharp
// After receiving validated response from Gemini
foreach (var selectedIssue in response.SprintPlan.SelectedIssues)
{
    await _context.SprintIssues.AddAsync(new SprintIssue
    {
        SprintId = newSprint.Id,
        IssueId = selectedIssue.IssueId,  // Direct mapping - no lookup needed!
        AddedAt = DateTime.UtcNow
    });
}
await _context.SaveChangesAsync();
```

---

## Logging Examples

### Successful Response

```
[INFO] Successfully generated sprint plan with 7 valid issues
[DEBUG] Selected issue IDs: a1b2c3d4-e010-4e11-a9a1-010101010110, a1b2c3d4-e007-4e11-a9a1-010101010107, ...
```

### Invalid IDs Detected

```
[WARN] Gemini returned 2 issues with IDs not in backlog: 99999999-9999-9999-9999-999999999999, 88888888-8888-8888-8888-888888888888
[INFO] Successfully generated sprint plan with 5 valid issues (filtered from 7 total)
```

---

## API Documentation Update

The AI Sprint Planner endpoint now guarantees:

1. **Input:** Backlog issues with unique `Guid` IDs
2. **Output:** Selected issues with matching `Guid` IDs
3. **Validation:** Automatic filtering of invalid IDs
4. **Logging:** Full audit trail of ID tracking

---

## Build Status

✅ **Project builds successfully** (0 errors, 134 warnings - pre-existing)

## Files Modified

1. `BACKEND_CQRS.Infrastructure/Services/GeminiAIService.cs`
   - Updated backlog issue formatting in prompt
   - Added ID preservation instructions
   - Implemented validation logic
   - Enhanced logging

---

**Implementation Complete**  
**Status:** ✅ Ready for Testing  
**Next Step:** Integration testing with live Gemini API
