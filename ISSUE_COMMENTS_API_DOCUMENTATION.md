# Issue Comments API Documentation

## Overview
This document describes the Issue Comments API endpoints implemented using CQRS architecture for the PMT User Backend.

## Base URL
`/api/issue`

---

## Endpoints

### 1. Create Issue Comment
**POST** `/api/issue/{issueId}/comments`

Creates a new comment for a specific issue.

#### Request
**Path Parameters:**
- `issueId` (Guid): The ID of the issue to comment on

**Headers:**
- `Authorization: Bearer {token}` (Required)

**Body:**
```json
{
  "body": "This is a comment on the issue",
  "authorId": 1,
  "mentionedUserIds": [2, 3, 5]
}
```

**Fields:**
- `body` (string, required): The comment text
- `authorId` (int, required): The ID of the user creating the comment
- `mentionedUserIds` (array of int, optional): List of user IDs to mention in the comment

#### Response
**Success (201 Created):**
```json
{
  "status": 201,
  "message": "Comment created successfully",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "issueId": "123e4567-e89b-12d3-a456-426614174000",
    "body": "This is a comment on the issue",
    "createdAt": "2024-01-15T10:30:00Z"
  },
  "errors": null
}
```

**Error (400 Bad Request):**
```json
{
  "status": 400,
  "message": "Issue not found",
  "data": null,
  "errors": ["Issue not found"]
}
```

---

### 2. Get Comments by Issue ID
**GET** `/api/issue/{issueId}/comments`

Retrieves all comments for a specific issue, ordered by creation date (newest first).

#### Request
**Path Parameters:**
- `issueId` (Guid): The ID of the issue

**Headers:**
- `Authorization: Bearer {token}` (Required)

#### Response
**Success (200 OK):**
```json
{
  "status": 200,
  "message": "Comments retrieved successfully",
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "issueId": "123e4567-e89b-12d3-a456-426614174000",
      "authorId": 1,
      "authorName": "John Doe",
      "authorAvatarUrl": "https://example.com/avatar.jpg",
      "body": "This is a comment",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-15T10:30:00Z",
      "mentions": [
        {
          "id": "660e8400-e29b-41d4-a716-446655440001",
          "mentionUserId": 2,
          "mentionUserName": "Jane Smith",
          "mentionUserEmail": "jane.smith@example.com"
        }
      ]
    }
  ],
  "errors": null
}
```

---

### 3. Get Comment by ID
**GET** `/api/issue/comments/{commentId}`

Retrieves a specific comment by its ID.

#### Request
**Path Parameters:**
- `commentId` (Guid): The ID of the comment

**Headers:**
- `Authorization: Bearer {token}` (Required)

#### Response
**Success (200 OK):**
```json
{
  "status": 200,
  "message": "Comment retrieved successfully",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "issueId": "123e4567-e89b-12d3-a456-426614174000",
    "authorId": 1,
    "authorName": "John Doe",
    "authorAvatarUrl": "https://example.com/avatar.jpg",
    "body": "This is a comment",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z",
    "mentions": []
  },
  "errors": null
}
```

**Error (404 Not Found):**
```json
{
  "status": 404,
  "message": "Comment not found",
  "data": null,
  "errors": ["Comment not found"]
}
```

---

### 4. Update Issue Comment
**PUT** `/api/issue/comments/{commentId}`

Updates an existing comment.

#### Request
**Path Parameters:**
- `commentId` (Guid): The ID of the comment to update

**Headers:**
- `Authorization: Bearer {token}` (Required)

**Body:**
```json
{
  "body": "Updated comment text",
  "updatedBy": 1,
  "mentionedUserIds": [3, 4]
}
```

**Fields:**
- `body` (string, required): The updated comment text
- `updatedBy` (int, required): The ID of the user updating the comment
- `mentionedUserIds` (array of int, optional): Updated list of user IDs to mention

#### Response
**Success (200 OK):**
```json
{
  "status": 200,
  "message": "Comment updated successfully",
  "data": "550e8400-e29b-41d4-a716-446655440000",
  "errors": null
}
```

**Error (404 Not Found):**
```json
{
  "status": 404,
  "message": "Comment not found",
  "data": null,
  "errors": ["Comment not found"]
}
```

---

### 5. Delete Issue Comment
**DELETE** `/api/issue/comments/{commentId}`

Deletes a comment and all associated mentions.

#### Request
**Path Parameters:**
- `commentId` (Guid): The ID of the comment to delete

**Headers:**
- `Authorization: Bearer {token}` (Required)

#### Response
**Success (200 OK):**
```json
{
  "status": 200,
  "message": "Comment deleted successfully",
  "data": "550e8400-e29b-41d4-a716-446655440000",
  "errors": null
}
```

**Error (404 Not Found):**
```json
{
  "status": 404,
  "message": "Comment not found",
  "data": null,
  "errors": ["Comment not found"]
}
```

---

## Architecture

### CQRS Pattern
This API follows the CQRS (Command Query Responsibility Segregation) pattern:

#### Commands (Write Operations)
- **CreateIssueCommentCommand** - Creates a new comment
- **UpdateIssueCommentCommand** - Updates an existing comment
- **DeleteIssueCommentCommand** - Deletes a comment

#### Queries (Read Operations)
- **GetCommentsByIssueIdQuery** - Retrieves all comments for an issue
- **GetCommentByIdQuery** - Retrieves a specific comment

