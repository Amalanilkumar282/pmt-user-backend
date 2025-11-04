# User Project Permissions - Testing Guide

## Prerequisites

Before testing, ensure you have:
- ? Application running (`dotnet run` in BACKEND_CQRS.Api)
- ? Database setup completed (run `Database_Setup_Permissions.sql`)
- ? At least one user account created
- ? At least one project created
- ? User added to the project as a member

## Quick Start Testing

### Step 1: Database Setup

1. Open your Supabase SQL Editor
2. Run the `Database_Setup_Permissions.sql` script
3. Verify data insertion:

```sql
-- Verify permissions
SELECT * FROM permissions ORDER BY name;

-- Verify roles
SELECT * FROM roles ORDER BY name;

-- Verify role-permission mappings
SELECT 
    r.name as role_name,
    COUNT(rp.permission_id) as permission_count
FROM roles r
LEFT JOIN role_permissions rp ON r.id = rp.role_id
GROUP BY r.id, r.name
ORDER BY permission_count DESC;
```

**Expected Results:**
- 6 permissions (project.create, project.read, project.update, project.delete, team.manage, user.manage)
- 5 roles (Project Owner, Admin, Developer, Team Lead, Viewer)
- Project Owner and Admin: 6 permissions each
- Developer: 3-4 permissions
- Team Lead: 2-3 permissions
- Viewer: 1-2 permissions

### Step 2: Add Test User to Project

If you don't have a project member yet, add one:

```sql
-- Add a user to a project as a Developer
INSERT INTO project_members (
    project_id, 
    user_id, 
    role_id, 
    is_owner, 
    added_at, 
    added_by
)
SELECT 
    '<YOUR_PROJECT_ID>'::uuid,  -- Replace with actual project ID
    <YOUR_USER_ID>,              -- Replace with actual user ID
    r.id,
    false,
    NOW(),
    1                            -- Replace with admin user ID
FROM roles r
WHERE r.name = 'Developer';
```

### Step 3: Test Authentication

1. **Login to get JWT token**

**Using Swagger:**
- Navigate to `http://localhost:5000/swagger`
- Find `/api/auth/login` endpoint
- Execute with your credentials
- Copy the `accessToken` from the response

**Using cURL:**
```bash
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "your-password"
  }'
```

**Expected Response:**
```json
{
  "status": 200,
  "data": {
    "userId": 5,
    "email": "user@example.com",
    "name": "John Doe",
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "accessTokenExpires": "2025-01-31T15:30:00Z",
    "refreshTokenExpires": "2025-02-07T14:30:00Z",
    "isActive": true,
    "isSuperAdmin": false
  },
  "succeeded": true,
  "message": "Login successful"
}
```

2. **Authorize in Swagger**
- Click "Authorize" button (top right)
- Enter: `Bearer <your-access-token>`
- Click "Authorize"

### Step 4: Test Permission Endpoints

#### Test 1: Get User Permissions for Project

**Endpoint:** `GET /api/permission/user/{userId}/project/{projectId}`

**Using Swagger:**
1. Find the endpoint in Swagger UI
2. Enter:
   - `userId`: Your user ID (e.g., 5)
   - `projectId`: Your project ID (e.g., 3fa85f64-5717-4562-b3fc-2c963f66afa6)
3. Click "Execute"

**Using cURL:**
```bash
curl -X GET "http://localhost:5000/api/permission/user/5/project/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Expected Response (200 OK):**
```json
{
  "status": 200,
  "data": {
    "userId": 5,
    "userName": "John Doe",
    "userEmail": "john.doe@example.com",
    "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "projectName": "PMT User Backend",
    "roleId": 2,
    "roleName": "Developer",
    "isOwner": false,
    "addedAt": "2025-01-15T10:30:00Z",
    "permissions": [
      {
        "id": 2,
        "name": "project.read",
        "description": "Can view/read project details"
      },
      {
        "id": 3,
        "name": "project.update",
        "description": "Can update project details"
      },
      {
        "id": 5,
        "name": "team.manage",
        "description": "Can manage teams (create, update, delete)"
      }
    ],
    "permissionFlags": {
      "canCreateProject": false,
      "canReadProject": true,
      "canUpdateProject": true,
      "canDeleteProject": false,
      "canManageTeams": true,
      "canManageUsers": false
    }
  },
  "succeeded": true,
  "message": "Successfully retrieved permissions for user 'John Doe' in project 'PMT User Backend'."
}
```

#### Test 2: Get Current User's Permissions

**Endpoint:** `GET /api/permission/me/project/{projectId}`

**Using Swagger:**
1. Find the endpoint in Swagger UI
2. Enter:
   - `projectId`: Your project ID
3. Click "Execute"

**Using cURL:**
```bash
curl -X GET "http://localhost:5000/api/permission/me/project/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Expected Response:** Same as Test 1

