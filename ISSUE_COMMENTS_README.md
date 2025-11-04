# Issue Comments API - Complete Implementation

## ?? Overview

A fully functional **Issue Comments API** implemented using **CQRS (Command Query Responsibility Segregation)** architecture for the PMT User Backend project. This implementation allows users to create, read, update, and delete comments on issues with support for user mentions.

## ? Features

### Core Functionality
- ? **Create Comments** - Add comments to issues with optional user mentions
- ? **Read Comments** - Retrieve comments by issue ID or individual comment ID
- ? **Update Comments** - Edit comment text and manage mentions
- ? **Delete Comments** - Remove comments with cascading deletion of mentions
- ? **User Mentions** - Tag multiple users in comments
- ? **Author Information** - Include author name and avatar in responses
- ? **Audit Trail** - Track creation and modification timestamps

### Technical Features
- ? **CQRS Architecture** - Separation of commands and queries
- ? **MediatR Integration** - Clean command/query handling
- ? **Entity Framework Core** - PostgreSQL database integration
- ? **JWT Authentication** - Secured endpoints
- ? **Validation** - Input validation and error handling
- ? **Cascading Deletes** - Automatic cleanup of related data
- ? **AutoMapper Support** - Object mapping configuration
- ? **RESTful Design** - Standard HTTP methods and status codes

## ?? Project Structure

```
BACKEND_CQRS.Application/
??? Command/
?   ??? CreateIssueCommentCommand.cs         # Command to create a comment
?   ??? UpdateIssueCommentCommand.cs         # Command to update a comment
?   ??? DeleteIssueCommentCommand.cs         # Command to delete a comment
?
??? Query/
?   ??? IssueComments/
?       ??? GetCommentsByIssueIdQuery.cs     # Query to get all comments for an issue
?       ??? GetCommentByIdQuery.cs           # Query to get a specific comment
?
??? Handler/
?   ??? IssueComments/
?       ??? CreateIssueCommentCommandHandler.cs
?       ??? UpdateIssueCommentCommandHandler.cs
?       ??? DeleteIssueCommentCommandHandler.cs
?       ??? GetCommentsByIssueIdQueryHandler.cs
?       ??? GetCommentByIdQueryHandler.cs
?
??? Dto/
?   ??? IssueCommentDto.cs                   # Comment data transfer object
?   ??? CreateIssueCommentDto.cs             # Creation response DTO
?   ??? MentionDto.cs                        # Mention data transfer object
?
??? MappingProfile/
    ??? IssueCommentProfile.cs               # AutoMapper configuration

BACKEND_CQRS.Domain/
??? Entities/
    ??? IssueComment.cs                      # Comment entity (already exists)
    ??? Mention.cs                           # Mention entity (already exists)

BACKEND_CQRS.Infrastructure/
??? Context/
    ??? AppDbContext.cs                      # Updated with IssueComment DbSet

BACKEND_CQRS.Api/
??? Controllers/
    ??? IssueController.cs                   # Extended with comment endpoints
```

## ?? API Endpoints

All endpoints require JWT authentication via `Authorization: Bearer {token}` header.

### 1. Create Comment
```http
POST /api/issue/{issueId}/comments
```

**Request Body:**
```json
{
  "authorId": 1,
  "body": "This is a comment",
  "mentionedUserIds": [2, 3]
}
```

**Response (201 Created):**
```json
{
  "status": 201,
  "message": "Comment created successfully",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "issueId": "123e4567-e89b-12d3-a456-426614174000",
    "body": "This is a comment",
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

### 2. Get All Comments for Issue
```http
GET /api/issue/{issueId}/comments
```

**Response (200 OK):**
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
          "mentionUserEmail": "jane@example.com"
        }
      ]
    }
  ]
}
```

### 3. Get Comment by ID
```http
GET /api/issue/comments/{commentId}
```

### 4. Update Comment
```http
PUT /api/issue/comments/{commentId}
```

**Request Body:**
```json
{
  "body": "Updated comment text",
  "updatedBy": 1,
  "mentionedUserIds": [3, 4]
}
```

### 5. Delete Comment
```http
DELETE /api/issue/comments/{commentId}
```

## ?? Database Schema

### issue_comments Table
| Column | Type | Description |
|--------|------|-------------|
| id | UUID | Primary key |
| issue_id | UUID | Foreign key to issues table |
| author_id | INTEGER | Foreign key to users table |
| mention_id | INTEGER | Foreign key to users table (legacy) |
| body | TEXT | Comment text |
| created_by | INTEGER | User who created the comment |
| updated_by | INTEGER | User who last updated the comment |
| created_at | TIMESTAMPTZ | Creation timestamp |
| updated_at | TIMESTAMPTZ | Last update timestamp |

### mentions Table
| Column | Type | Description |
|--------|------|-------------|
| id | UUID | Primary key |
| mention_user_id | INTEGER | User being mentioned |
| issue_comments_id | UUID | Foreign key to issue_comments |
| created_by | INTEGER | User who created the mention |
| updated_by | INTEGER | User who updated the mention |
| created_at | TIMESTAMPTZ | Creation timestamp |
| updated_at | TIMESTAMPTZ | Last update timestamp |

## ??? Architecture Details

### CQRS Pattern Implementation

#### Commands (Write Operations)
Commands modify state and return success/failure responses:
- `CreateIssueCommentCommand` ? `CreateIssueCommentCommandHandler`
- `UpdateIssueCommentCommand` ? `UpdateIssueCommentCommandHandler`
- `DeleteIssueCommentCommand` ? `DeleteIssueCommentCommandHandler`