### Project Structure
```
BACKEND_CQRS.Application/
??? Command/
?   ??? CreateIssueCommentCommand.cs
?   ??? UpdateIssueCommentCommand.cs
?   ??? DeleteIssueCommentCommand.cs
??? Query/
?   ??? IssueComments/
?       ??? GetCommentsByIssueIdQuery.cs
?       ??? GetCommentByIdQuery.cs
??? Handler/
?   ??? IssueComments/
?       ??? CreateIssueCommentCommandHandler.cs
?       ??? UpdateIssueCommentCommandHandler.cs
?       ??? DeleteIssueCommentCommandHandler.cs
?       ??? GetCommentsByIssueIdQueryHandler.cs
?       ??? GetCommentByIdQueryHandler.cs
??? Dto/
    ??? IssueCommentDto.cs
    ??? CreateIssueCommentDto.cs
    ??? MentionDto.cs
```

---

## Database Schema

### issue_comments Table
```sql
CREATE TABLE issue_comments (
  id UUID PRIMARY KEY,
  issue_id UUID NOT NULL REFERENCES issues(id) ON DELETE CASCADE,
  author_id INTEGER NOT NULL REFERENCES users(id),
  mention_id INTEGER NOT NULL REFERENCES users(id),
  body TEXT NOT NULL,
  created_by INTEGER REFERENCES users(id),
  updated_by INTEGER REFERENCES users(id),
  created_at TIMESTAMPTZ NOT NULL,
  updated_at TIMESTAMPTZ NOT NULL
);
```

### mentions Table
```sql
CREATE TABLE mentions (
  id UUID PRIMARY KEY,
  mention_user_id INTEGER REFERENCES users(id),
  issue_comments_id UUID REFERENCES issue_comments(id) ON DELETE CASCADE,
  created_by INTEGER REFERENCES users(id),
  updated_by INTEGER REFERENCES users(id),
  created_at TIMESTAMPTZ NOT NULL,
  updated_at TIMESTAMPTZ NOT NULL
);
```

---

## Features

### ? Implemented Features
1. **Create Comments** - Add comments to issues with optional user mentions
2. **Read Comments** - Retrieve comments by issue or comment ID
3. **Update Comments** - Edit comment text and update mentions
4. **Delete Comments** - Remove comments and associated mentions
5. **User Mentions** - Tag multiple users in comments
6. **Author Information** - Include author name and avatar in responses
7. **Cascading Deletes** - Automatically delete mentions when comments are deleted
8. **Validation** - Validate issue and user existence before creating comments
9. **Audit Trail** - Track who created and updated comments

### ?? Security
- All endpoints require authentication via JWT Bearer token
- User authorization should be implemented at the application level

---

## Error Handling

All endpoints return standard error responses:

```json
{
  "status": <http_status_code>,
  "message": "<error_message>",
  "data": null,
  "errors": ["<detailed_error_1>", "<detailed_error_2>"]
}
```

Common HTTP Status Codes:
- **200 OK** - Successful operation
- **201 Created** - Resource created successfully
- **400 Bad Request** - Invalid input or validation error
- **401 Unauthorized** - Missing or invalid authentication token
- **404 Not Found** - Resource not found
- **500 Internal Server Error** - Server error

---

## Example Usage

### Using cURL

**Create a comment:**
```bash
curl -X POST https://api.example.com/api/issue/123e4567-e89b-12d3-a456-426614174000/comments \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "body": "Great work on this issue!",
    "authorId": 1,
    "mentionedUserIds": [2, 3]
  }'
```

**Get comments:**
```bash
curl -X GET https://api.example.com/api/issue/123e4567-e89b-12d3-a456-426614174000/comments \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Update a comment:**
```bash
curl -X PUT https://api.example.com/api/issue/comments/550e8400-e29b-41d4-a716-446655440000 \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "body": "Updated comment text",
    "updatedBy": 1,
    "mentionedUserIds": [2]
  }'
```

**Delete a comment:**
```bash
curl -X DELETE https://api.example.com/api/issue/comments/550e8400-e29b-41d4-a716-446655440000 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## Testing Checklist

- [ ] Create comment with valid data
- [ ] Create comment with invalid issue ID
- [ ] Create comment with invalid author ID
- [ ] Create comment with mentions
- [ ] Get all comments for an issue
- [ ] Get comment by ID
- [ ] Get non-existent comment
- [ ] Update comment text
- [ ] Update comment mentions
- [ ] Update non-existent comment
- [ ] Delete comment
- [ ] Delete non-existent comment
- [ ] Verify mentions are deleted when comment is deleted
- [ ] Test authentication requirements
- [ ] Test with empty comment body
- [ ] Test with very long comment body

---

## Future Enhancements

1. **Pagination** - Add pagination for comments list
2. **Sorting** - Allow custom sorting (by date, author, etc.)
3. **Filtering** - Filter comments by author or date range
4. **Reactions** - Add emoji reactions to comments
5. **Edit History** - Track comment edit history
6. **Rich Text** - Support markdown or rich text formatting
7. **File Attachments** - Allow attaching files to comments
8. **Real-time Updates** - WebSocket support for real-time comment updates
9. **Notifications** - Send notifications when users are mentioned
10. **Soft Delete** - Implement soft delete instead of hard delete