## Test Scenarios

### Scenario 1: Valid User in Project
**Setup:**
- User exists
- Project exists
- User is a project member with a role

**Expected Result:** ? 200 OK with permissions

### Scenario 2: Invalid User ID
**Setup:**
- Use userId = 0 or negative number

**Test:**
```bash
curl -X GET "http://localhost:5000/api/permission/user/0/project/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer <token>"
```

**Expected Response:** ? 400 Bad Request
```json
{
  "status": 400,
  "data": null,
  "succeeded": false,
  "message": "Invalid user ID. User ID must be greater than 0."
}
```

### Scenario 3: Invalid Project ID
**Setup:**
- Use empty GUID (00000000-0000-0000-0000-000000000000)

**Expected Response:** ? 400 Bad Request
```json
{
  "status": 400,
  "data": null,
  "succeeded": false,
  "message": "Invalid project ID. Project ID cannot be empty."
}
```

### Scenario 4: User Not Found
**Setup:**
- Use a userId that doesn't exist in database

**Expected Response:** ? 404 Not Found
```json
{
  "status": 404,
  "data": null,
  "succeeded": false,
  "message": "User with ID 999999 not found."
}
```

### Scenario 5: Project Not Found
**Setup:**
- Use a projectId that doesn't exist in database

**Expected Response:** ? 404 Not Found
```json
{
  "status": 404,
  "data": null,
  "succeeded": false,
  "message": "Project with ID <guid> not found."
}
```

### Scenario 6: User Not a Project Member
**Setup:**
- Valid user and project, but user is not added to project

**Expected Response:** ? 404 Not Found
```json
{
  "status": 404,
  "data": null,
  "succeeded": false,
  "message": "User John Doe (ID: 5) is not a member of project 'PMT User Backend'."
}
```

### Scenario 7: No Authentication Token
**Setup:**
- Don't send Authorization header

**Expected Response:** ? 401 Unauthorized

### Scenario 8: Expired or Invalid Token
**Setup:**
- Use expired or malformed JWT token

**Expected Response:** ? 401 Unauthorized

## Testing Different Roles

### Test as Project Owner
**Setup:**
```sql
UPDATE project_members 
SET role_id = (SELECT id FROM roles WHERE name = 'Project Owner'),
    is_owner = true
WHERE user_id = <YOUR_USER_ID> AND project_id = '<YOUR_PROJECT_ID>';
```

**Expected Permissions:**
- All 6 permissions (project.create, project.read, project.update, project.delete, team.manage, user.manage)
- `isOwner: true`
- All permission flags set to true

### Test as Admin
**Setup:**
```sql
UPDATE project_members 
SET role_id = (SELECT id FROM roles WHERE name = 'Admin'),
    is_owner = false
WHERE user_id = <YOUR_USER_ID> AND project_id = '<YOUR_PROJECT_ID>';
```

**Expected Permissions:**
- All 6 permissions
- `isOwner: false`
- All permission flags set to true

### Test as Developer
**Setup:**
```sql
UPDATE project_members 
SET role_id = (SELECT id FROM roles WHERE name = 'Developer')
WHERE user_id = <YOUR_USER_ID> AND project_id = '<YOUR_PROJECT_ID>';
```

**Expected Permissions:**
- project.read, project.update, team.manage (3 permissions)
- `canReadProject: true`
- `canUpdateProject: true`
- `canManageTeams: true`
- Other flags: false

### Test as Team Lead
**Setup:**
```sql
UPDATE project_members 
SET role_id = (SELECT id FROM roles WHERE name = 'Team Lead')
WHERE user_id = <YOUR_USER_ID> AND project_id = '<YOUR_PROJECT_ID>';
```

**Expected Permissions:**
- project.read, team.manage (2-3 permissions depending on setup)
- Limited permission flags

