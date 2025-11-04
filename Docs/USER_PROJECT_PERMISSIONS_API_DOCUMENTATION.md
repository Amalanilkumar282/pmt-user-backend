# User Project Permissions API Documentation

## Overview
This API provides endpoints to retrieve user permissions for specific projects. The system uses role-based access control (RBAC) where users are assigned roles in projects, and roles have associated permissions.

## Architecture

### Database Schema
```
users ???
        ?
        ???? project_members ???
        ?                      ?
projects ??????????????????????
                               ?
                               ???? roles ??? role_permissions ??? permissions
```

### Flow
1. User is added to a project as a `project_member` with a specific `role`
2. Role has associated `permissions` through `role_permissions` table
3. API queries this relationship to fetch user's permissions for a project

## API Endpoints

### 1. Get User Permissions for a Project

**Endpoint:** `GET /api/permission/user/{userId}/project/{projectId}`

**Description:** Get a specific user's permissions for a specific project.

**Authorization:** Requires JWT Bearer token

**Parameters:**
- `userId` (path, required): Integer - The user ID
- `projectId` (path, required): GUID - The project ID

**Response (200 OK):**
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
        "id": 1,
        "name": "project.read",
        "description": "Can view project details"
      },
      {
        "id": 2,
        "name": "project.update",
        "description": "Can update project"
      },
      {
        "id": 5,
        "name": "team.manage",
        "description": "Can manage teams"
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
  "message": "Successfully retrieved permissions for user 'John Doe' in project 'PMT User Backend'.",
  "errors": null
}
```

**Error Responses:**

**400 Bad Request:**
```json
{
  "status": 400,
  "data": null,
  "succeeded": false,
  "message": "Invalid user ID. User ID must be greater than 0.",
  "errors": null
}
```

**404 Not Found:**
```json
{
  "status": 404,
  "data": null,
  "succeeded": false,
  "message": "User John Doe (ID: 5) is not a member of project 'PMT User Backend'.",
  "errors": null
}
```

### 2. Get Current User's Permissions for a Project

**Endpoint:** `GET /api/permission/me/project/{projectId}`

**Description:** Get the current logged-in user's permissions for a specific project. Uses JWT token to identify the user.

**Authorization:** Requires JWT Bearer token

**Parameters:**
- `projectId` (path, required): GUID - The project ID

**Response:** Same as endpoint #1

**Convenience:** This endpoint is more convenient when you don't need to specify the user ID explicitly.

## Permission Names

The system recognizes the following standard permissions:

| Permission Name | Description |
|----------------|-------------|
| `project.create` | Can create new projects |
| `project.read` | Can view/read projects |
| `project.update` | Can update project details |
| `project.delete` | Can delete projects |
| `team.manage` | Can manage teams (create, update, delete teams) |
| `user.manage` | Can manage users (add/remove project members) |

**Note:** You can add more permissions in the `permissions` table as needed.

## Use Cases

### 1. User Clicks on a Project
When a user clicks on a project in the frontend:

```typescript
// TypeScript/Angular example
async onProjectClick(projectId: string) {
  const response = await this.http.get<ApiResponse<UserProjectPermissionsDto>>(
    `/api/permission/me/project/${projectId}`
  ).toPromise();
  
  if (response.succeeded) {
    const permissions = response.data;
    
    // Store permissions in state/service
    this.permissionService.setCurrentProjectPermissions(permissions);
    
    // Use permission flags to show/hide UI elements
    this.canCreateIssues = permissions.permissionFlags.canUpdateProject;
    this.canManageTeam = permissions.permissionFlags.canManageTeams;
    this.canManageMembers = permissions.permissionFlags.canManageUsers;
    
    // Navigate to project dashboard
    this.router.navigate(['/projects', projectId]);
  }
}
```

### 2. Conditional UI Rendering

```typescript
// In component template
<button *ngIf="permissions.permissionFlags.canManageUsers" 
        (click)="addMember()">
  Add Member
</button>

<button *ngIf="permissions.permissionFlags.canUpdateProject" 
        (click)="editProject()">
  Edit Project
</button>

<button *ngIf="permissions.isOwner" 
        (click)="deleteProject()" 
        class="danger">
  Delete Project
</button>
```

### 3. Route Guards

```typescript
// Angular route guard example
@Injectable()
export class ProjectPermissionGuard implements CanActivate {
  constructor(
    private permissionService: PermissionService,
    private router: Router
  ) {}
  
  async canActivate(route: ActivatedRouteSnapshot): Promise<boolean> {
    const projectId = route.params['projectId'];
    const requiredPermission = route.data['permission'];
    
    const permissions = await this.permissionService
      .getProjectPermissions(projectId);
    
    const hasPermission = permissions.permissions
      .some(p => p.name === requiredPermission);
    
    if (!hasPermission) {
      this.router.navigate(['/access-denied']);
      return false;
    }
    
    return true;
  }
}

// Usage in routes
{
  path: 'projects/:projectId/settings',
  component: ProjectSettingsComponent,
  canActivate: [ProjectPermissionGuard],
  data: { permission: 'project.update' }
}
```

### 4. Service Layer Permission Check

```typescript
// Permission service
@Injectable()
export class PermissionService {
  private currentPermissions: UserProjectPermissionsDto | null = null;
  
