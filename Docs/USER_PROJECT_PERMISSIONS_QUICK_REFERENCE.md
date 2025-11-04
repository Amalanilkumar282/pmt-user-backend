# User Project Permissions - Quick Reference

## ?? Quick Start (3 Steps)

### 1. Setup Database (1 minute)
```sql
-- Run this in your Supabase SQL Editor
-- Copy from: Docs/Database_Setup_Permissions.sql
INSERT INTO permissions (name, description) VALUES
('project.create', 'Can create new projects'),
('project.read', 'Can view/read project details'),
('project.update', 'Can update project details'),
('project.delete', 'Can delete projects'),
('team.manage', 'Can manage teams'),
('user.manage', 'Can manage users');
```

### 2. Test API (2 minutes)
```bash
# Login
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "user@example.com", "password": "password"}'

# Get Permissions (replace <token> and <projectId>)
curl -X GET "http://localhost:5000/api/permission/me/project/<projectId>" \
  -H "Authorization: Bearer <token>"
```

### 3. Use in Frontend (1 minute)
```typescript
// Angular/TypeScript
const permissions = await this.http.get(
  `/api/permission/me/project/${projectId}`
).toPromise();

// Show/hide UI based on permissions
this.canEdit = permissions.data.permissionFlags.canUpdateProject;
```

---

## ?? API Endpoints

### Get User Permissions
```
GET /api/permission/user/{userId}/project/{projectId}
```

### Get My Permissions (JWT-based)
```
GET /api/permission/me/project/{projectId}
```

---

## ?? Standard Permissions

| Permission | Description | Typical Roles |
|-----------|-------------|---------------|
| `project.create` | Create projects | Owner, Admin |
| `project.read` | View projects | All roles |
| `project.update` | Update projects | Owner, Admin, Developer |
| `project.delete` | Delete projects | Owner, Admin |
| `team.manage` | Manage teams | Owner, Admin, Developer, Team Lead |
| `user.manage` | Manage users | Owner, Admin |

---

## ?? Standard Roles

| Role | Permissions Count | Description |
|------|------------------|-------------|
| **Project Owner** | All (6+) | Full access, project creator |
| **Admin** | All (6+) | Full administrative access |
| **Developer** | 3-4 | Read, update, team management |
| **Team Lead** | 2-3 | Read, team management |
| **Viewer** | 1-2 | Read-only access |

---

## ?? Response Structure

```json
{
  "status": 200,
  "data": {
    "userId": 5,
    "userName": "John Doe",
    "userEmail": "john.doe@example.com",
    "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "projectName": "PMT Backend",
    "roleId": 2,
    "roleName": "Developer",
    "isOwner": false,
    "addedAt": "2025-01-15T10:30:00Z",
    "permissions": [
      {
        "id": 2,
        "name": "project.read",
        "description": "Can view project details"
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
  "message": "Successfully retrieved permissions..."
}
```

---

## ?? Permission Flags (Quick Checks)

```typescript
// In your TypeScript/Angular code
if (permissions.permissionFlags.canUpdateProject) {
  // Show edit button
}

if (permissions.permissionFlags.canManageTeams) {
  // Show team management
}

if (permissions.isOwner) {
  // Show owner-only features
}
```

---

## ??? Common SQL Queries

### Check User's Role in Project
```sql
SELECT 
    pm.user_id,
    u.name,
    pm.project_id,
    p.name as project_name,
    r.name as role_name,
    pm.is_owner
FROM project_members pm
JOIN users u ON pm.user_id = u.id
JOIN projects p ON pm.project_id = p.id
JOIN roles r ON pm.role_id = r.id
WHERE pm.user_id = <USER_ID> 
  AND pm.project_id = '<PROJECT_ID>';
```

### View User's Permissions
```sql
SELECT 
    u.name as user_name,
    p.name as project_name,
    r.name as role_name,
    perm.name as permission_name
FROM project_members pm
JOIN users u ON pm.user_id = u.id
JOIN projects p ON pm.project_id = p.id
JOIN roles r ON pm.role_id = r.id
JOIN role_permissions rp ON r.id = rp.role_id
JOIN permissions perm ON rp.permission_id = perm.id
WHERE pm.user_id = <USER_ID> 
  AND pm.project_id = '<PROJECT_ID>';
```

### Add User to Project
```sql
INSERT INTO project_members 
(project_id, user_id, role_id, is_owner, added_at, added_by)
SELECT 
    '<PROJECT_ID>'::uuid,
    <USER_ID>,
    r.id,
    false,
    NOW(),
    <ADMIN_USER_ID>
FROM roles r
WHERE r.name = 'Developer';
```

