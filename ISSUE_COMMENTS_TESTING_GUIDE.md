# Issue Comments API - Testing Guide

## Prerequisites
1. Backend server running
2. Valid JWT authentication token
3. At least one project, issue, and user in the database
4. API testing tool (Postman, Insomnia, or cURL)

## Test Environment Setup

### Base Configuration
- **Base URL:** `https://localhost:5001/api/issue` (adjust port as needed)
- **Authentication:** Bearer Token in Authorization header

### Test Data Requirements
- Valid Issue ID (Guid format)
- Valid User IDs (Integer format)
- Valid Comment IDs (Guid format after creation)

---

## Test Scenarios

### 1. CREATE COMMENT - Success Case

**Endpoint:** `POST /api/issue/{issueId}/comments`

**Prerequisites:**
- Get a valid `issueId` from your database
- Get a valid `authorId` from your database

**Request:**
```http
POST /api/issue/550e8400-e29b-41d4-a716-446655440000/comments
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "authorId": 1,
  "body": "This is a test comment for the issue",
  "mentionedUserIds": [2, 3]
}
```

**Expected Response:** `201 Created`
```json
{
  "status": 201,
  "message": "Comment created successfully",
  "data": {
    "id": "generated-guid-here",
    "issueId": "550e8400-e29b-41d4-a716-446655440000",
    "body": "This is a test comment for the issue",
    "createdAt": "2024-01-15T10:30:00Z"
  },
  "errors": null
}
```

**Save the returned `id` for subsequent tests!**

---

### 2. CREATE COMMENT - Invalid Issue ID

**Request:**
```http
POST /api/issue/00000000-0000-0000-0000-000000000000/comments
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "authorId": 1,
  "body": "Comment on non-existent issue"
}
```

**Expected Response:** `400 Bad Request`
```json
{
  "status": 400,
  "message": "Issue not found",
  "data": null,
  "errors": ["Issue not found"]
}
```

---

### 3. CREATE COMMENT - Invalid Author ID

**Request:**
```http
POST /api/issue/{valid-issue-id}/comments
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "authorId": 99999,
  "body": "Comment with invalid author"
}
```

**Expected Response:** `400 Bad Request`
```json
{
  "status": 400,
  "message": "Author not found",
  "data": null,
  "errors": ["Author not found"]
}
```

---

### 4. CREATE COMMENT - Without Authentication

**Request:**
```http
POST /api/issue/{valid-issue-id}/comments
Content-Type: application/json

{
  "authorId": 1,
  "body": "Unauthenticated comment"
}
```

**Expected Response:** `401 Unauthorized`

---

### 5. GET ALL COMMENTS BY ISSUE ID - Success

**Endpoint:** `GET /api/issue/{issueId}/comments`

**Request:**
```http
GET /api/issue/550e8400-e29b-41d4-a716-446655440000/comments
Authorization: Bearer YOUR_JWT_TOKEN
```

**Expected Response:** `200 OK`
```json
{
  "status": 200,
  "message": "Comments retrieved successfully",
  "data": [
    {
      "id": "comment-guid-1",
      "issueId": "550e8400-e29b-41d4-a716-446655440000",
      "authorId": 1,
      "authorName": "John Doe",
      "authorAvatarUrl": "https://example.com/avatar.jpg",
      "body": "This is a test comment",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-15T10:30:00Z",
      "mentions": [
        {
          "id": "mention-guid",
          "mentionUserId": 2,
          "mentionUserName": "Jane Smith",
          "mentionUserEmail": "jane@example.com"
        }
      ]
    }
  ],
  "errors": null
}
```

---

### 6. GET ALL COMMENTS - Empty Result

**Request:**
```http
GET /api/issue/{issue-with-no-comments}/comments
Authorization: Bearer YOUR_JWT_TOKEN
```

**Expected Response:** `200 OK`
```json
{
  "status": 200,
  "message": "Comments retrieved successfully",
  "data": [],
  "errors": null
}
```

---

### 7. GET COMMENT BY ID - Success

**Endpoint:** `GET /api/issue/comments/{commentId}`

**Request:**
```http
GET /api/issue/comments/550e8400-e29b-41d4-a716-446655440000
Authorization: Bearer YOUR_JWT_TOKEN
```

**Expected Response:** `200 OK`
```json
{
  "status": 200,
  "message": "Comment retrieved successfully",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "issueId": "issue-guid",
    "authorId": 1,
    "authorName": "John Doe",
    "authorAvatarUrl": "https://example.com/avatar.jpg",
    "body": "This is a test comment",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z",
    "mentions": []
  },
  "errors": null
}
```

