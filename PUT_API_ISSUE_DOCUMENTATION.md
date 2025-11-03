# PUT /api/Issue API Documentation

## Purpose

Updates an existing issue (task, bug, story, etc.) in the project backlog. This endpoint is used to modify issue details such as title, description, type, priority, story points, assignee, labels, and status.

---

## Endpoint

```
PUT /api/Issue
```

---

## Request

### Method
- **PUT**

### URL
- `/api/Issue`

### Headers
- `Authorization: Bearer <token>` (required)
- `Content-Type: application/json`

### Request Body

All fields except `id` are optional and can be null. Only the fields provided will be updated.

| Field         | Type     | Required | Nullable | Description                                 |
|---------------|----------|----------|----------|---------------------------------------------|
| id            | string   | Yes      | No       | Unique identifier (GUID) of the issue       |
| title         | string   | No       | Yes      | Title of the issue                          |
| description   | string   | No       | Yes      | Detailed description                        |
| issueType     | string   | No       | Yes      | Type (e.g., Story, Bug, Task)               |
| priority      | string   | No       | Yes      | Priority (e.g., CRITICAL, HIGH, MEDIUM, LOW)|
| storyPoints   | integer  | No       | Yes      | Story points for the issue                  |
| assigneeId    | string   | No       | Yes      | User ID of the assignee                     |
| status        | string   | No       | Yes      | Issue status (e.g., Backlog, In Progress)   |
| labels        | array    | No       | Yes      | List of labels/tags                         |
| epicId        | string   | No       | Yes      | Epic ID if the issue is part of an epic     |
| parentIssueId | string   | No       | Yes      | Parent issue ID for sub-tasks               |

#### Example Request
```json
{
  "id": "issue-guid",
  "title": "Update login page UI",
  "description": "Improve the login page for better UX.",
  "issueType": "Story",
  "priority": "HIGH",
  "storyPoints": 5,
  "assigneeId": "user-guid",
  "status": "In Progress",
  "labels": ["frontend", "ui"],
  "epicId": "epic-guid",
  "parentIssueId": "parent-issue-guid"
}
```

---

## Response

### Success (200 OK)

```json
{
  "succeeded": true,
  "statusCode": 200,
  "data": {
    "id": "issue-guid",
    "title": "Update login page UI",
    "description": "Improve the login page for better UX.",
    "issueType": "Story",
    "priority": "HIGH",
    "storyPoints": 5,
    "assigneeId": "user-guid",
    "status": "In Progress",
    "labels": ["frontend", "ui"],
    "epicId": "epic-guid",
    "parentIssueId": "parent-issue-guid"
  }
}
```

---

## Error Responses

All error messages are returned in the `message` field with the appropriate HTTP status code.

### 400 Bad Request
- Missing or invalid `id`:
  ```json
  { "succeeded": false, "statusCode": 400, "message": "Invalid or missing issue ID." }
  ```
- Invalid field values (e.g., invalid priority, issueType, status):
  ```json
  { "succeeded": false, "statusCode": 400, "message": "Invalid value for field 'priority'." }
  ```
- Invalid storyPoints:
  ```json
  { "succeeded": false, "statusCode": 400, "message": "Story points must be a positive integer." }
  ```

### 401 Unauthorized
- Missing or invalid token:
  ```json
  { "succeeded": false, "statusCode": 401, "message": "Unauthorized. Invalid or missing token." }
  ```

### 404 Not Found
- Issue not found:
  ```json
  { "succeeded": false, "statusCode": 404, "message": "Issue not found." }
  ```
- Assignee, epic, or parent issue not found:
  ```json
  { "succeeded": false, "statusCode": 404, "message": "Assignee, epic, or parent issue not found." }
  ```

### 409 Conflict
- Duplicate title in the same project:
  ```json
  { "succeeded": false, "statusCode": 409, "message": "Duplicate issue title in project." }
  ```

### 500 Internal Server Error
- Unexpected server error:
  ```json
  { "succeeded": false, "statusCode": 500, "message": "An unexpected error occurred. Please try again later." }
  ```

---

## Usage Notes
- Only fields provided in the request will be updated; others remain unchanged.
- All endpoints require a valid JWT Bearer token.
- Error messages are user-friendly and suitable for frontend display.
- Labels, epicId, and parentIssueId are optional and can be omitted if not relevant.

---

**Last Updated:** November 2, 2025
