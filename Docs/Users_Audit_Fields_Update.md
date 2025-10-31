# Users Entity Audit Fields - Database Update

## ?? Changes Made

### Database Schema Update
You've successfully added the following columns to the `users` table in PostgreSQL:
- ? `created_by` (int, nullable, FK to users.id)
- ? `deleted_by` (int, nullable, FK to users.id)
- ? `updated_by` (int, nullable, FK to users.id)
- ? `is_deleted` (boolean, default false)

### Code Changes Applied

#### 1. Users Entity (`BACKEND_CQRS.Domain/Entities/Users.cs`)
**Removed** `[NotMapped]` attributes from audit fields since they now exist in the database:

```csharp
// Audit fields - now exist in the database
[Column("created_by")]
public int? CreatedBy { get; set; }

[Column("deleted_by")]
public int? DeletedBy { get; set; }

[Column("updated_by")]
public int? UpdatedBy { get; set; }

[Column("is_deleted")]
public bool IsDeleted { get; set; } = false;

// Self-referencing navigation properties
[ForeignKey("CreatedBy")]
public Users? CreatedByUser { get; set; }

[ForeignKey("DeletedBy")]
public Users? DeletedByUser { get; set; }

[ForeignKey("UpdatedBy")]
public Users? UpdatedByUser { get; set; }
```

#### 2. AppDbContext (`BACKEND_CQRS.Infrastructure/Context/AppDbContext.cs`)
**Added back** the Users self-referencing configuration:

```csharp
// Configure Users self-referencing relationships
modelBuilder.Entity<Users>(entity =>
{
    entity.HasOne(u => u.CreatedByUser)
          .WithMany()
          .HasForeignKey(u => u.CreatedBy)
          .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(u => u.DeletedByUser)
          .WithMany()
          .HasForeignKey(u => u.DeletedBy)
          .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(u => u.UpdatedByUser)
          .WithMany()
          .HasForeignKey(u => u.UpdatedBy)
          .OnDelete(DeleteBehavior.Restrict);
});
```

## ? Benefits

### 1. Full Audit Trail
You can now track:
- **Who created** each user (`created_by`)
- **Who updated** each user (`updated_by`)
- **Who deleted** each user (`deleted_by`)
- **When** these actions occurred (`created_at`, `updated_at`, `deleted_at`)

### 2. Soft Delete Support
The `is_deleted` boolean field enables soft deletes:
- Users are marked as deleted instead of being physically removed
- Historical data and relationships remain intact
- Can be restored if needed

### 3. Self-Referencing Relationships
Users can reference other users for audit purposes:
```csharp
var user = await context.Users
    .Include(u => u.CreatedByUser)
    .Include(u => u.UpdatedByUser)
    .FirstOrDefaultAsync(u => u.Id == userId);

Console.WriteLine($"User created by: {user.CreatedByUser?.Name}");
Console.WriteLine($"User updated by: {user.UpdatedByUser?.Name}");
```

## ?? Usage Examples

### Creating a User
```csharp
var newUser = new Users
{
    Email = "john.doe@example.com",
    Name = "John Doe",
    CreatedBy = currentUserId, // ID of the admin creating this user
    CreatedAt = DateTime.UtcNow,
    IsActive = true,
    IsDeleted = false
};

await context.Users.AddAsync(newUser);
await context.SaveChangesAsync();
```

### Updating a User
```csharp
var user = await context.Users.FindAsync(userId);
if (user != null)
{
    user.Name = "Updated Name";
    user.UpdatedBy = currentUserId; // ID of the admin updating this user
    user.UpdatedAt = DateTime.UtcNow;
    
    await context.SaveChangesAsync();
}
```

### Soft Deleting a User
```csharp
var user = await context.Users.FindAsync(userId);
if (user != null)
{
    user.IsDeleted = true;
    user.DeletedBy = currentUserId; // ID of the admin deleting this user
    user.DeletedAt = DateTime.UtcNow;
    user.IsActive = false;
    
    await context.SaveChangesAsync();
}
```

### Filtering Out Deleted Users
```csharp
var activeUsers = await context.Users
    .Where(u => !u.IsDeleted && u.IsActive == true)
    .ToListAsync();
```

## ?? Testing Status
- ? **Build**: Successful
- ? **Entity Model**: Aligned with database schema
- ? **EF Configuration**: All relationships properly configured
- ?? **Runtime**: Ready for testing

## ?? Next Steps

### 1. Test the Login Flow
The login should now work without any database column errors:
```bash
POST /api/auth/login
{
  "email": "emma.raj@example.com",
  "password": "your_password"
}
```

### 2. Implement Audit Logic
Consider creating a base service or interceptor to automatically set audit fields:

```csharp
public interface IAuditService
{
    int GetCurrentUserId();
}

// In your service/handler
user.CreatedBy = _auditService.GetCurrentUserId();
user.CreatedAt = DateTime.UtcNow;
```

### 3. Add Soft Delete Queries
Update your queries to exclude soft-deleted users by default:

```csharp
public async Task<Users?> GetUserByEmailAsync(string email)
{
    return await _context.Users
        .Where(u => !u.IsDeleted && u.Email.ToLower() == email.ToLower())
        .FirstOrDefaultAsync();
}
```

### 4. Consider Global Query Filters
Automatically exclude soft-deleted records:

```csharp
// In AppDbContext.OnModelCreating
modelBuilder.Entity<Users>()
    .HasQueryFilter(u => !u.IsDeleted);
```

## ?? Important Notes

### Foreign Key Constraints
The database foreign keys for `created_by`, `updated_by`, and `deleted_by` reference the same table (`users`). This means:
- ?? You cannot delete a user who has created/updated/deleted other users (referential integrity)
- ? Use soft delete (`is_deleted = true`) instead of physical deletion
- ? The `DeleteBehavior.Restrict` prevents cascade deletes

### Nullable Fields
All audit fields are nullable (`int?`) because:
- System-created users might not have a `created_by` value
- Initial users won't have a creator
- Not all updates may track who made the change

### Best Practices
1. **Always set audit fields** when creating/updating/deleting users
2. **Use soft deletes** instead of physical deletes
3. **Filter by `is_deleted`** in all user queries
4. **Track who did what** for compliance and debugging

## ?? Related Documentation
- `Docs/DbContext_Configuration_Fix.md` - Initial relationship configuration
- `Docs/LOGIN_FIX_SUMMARY.md` - Login fix summary
- `Docs/Authentication_README.md` - Authentication implementation

---

**Summary**: Your database schema now matches the entity model, enabling full audit trail support with self-referencing relationships! ??