---

### 8. GET COMMENT BY ID - Not Found

**Request:**
```http
GET /api/issue/comments/00000000-0000-0000-0000-000000000000
Authorization: Bearer YOUR_JWT_TOKEN
```

**Expected Response:** `404 Not Found`
```json
{
  "status": 404,
  "message": "Comment not found",
  "data": null,
  "errors": ["Comment not found"]
}
```

---

### 9. UPDATE COMMENT - Success

**Endpoint:** `PUT /api/issue/comments/{commentId}`

**Request:**
```http
PUT /api/issue/comments/550e8400-e29b-41d4-a716-446655440000
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "body": "Updated comment text with new content",
  "updatedBy": 1,
  "mentionedUserIds": [2, 4, 5]
}
```

**Expected Response:** `200 OK`
```json
{
  "status": 200,
  "message": "Comment updated successfully",
  "data": "550e8400-e29b-41d4-a716-446655440000",
  "errors": null
}
```

---

### 10. UPDATE COMMENT - Not Found

**Request:**
```http
PUT /api/issue/comments/00000000-0000-0000-0000-000000000000
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "body": "Updated content",
  "updatedBy": 1
}
```

**Expected Response:** `404 Not Found`
```json
{
  "status": 404,
  "message": "Comment not found",
  "data": null,
  "errors": ["Comment not found"]
}
```

---

### 11. DELETE COMMENT - Success

**Endpoint:** `DELETE /api/issue/comments/{commentId}`

**Request:**
```http
DELETE /api/issue/comments/550e8400-e29b-41d4-a716-446655440000
Authorization: Bearer YOUR_JWT_TOKEN
```

**Expected Response:** `200 OK`
```json
{
  "status": 200,
  "message": "Comment deleted successfully",
  "data": "550e8400-e29b-41d4-a716-446655440000",
  "errors": null
}
```

**Verification:**
- Try to GET the deleted comment - should return 404
- Verify mentions are also deleted from database

---

### 12. DELETE COMMENT - Not Found

**Request:**
```http
DELETE /api/issue/comments/00000000-0000-0000-0000-000000000000
Authorization: Bearer YOUR_JWT_TOKEN
```

**Expected Response:** `404 Not Found`
```json
{
  "status": 404,
  "message": "Comment not found",
  "data": null,
  "errors": ["Comment not found"]
}
```

---

## Advanced Test Scenarios

### 13. Multiple Mentions Test

**Create a comment with multiple mentions:**
```json
{
  "authorId": 1,
  "body": "Great work @user2, @user3, and @user4!",
  "mentionedUserIds": [2, 3, 4]
}
```

**Verify:**
- All mentions are created
- All mentioned users appear in the response
- Duplicate user IDs are handled correctly

---

### 14. Update Mentions Test

**Initial comment with mentions:**
```json
{
  "authorId": 1,
  "body": "Hello @user2",
  "mentionedUserIds": [2]
}
```

**Update to different mentions:**
```json
{
  "body": "Hello @user3 and @user4",
  "updatedBy": 1,
  "mentionedUserIds": [3, 4]
}
```

**Verify:**
- Old mentions (user 2) are removed
- New mentions (users 3, 4) are added
- Response includes updated mentions

---

### 15. Cascading Delete Test

**Steps:**
1. Create a comment with mentions
2. Note the comment ID and verify mentions exist in database
3. Delete the comment
4. Verify mentions are also deleted from database

**SQL Query to verify:**
```sql
SELECT * FROM mentions WHERE issue_comments_id = 'your-comment-id';
-- Should return 0 rows after deletion
```

---

### 16. Long Comment Body Test

**Create comment with long text (test character limits):**
```json
{
  "authorId": 1,
  "body": "Lorem ipsum dolor sit amet, consectetur adipiscing elit... (very long text, e.g., 5000+ characters)",
  "mentionedUserIds": []
}
```

**Verify:**
- Comment is created successfully
- Full text is stored and retrieved correctly

---

### 17. Special Characters Test

**Create comment with special characters:**
```json
{
  "authorId": 1,
  "body": "Testing special chars: <html>, @mention, #hashtag, emojis ????, quotes \"test\", apostrophes 'test'",
  "mentionedUserIds": []
}
```

**Verify:**
- Special characters are stored correctly
- JSON escaping works properly
- Text is retrieved without corruption

---

### 18. Concurrent Updates Test

**Steps:**
1. Create a comment
2. Attempt to update the same comment from two different requests simultaneously
3. Verify both updates are processed correctly
4. Check that the last update wins

---