  async loadProjectPermissions(projectId: string) {
    const response = await this.http.get<ApiResponse<UserProjectPermissionsDto>>(
      `/api/permission/me/project/${projectId}`
    ).toPromise();
    
    this.currentPermissions = response.data;
    return this.currentPermissions;
  }
  
  hasPermission(permissionName: string): boolean {
    return this.currentPermissions?.permissions
      .some(p => p.name === permissionName) ?? false;
  }
  
  isProjectOwner(): boolean {
    return this.currentPermissions?.isOwner ?? false;
  }
  
  get permissions(): UserProjectPermissionsDto | null {
    return this.currentPermissions;
  }
}
```

## Database Setup

### Required Tables

The following tables must exist in your database:

1. **users** - User accounts
2. **projects** - Projects
3. **project_members** - Links users to projects with roles
4. **roles** - Role definitions
5. **permissions** - Permission definitions
6. **role_permissions** - Links roles to permissions

### Sample Data

#### Permissions Table
```sql
INSERT INTO permissions (name, description) VALUES
('project.create', 'Can create new projects'),
('project.read', 'Can view/read projects'),
('project.update', 'Can update project details'),
('project.delete', 'Can delete projects'),
('team.manage', 'Can manage teams'),
('user.manage', 'Can manage users');
```

#### Roles Table
```sql
INSERT INTO roles (name, description) VALUES
('Admin', 'Full access to project'),
('Developer', 'Can read and update project, manage teams'),
('Viewer', 'Can only view project');
```

#### Role-Permission Mappings
```sql
-- Admin role gets all permissions
INSERT INTO role_permissions (role_id, permission_id)
SELECT 1, id FROM permissions;

-- Developer role gets read, update, and team.manage
INSERT INTO role_permissions (role_id, permission_id)
SELECT 2, id FROM permissions 
WHERE name IN ('project.read', 'project.update', 'team.manage');

-- Viewer role gets only read permission
INSERT INTO role_permissions (role_id, permission_id)
SELECT 3, id FROM permissions 
WHERE name = 'project.read';
```

## Testing

### 1. Using Swagger UI

1. Start your application
2. Navigate to `/swagger`
3. Authenticate using the `/api/auth/login` endpoint
4. Click "Authorize" and enter: `Bearer your-access-token`
5. Test the permission endpoints:
   - `/api/permission/user/{userId}/project/{projectId}`
   - `/api/permission/me/project/{projectId}`

### 2. Using cURL

```bash
# Login first
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "user@example.com", "password": "password"}'

# Get permissions (using token from login response)
curl -X GET "http://localhost:5000/api/permission/me/project/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### 3. Using Postman

1. Create a new request collection
2. Add Authorization header with Bearer token
3. Test GET requests to both endpoints
4. Verify response structure matches documentation

## Error Handling

The API returns consistent error responses:

| Status Code | Scenario |
|------------|----------|
| 200 | Success - permissions retrieved |
| 400 | Invalid user ID or project ID |
| 401 | Not authenticated (missing/invalid JWT token) |
| 404 | User not found, project not found, or user not a project member |
| 500 | Server error |

## Best Practices

### Frontend Implementation

1. **Cache Permissions**: Store permissions when user selects a project
2. **Refresh on Project Switch**: Reload permissions when user switches projects
3. **Use Permission Flags**: Use the `permissionFlags` for quick UI checks
4. **Graceful Degradation**: Hide features the user can't access rather than showing errors
5. **Clear on Logout**: Clear cached permissions when user logs out

### Backend Implementation

1. **Always Check Permissions**: Verify permissions server-side even if frontend hides UI
2. **Use Middleware**: Consider creating middleware to check permissions before controller actions
3. **Audit Logging**: Log permission checks and access attempts
4. **Cache Strategy**: Consider caching permissions for better performance

## Security Considerations

1. **Server-Side Validation**: Always validate permissions on the backend
2. **JWT Validation**: Ensure JWT tokens are valid and not expired
3. **Project Membership**: Verify user is actually a project member before returning permissions
4. **Least Privilege**: Grant minimum permissions necessary for each role
5. **Regular Audits**: Periodically review role-permission mappings

## Troubleshooting

### Issue: Empty Permissions Array

**Cause:** User's role has no permissions assigned

**Solution:** 
1. Check `role_permissions` table for the user's role
2. Verify permissions are properly assigned to the role

### Issue: 404 - User not a project member

**Cause:** User doesn't have a record in `project_members` for the project

**Solution:**
1. Add user to project using the add project member endpoint
2. Verify `project_members` table has a record for the user and project

### Issue: Permissions not updating after role change

**Cause:** Cached permissions or need to refetch

**Solution:**
1. Frontend should refetch permissions after role changes
2. Consider implementing a permission version number for cache invalidation

## Related Endpoints

- `POST /api/project/member` - Add member to project
- `GET /api/project/{projectId}/users` - Get all users in a project
- `GET /api/role` - Get all roles
- `PUT /api/project/update` - Update project member role

## Support

For issues or questions:
1. Check application logs for detailed error messages
2. Verify database schema matches documentation
3. Ensure JWT token is valid and contains correct user ID
4. Review Swagger documentation at `/swagger`

---

**Version:** 1.0  
**Last Updated:** 2025-01-31  
**Author:** GitHub Copilot