### Change User's Role
```sql
UPDATE project_members 
SET role_id = (SELECT id FROM roles WHERE name = 'Team Lead')
WHERE user_id = <USER_ID> 
  AND project_id = '<PROJECT_ID>';
```

---

## ?? Frontend Integration Examples

### Angular Component
```typescript
export class ProjectComponent implements OnInit {
  permissions: UserProjectPermissionsDto;
  
  async ngOnInit() {
    this.permissions = await this.permissionService
      .getProjectPermissions(this.projectId);
  }
  
  get canEdit(): boolean {
    return this.permissions.permissionFlags.canUpdateProject;
  }
  
  get canManageTeam(): boolean {
    return this.permissions.permissionFlags.canManageTeams;
  }
}
```

### Template
```html
<div *ngIf="permissions">
  <button *ngIf="canEdit" (click)="editProject()">
    Edit Project
  </button>
  
  <button *ngIf="canManageTeam" (click)="manageTeam()">
    Manage Team
  </button>
  
  <button *ngIf="permissions.isOwner" (click)="deleteProject()">
    Delete Project
  </button>
</div>
```

### Route Guard
```typescript
@Injectable()
export class PermissionGuard implements CanActivate {
  async canActivate(route: ActivatedRouteSnapshot): Promise<boolean> {
    const projectId = route.params['projectId'];
    const requiredPermission = route.data['permission'];
    
    const permissions = await this.permissionService
      .getProjectPermissions(projectId);
    
    return permissions.data.permissions
      .some(p => p.name === requiredPermission);
  }
}

// Usage
{
  path: 'projects/:projectId/edit',
  component: EditProjectComponent,
  canActivate: [PermissionGuard],
  data: { permission: 'project.update' }
}
```

---

## ?? Common Issues & Solutions

### Issue: Empty Permissions Array
**Solution:**
```sql
-- Check if role has permissions
SELECT * FROM role_permissions WHERE role_id = <ROLE_ID>;

-- If empty, run Database_Setup_Permissions.sql
```

### Issue: 404 Not Found
**Solution:**
```sql
-- Check if user is in project
SELECT * FROM project_members 
WHERE user_id = <USER_ID> AND project_id = '<PROJECT_ID>';

-- If not found, add user to project
```

### Issue: 401 Unauthorized
**Solution:**
1. Login again to get fresh JWT token
2. Ensure token is in header: `Authorization: Bearer <token>`
3. Check token hasn't expired

---

## ?? File Locations

| File Type | Location |
|-----------|----------|
| **Controller** | `BACKEND_CQRS.Api/Controllers/PermissionController.cs` |
| **Query** | `BACKEND_CQRS.Application/Query/Permissions/GetUserProjectPermissionsQuery.cs` |
| **Handler** | `BACKEND_CQRS.Application/Handler/Permissions/GetUserProjectPermissionsQueryHandler.cs` |
| **Repository** | `BACKEND_CQRS.Infrastructure/Repository/PermissionRepository.cs` |
| **DTOs** | `BACKEND_CQRS.Application/Dto/UserProjectPermissionsDto.cs` |
| **Documentation** | `Docs/USER_PROJECT_PERMISSIONS_API_DOCUMENTATION.md` |
| **Testing Guide** | `Docs/USER_PROJECT_PERMISSIONS_TESTING_GUIDE.md` |
| **SQL Setup** | `Docs/Database_Setup_Permissions.sql` |
| **Postman** | `Docs/User_Project_Permissions_Postman_Collection.json` |

---

## ?? Related Endpoints

```bash
# Add member to project
POST /api/project/member

# Get project members
GET /api/project/{projectId}/users

# Get all roles
GET /api/role

# Update member role
PUT /api/project/update
```

---

## ?? Quick Help

### Swagger UI
```
http://localhost:5000/swagger
```

### View Logs
```bash
# Check application logs for errors
dotnet run --project BACKEND_CQRS.Api
```

### Verify Database
```sql
-- Count permissions
SELECT COUNT(*) FROM permissions;

-- Count roles
SELECT COUNT(*) FROM roles;

-- Count role-permission mappings
SELECT COUNT(*) FROM role_permissions;
```

---

## ? Quick Checklist

- [ ] Database setup completed
- [ ] Can login and get JWT token
- [ ] Can call permission endpoint successfully
- [ ] Response contains expected permissions
- [ ] Permission flags are correct
- [ ] Frontend can retrieve permissions
- [ ] UI updates based on permissions

---

**Need more help?** Check the full documentation in the `Docs` folder!

---

**Version:** 1.0  
**Last Updated:** 2025-01-31