## Postman Collection Structure

```
Issue Comments API Tests
??? Setup
?   ??? Get Auth Token
??? Create Comment
?   ??? Success
?   ??? Invalid Issue
?   ??? Invalid Author
?   ??? With Mentions
?   ??? Without Auth
??? Get Comments
?   ??? By Issue ID - Success
?   ??? By Issue ID - Empty
?   ??? By Comment ID - Success
?   ??? By Comment ID - Not Found
??? Update Comment
?   ??? Success
?   ??? Not Found
?   ??? Update Mentions
?   ??? Clear Mentions
??? Delete Comment
    ??? Success
    ??? Not Found
    ??? Verify Cascade Delete
```

---

## Database Verification Queries

### Check Comments
```sql
SELECT * FROM issue_comments WHERE issue_id = 'your-issue-id' ORDER BY created_at DESC;
```

### Check Mentions
```sql
SELECT m.*, u.name as mentioned_user_name 
FROM mentions m
LEFT JOIN users u ON m.mention_user_id = u.id
WHERE m.issue_comments_id = 'your-comment-id';
```

### Check Comment Count by Issue
```sql
SELECT issue_id, COUNT(*) as comment_count 
FROM issue_comments 
GROUP BY issue_id 
ORDER BY comment_count DESC;
```

### Check Orphaned Mentions (should be 0)
```sql
SELECT * FROM mentions 
WHERE issue_comments_id NOT IN (SELECT id FROM issue_comments);
```

---

## Performance Testing

### Load Test Scenarios
1. Create 100 comments on the same issue
2. Retrieve all comments for an issue with 100+ comments
3. Update 50 comments concurrently
4. Delete 50 comments concurrently

### Expected Performance
- Comment creation: < 200ms
- Get comments list: < 500ms (for 100 comments)
- Get single comment: < 100ms
- Update comment: < 200ms
- Delete comment: < 200ms

---

## Error Handling Verification

### Test Error Scenarios
- [ ] Invalid Guid format in URL
- [ ] Missing required fields in request body
- [ ] Null or empty comment body
- [ ] Non-existent issue ID
- [ ] Non-existent user ID
- [ ] Non-existent comment ID
- [ ] Missing authentication token
- [ ] Invalid/expired authentication token
- [ ] Malformed JSON in request body
- [ ] Database connection errors (simulate)

---

## Checklist

### Create Comment
- [ ] Create comment successfully
- [ ] Create comment with mentions
- [ ] Create comment without mentions
- [ ] Fail on invalid issue ID
- [ ] Fail on invalid author ID
- [ ] Fail on invalid mentioned user IDs (skip invalid IDs)
- [ ] Fail without authentication
- [ ] Handle empty body
- [ ] Handle very long body

### Get Comments
- [ ] Get all comments for issue
- [ ] Get comments for issue with no comments
- [ ] Get single comment by ID
- [ ] Fail on non-existent comment ID
- [ ] Verify author information included
- [ ] Verify mentions included
- [ ] Verify sorted by created_at DESC

### Update Comment
- [ ] Update comment body
- [ ] Update mentions
- [ ] Clear mentions (empty array)
- [ ] Fail on non-existent comment ID
- [ ] Verify updated_at timestamp changes
- [ ] Verify updated_by is recorded

### Delete Comment
- [ ] Delete comment successfully
- [ ] Fail on non-existent comment ID
- [ ] Verify mentions are deleted (cascade)
- [ ] Verify comment cannot be retrieved after deletion

### Security
- [ ] All endpoints require authentication
- [ ] Invalid token returns 401
- [ ] Missing token returns 401

### Data Integrity
- [ ] Comments linked to correct issue
- [ ] Mentions linked to correct comment
- [ ] Audit fields populated correctly
- [ ] Timestamps are accurate
- [ ] No orphaned mentions after delete

---

## Troubleshooting

### Common Issues

**Issue:** 401 Unauthorized
- **Solution:** Check authentication token is valid and included in Authorization header

**Issue:** 404 Not Found on valid ID
- **Solution:** Verify ID exists in database and is correct Guid format

**Issue:** 500 Internal Server Error
- **Solution:** Check server logs for detailed error message, verify database connection

**Issue:** Comments not appearing
- **Solution:** Check issue_comments table directly, verify issue_id matches

**Issue:** Mentions not created
- **Solution:** Verify user IDs exist in users table, check mentions table

---

## Support

For issues or questions:
1. Check server logs for detailed error messages
2. Verify database schema matches expected structure
3. Ensure all migrations are applied
4. Review API documentation
5. Contact development team with error details and request/response logs