### Test as Viewer
**Setup:**
```sql
UPDATE project_members 
SET role_id = (SELECT id FROM roles WHERE name = 'Viewer')
WHERE user_id = <YOUR_USER_ID> AND project_id = '<YOUR_PROJECT_ID>';
```

**Expected Permissions:**
- project.read only (1 permission)
- `canReadProject: true`
- All other flags: false

## Postman Testing

### Import Collection
1. Open Postman
2. Click "Import"
3. Select `User_Project_Permissions_Postman_Collection.json`
4. Collection will be imported with all requests

### Configure Variables
1. In the collection, go to "Variables" tab
2. Set values:
   - `baseUrl`: `http://localhost:5000` (or your API URL)
   - `userId`: Your test user ID
   - `projectId`: Your test project ID

### Run Tests
1. **Login First:**
   - Execute "Authentication > Login" request
   - Token will be automatically saved to collection variables

2. **Test Permissions:**
   - Execute "Permissions > Get User Permissions for Project"
   - Execute "Permissions > Get My Permissions for Project"

3. **Verify Results:**
   - Check response status is 200
   - Verify permissions list matches user's role
   - Verify permission flags are correct

## Frontend Integration Testing

### Angular Example Test
```typescript
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PermissionService } from './permission.service';

describe('PermissionService', () => {
  let service: PermissionService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [PermissionService]
    });
    service = TestBed.inject(PermissionService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('should retrieve user permissions for project', () => {
    const projectId = '3fa85f64-5717-4562-b3fc-2c963f66afa6';
    const mockResponse = {
      status: 200,
      data: {
        userId: 5,
        userName: 'John Doe',
        permissions: [
          { id: 2, name: 'project.read', description: 'Can read' },
          { id: 3, name: 'project.update', description: 'Can update' }
        ],
        permissionFlags: {
          canReadProject: true,
          canUpdateProject: true,
          canDeleteProject: false
        }
      },
      succeeded: true
    };

    service.getProjectPermissions(projectId).subscribe(response => {
      expect(response.data.permissions.length).toBe(2);
      expect(response.data.permissionFlags.canReadProject).toBe(true);
    });

    const req = httpMock.expectOne(`/api/permission/me/project/${projectId}`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });
});
```

## Performance Testing

### Test Response Time
```bash
# Using Apache Bench
ab -n 100 -c 10 -H "Authorization: Bearer <token>" \
  "http://localhost:5000/api/permission/me/project/3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

**Expected:**
- Average response time: < 100ms
- 100% success rate
- No errors

## Troubleshooting

### Issue: 401 Unauthorized
**Cause:** Missing or invalid JWT token

**Solution:**
1. Login again to get fresh token
2. Ensure token is properly formatted: `Bearer <token>`
3. Check token hasn't expired

### Issue: Empty permissions array
**Cause:** Role has no permissions assigned

**Solution:**
```sql
-- Check role-permission mappings
SELECT * FROM role_permissions 
WHERE role_id = (SELECT role_id FROM project_members WHERE user_id = <YOUR_USER_ID>);

-- If empty, run the Database_Setup_Permissions.sql script again
```

### Issue: 404 - User not a project member
**Cause:** User not in project_members table

**Solution:**
```sql
-- Add user to project
INSERT INTO project_members (project_id, user_id, role_id, is_owner, added_at, added_by)
VALUES ('<PROJECT_ID>'::uuid, <USER_ID>, 2, false, NOW(), 1);
```

### Issue: Database connection errors
**Cause:** Connection string or database issues

**Solution:**
1. Check `appsettings.json` connection string
2. Verify database is accessible
3. Check database logs in Supabase

## Success Criteria

? All test scenarios return expected status codes  
? Permission lists match user's role  
? Permission flags correctly reflect permissions  
? Response time < 100ms  
? No errors in application logs  
? Frontend can successfully retrieve and use permissions  

## Next Steps

After successful testing:
1. ? Mark all tests as passing
2. ?? Document any custom permissions added
3. ?? Deploy to staging environment
4. ?? Share API documentation with frontend team
5. ?? Review security with team
6. ?? Set up monitoring for permission endpoints

---

**Version:** 1.0  
**Last Updated:** 2025-01-31  
**Author:** GitHub Copilot