#### Queries (Read Operations)
Queries retrieve data without modifying state:
- `GetCommentsByIssueIdQuery` ? `GetCommentsByIssueIdQueryHandler`
- `GetCommentByIdQuery` ? `GetCommentByIdQueryHandler`

### Key Design Decisions

1. **Cascading Deletes**: When a comment is deleted, all associated mentions are automatically removed
2. **Validation**: Issues and users are validated before creating comments
3. **Duplicate Handling**: Duplicate user IDs in mentions are automatically filtered
4. **Ordering**: Comments are returned in reverse chronological order (newest first)
5. **Nullable Mentions**: Comments can exist without mentions
6. **Audit Trail**: CreatedBy, UpdatedBy, CreatedAt, and UpdatedAt are tracked

## ?? Installation & Setup

### Prerequisites
- .NET 8 SDK
- PostgreSQL database
- Valid JWT authentication setup

### Steps

1. **Build the solution:**
```bash
dotnet build
```

2. **Run migrations (if needed):**
```bash
dotnet ef database update --project BACKEND_CQRS.Infrastructure
```

3. **Run the application:**
```bash
dotnet run --project BACKEND_CQRS.Api
```

4. **The API will be available at:**
```
https://localhost:5001
```

## ?? Usage Examples

### Using cURL

**Create a comment:**
```bash
curl -X POST https://localhost:5001/api/issue/550e8400-e29b-41d4-a716-446655440000/comments \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "authorId": 1,
    "body": "Great work on this issue!",
    "mentionedUserIds": [2, 3]
  }'
```

**Get comments:**
```bash
curl -X GET https://localhost:5001/api/issue/550e8400-e29b-41d4-a716-446655440000/comments \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Update a comment:**
```bash
curl -X PUT https://localhost:5001/api/issue/comments/660e8400-e29b-41d4-a716-446655440001 \
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
curl -X DELETE https://localhost:5001/api/issue/comments/660e8400-e29b-41d4-a716-446655440001 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## ?? Testing

### Postman Collection
Import the provided `Issue_Comments_Postman_Collection.json` file into Postman for ready-to-use API tests.

### Test Coverage
- ? Create comment with valid data
- ? Create comment with invalid issue ID
- ? Create comment with invalid author ID
- ? Create comment with mentions
- ? Get all comments for an issue
- ? Get single comment by ID
- ? Update comment text and mentions
- ? Delete comment and verify cascade
- ? Authentication requirements
- ? Error handling

See `ISSUE_COMMENTS_TESTING_GUIDE.md` for comprehensive testing instructions.

## ?? Documentation

- **API Documentation**: `ISSUE_COMMENTS_API_DOCUMENTATION.md`
- **Testing Guide**: `ISSUE_COMMENTS_TESTING_GUIDE.md`
- **Postman Collection**: `Issue_Comments_Postman_Collection.json`

## ?? Security

- All endpoints require JWT Bearer authentication
- Authorization header format: `Authorization: Bearer {token}`
- 401 Unauthorized returned for missing/invalid tokens
- User permissions should be validated at the application level

## ? Performance Considerations

- Comments are eager-loaded with author and mention information
- Indexes exist on `issue_id` and `issue_comments_id` for fast lookups
- Mentions use distinct filtering to prevent duplicates
- Cascading deletes are handled at the database level for efficiency

## ?? Error Handling

All endpoints return standardized error responses:

```json
{
  "status": <http_status_code>,
  "message": "<error_message>",
  "data": null,
  "errors": ["<detailed_error>"]
}
```

### Common HTTP Status Codes
- **200 OK** - Successful operation
- **201 Created** - Resource created
- **400 Bad Request** - Invalid input
- **401 Unauthorized** - Authentication required
- **404 Not Found** - Resource not found
- **500 Internal Server Error** - Server error

## ?? Future Enhancements

Potential improvements for future iterations:

1. **Pagination** - Add pagination for large comment lists
2. **Rich Text** - Support markdown or HTML formatting
3. **Attachments** - Allow file attachments in comments
4. **Reactions** - Add emoji reactions to comments
5. **Edit History** - Track comment revision history
6. **Real-time Updates** - WebSocket support for live comments
7. **Notifications** - Send notifications when users are mentioned
8. **Soft Delete** - Implement soft delete with restore capability
9. **Comment Threads** - Support nested replies
10. **Search** - Full-text search across comments

## ?? Support

For issues or questions:
1. Check the API documentation
2. Review the testing guide
3. Verify database schema matches expected structure
4. Check server logs for detailed error messages

## ? Implementation Checklist

- [x] Domain entities (IssueComment, Mention)
- [x] DTOs (IssueCommentDto, CreateIssueCommentDto, MentionDto)
- [x] Commands (Create, Update, Delete)
- [x] Queries (GetByIssueId, GetById)
- [x] Command handlers
- [x] Query handlers
- [x] Controller endpoints
- [x] DbContext configuration
- [x] Entity relationships
- [x] AutoMapper profile
- [x] API documentation
- [x] Testing guide
- [x] Postman collection
- [x] Error handling
- [x] Validation
- [x] Authentication integration
- [x] Build verification

## ?? Summary

This implementation provides a complete, production-ready Issue Comments API using CQRS architecture. All components are properly structured, documented, and ready for use. The code follows best practices and integrates seamlessly with the existing PMT User Backend project.

**Status**: ? **FULLY IMPLEMENTED AND TESTED**

---

**Created by**: GitHub Copilot  
**Date**: January 2024  
**Version**: 1.0.0
