# AI Sprint Planner API Documentation

## Endpoint

```
POST /api/sprints/projects/{projectId}/ai-plan
```

## Description

Generates an AI-powered sprint plan for the specified project, selecting issues, estimating story points, and providing recommendations and risk analysis. Integrates Gemini AI to optimize sprint planning based on backlog, team velocity, and sprint goals.

---

## Request

### Path Parameters

| Name      | Type   | Required | Description                          |
| --------- | ------ | -------- | ------------------------------------ |
| projectId | string | Yes      | The unique identifier of the project |

### Request Body (JSON)

All fields are optional. The API gracefully handles missing/null values.

| Field             | Type    | Required | Description                         |
| ----------------- | ------- | -------- | ----------------------------------- |
| sprintGoal        | string  | No       | The goal for the sprint             |
| startDate         | string  | No       | Sprint start date (ISO 8601 format) |
| endDate           | string  | No       | Sprint end date (ISO 8601 format)   |
| status            | string  | No       | Sprint status                       |
| targetStoryPoints | integer | No       | Target story points for the sprint  |
| teamId            | integer | No       | Team identifier                     |

Example minimal request:

```json
{}
```

Example full request:

```json
{
  "sprintGoal": "Complete front-end integration",
  "startDate": "2025-11-05",
  "endDate": "2025-11-19",
  "status": "Planned",
  "targetStoryPoints": 40,
  "teamId": 1001
}
```

---

## Response

### Success (200)

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
          "rationale": "Highest priority CRITICAL issue that aligns with the sprint goal of front-end work (though labeled backend, integration implies front-end impact)."
        }
        // ...more issues...
      ],
      "totalStoryPoints": 36,
      "summary": "This sprint plan focuses on completing critical front-end bugs and high-priority tasks to align with the sprint goal. It includes the highest priority issues (CRITICAL and HIGH) first, then incorporates medium priority items to reach a balanced workload close to the target of 40 story points. The selected issues are a mix of BUG, STORY, and TASK types, with a strong emphasis on front-end related items where possible. The total story points selected are 36, which is within a reasonable range of the target and considers the team's historically low velocity.",
      "recommendations": [
        {
          "type": "capacity",
          "severity": "critical",
          "message": "Team velocity is consistently 0. The team has not completed any story points in recent sprints. This plan assumes a significant improvement or a change in how work is estimated/completed. A velocity of 0 is unsustainable and requires immediate investigation."
        }
        // ...more recommendations...
      ],
      "capacityAnalysis": {
        "teamCapacityUtilization": 90,
        "estimatedCompletionProbability": 20,
        "riskFactors": [
          "Extremely low historical velocity (0 points)",
          "High dependency on successful completion of 'Integrate Notification API' (PHX-202)",
          "Potential for scope creep or underestimation of complex tasks",
          "Lack of recent demonstrated delivery",
          "Potential for blockers or unforeseen issues in critical path items"
        ]
      }
    }
  },
  "message": "Sprint plan generated successfully"
}
```

---

## Error Responses

### 400 Bad Request

- **Missing or invalid projectId**
  ```json
  { "status": 400, "message": "Invalid or missing projectId." }
  ```
- **Invalid date format**
  ```json
  {
    "status": 400,
    "message": "Invalid date format. Use ISO 8601 (YYYY-MM-DD)."
  }
  ```
- **Invalid targetStoryPoints**
  ```json
  {
    "status": 400,
    "message": "Target story points must be a positive integer."
  }
  ```
- **Invalid teamId**
  ```json
  {
    "status": 400,
    "message": "Specified team does not exist or is not part of the project."
  }
  ```

### 404 Not Found

- **Project not found**
  ```json
  { "status": 404, "message": "Project not found." }
  ```

### 500 Internal Server Error

- **AI service failure**
  ```json
  {
    "status": 500,
    "message": "Failed to generate sprint plan due to AI service error."
  }
  ```
- **Unexpected error**
  ```json
  {
    "status": 500,
    "message": "An unexpected error occurred. Please try again later."
  }
  ```

---

## Notes for Frontend Integration

- All request fields are optional; send only what is available.
- Dates must be in ISO 8601 format (`YYYY-MM-DD`).
- Error messages are returned in the `message` field with appropriate HTTP status codes.
- On success, the response includes a detailed sprint plan, recommendations, and risk analysis.
- Handle all error cases gracefully in the frontend, displaying the `message` to the user.
