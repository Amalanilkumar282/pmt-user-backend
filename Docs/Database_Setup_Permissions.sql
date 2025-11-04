-- ============================================
-- User Project Permissions - Database Setup
-- ============================================
-- This script sets up sample permissions, roles, and role-permission mappings
-- for the User Project Permissions feature.
--
-- Usage:
-- 1. Connect to your PostgreSQL database (Supabase)
-- 2. Run this script in your SQL Editor
-- 3. Verify data was inserted successfully
--
-- Note: This script uses INSERT with ON CONFLICT to avoid duplicate errors
-- ============================================

-- ============================================
-- 1. INSERT PERMISSIONS
-- ============================================
-- Standard permissions for project management

INSERT INTO permissions (name, description) VALUES
('project.create', 'Can create new projects'),
('project.read', 'Can view/read project details'),
('project.update', 'Can update project details'),
('project.delete', 'Can delete projects'),
('team.manage', 'Can manage teams (create, update, delete)'),
('user.manage', 'Can manage users (add/remove project members)')
ON CONFLICT (name) DO NOTHING;

-- Optional: Add more specific permissions
INSERT INTO permissions (name, description) VALUES
('issue.create', 'Can create issues'),
('issue.update', 'Can update issues'),
('issue.delete', 'Can delete issues'),
('sprint.manage', 'Can manage sprints'),
('board.manage', 'Can manage boards'),
('report.view', 'Can view reports')
ON CONFLICT (name) DO NOTHING;

-- ============================================
-- 2. INSERT ROLES
-- ============================================
-- Standard roles with descriptions

INSERT INTO roles (name, description) VALUES
('Project Owner', 'Full access to project - owner of the project'),
('Admin', 'Full access to all project features'),
('Developer', 'Can read and update project, manage teams and issues'),
('Team Lead', 'Can manage teams, issues, and sprints'),
('Viewer', 'Can only view project details and issues')
ON CONFLICT (name) DO NOTHING;

-- ============================================
-- 3. MAP ROLES TO PERMISSIONS
-- ============================================
-- This creates the role-permission relationships

-- Project Owner: All permissions
INSERT INTO role_permissions (role_id, permission_id)
SELECT 
    r.id as role_id,
    p.id as permission_id
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'Project Owner'
ON CONFLICT DO NOTHING;

-- Admin: All permissions (same as Project Owner for flexibility)
INSERT INTO role_permissions (role_id, permission_id)
SELECT 
    r.id as role_id,
    p.id as permission_id
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'Admin'
ON CONFLICT DO NOTHING;

-- Developer: project.read, project.update, team.manage, issue.*, sprint.manage
INSERT INTO role_permissions (role_id, permission_id)
SELECT 
    r.id as role_id,
    p.id as permission_id
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'Developer'
AND p.name IN (
    'project.read', 
    'project.update', 
    'team.manage',
    'issue.create',
    'issue.update',
    'issue.delete',
    'sprint.manage'
)
ON CONFLICT DO NOTHING;

-- Team Lead: project.read, team.manage, issue.*, sprint.manage
INSERT INTO role_permissions (role_id, permission_id)
SELECT 
    r.id as role_id,
    p.id as permission_id
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'Team Lead'
AND p.name IN (
    'project.read',
    'team.manage',
    'issue.create',
    'issue.update',
    'issue.delete',
    'sprint.manage',
    'board.manage'
)
ON CONFLICT DO NOTHING;

-- Viewer: Only project.read and report.view
INSERT INTO role_permissions (role_id, permission_id)
SELECT 
    r.id as role_id,
    p.id as permission_id
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'Viewer'
AND p.name IN ('project.read', 'report.view')
ON CONFLICT DO NOTHING;

-- ============================================
-- 4. VERIFICATION QUERIES
-- ============================================
-- Run these to verify the setup

-- Check all permissions
SELECT * FROM permissions ORDER BY name;

-- Check all roles
SELECT * FROM roles ORDER BY name;

-- Check role-permission mappings
SELECT 
    r.name as role_name,
    p.name as permission_name,
    p.description as permission_description
FROM role_permissions rp
JOIN roles r ON rp.role_id = r.id
JOIN permissions p ON rp.permission_id = p.id
ORDER BY r.name, p.name;

-- Count permissions per role
SELECT 
    r.name as role_name,
    COUNT(rp.permission_id) as permission_count
FROM roles r
LEFT JOIN role_permissions rp ON r.id = rp.role_id
GROUP BY r.id, r.name
ORDER BY permission_count DESC;

-- ============================================
-- 5. OPTIONAL: Sample Project Member Setup
-- ============================================
-- Uncomment and modify these to add sample project members
-- Replace the GUIDs and user IDs with your actual values

/*
-- Example: Add a user to a project as Admin
INSERT INTO project_members (
    project_id, 
    user_id, 
    role_id, 
    is_owner, 
    added_at, 
    added_by
)
SELECT 
    '3fa85f64-5717-4562-b3fc-2c963f66afa6'::uuid,  -- Replace with your project ID
    5,                                               -- Replace with your user ID
    r.id,
    false,
    NOW(),
    1                                                -- Replace with admin user ID
FROM roles r
WHERE r.name = 'Admin'
ON CONFLICT DO NOTHING;
*/

-- ============================================
-- 6. CLEANUP (USE WITH CAUTION)
-- ============================================
-- Uncomment only if you need to remove test data

/*
-- Delete all role-permission mappings
DELETE FROM role_permissions;

-- Delete all roles (be careful - this will affect existing project members)
DELETE FROM roles WHERE name IN ('Project Owner', 'Admin', 'Developer', 'Team Lead', 'Viewer');

-- Delete all permissions (be careful - this will break role-permission mappings)
DELETE FROM permissions;
*/

-- ============================================
-- END OF SCRIPT
-- ============================================
-- If all queries executed successfully, your permissions system is ready!
-- You can now test the API endpoints:
-- - GET /api/permission/user/{userId}/project/{projectId}
-- - GET /api/permission/me/project/{projectId}
-- ============================================
