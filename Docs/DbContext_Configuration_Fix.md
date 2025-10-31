# Entity Framework DbContext Configuration Fix

## Problem
The application was experiencing runtime errors during login:

### Error 1: Model Configuration Error
```
System.InvalidOperationException: Unable to determine the relationship represented by navigation 'Projects.Creator' of type 'Users'. Either manually configure the relationship, or ignore this property using the '[NotMapped]' attribute or by using 'EntityTypeBuilder.Ignore' in 'OnModelCreating'.
```

### Error 2: Database Column Missing
```
Npgsql.PostgresException: 42703: column u.created_by does not exist
Hint: Perhaps you meant to reference the column "u.created_at".
```

## Root Causes

### Cause 1: Ambiguous Relationships
Entity Framework Core was unable to automatically determine relationships between entities when:
1. Multiple navigation properties on one entity point to the same related entity
2. The target entity doesn't have corresponding inverse navigation properties for all relationships

Specifically, entities like `Projects`, `Epic`, `Board`, `Issue`, `Message`, and `Mention` all had `Creator` and `Updater` navigation properties pointing to `Users`, but the `Users` entity didn't have inverse collections for these relationships.

### Cause 2: Entity-Database Schema Mismatch
The `Users` entity model defined properties (`CreatedBy`, `UpdatedBy`, `DeletedBy`) and navigation properties that don't exist in the actual database table. Entity Framework tried to query these non-existent columns, causing runtime errors.

## Solution

### Part 1: Users Entity - Mark Non-Existent Columns as NotMapped

Updated `BACKEND_CQRS.Domain/Entities/Users.cs`:

```csharp
// These columns don't exist in the database, so mark them as NotMapped
[NotMapped]
public int? CreatedBy { get; set; }

[NotMapped]
public int? DeletedBy { get; set; }

[NotMapped]
public int? UpdatedBy { get; set; }

// Navigation properties - mark as NotMapped since the FK columns don't exist
[NotMapped]
public Users? CreatedByUser { get; set; }

[NotMapped]
public Users? DeletedByUser { get; set; }

[NotMapped]
public Users? UpdatedByUser { get; set; }
```

### Part 2: AppDbContext - Explicit Relationship Configuration

Configured all ambiguous relationships in `AppDbContext.OnModelCreating` using the Fluent API.

#### 1. Added DbSet for Message entity (already existed but added configuration)
```csharp
public DbSet<Message> Messages { get; set; }
public DbSet<Mention> Mentions { get; set; }
```

#### 2. Configured Projects Relationships
```csharp
modelBuilder.Entity<Projects>(entity =>
{
    // ProjectManager has inverse collection: Users.ManagedProjects
    entity.HasOne(p => p.ProjectManager)
          .WithMany(u => u.ManagedProjects)
          .HasForeignKey(p => p.ProjectManagerId)
          .OnDelete(DeleteBehavior.Restrict);

    // Creator has no inverse collection
    entity.HasOne(p => p.Creator)
          .WithMany()
          .HasForeignKey(p => p.CreatedBy)
          .OnDelete(DeleteBehavior.Restrict);

    // Updater has no inverse collection
    entity.HasOne(p => p.Updater)
          .WithMany()
          .HasForeignKey(p => p.UpdatedBy)
          .OnDelete(DeleteBehavior.Restrict);
});
```

#### 3. Configured Epic Relationships
```csharp
modelBuilder.Entity<Epic>(entity =>
{
    entity.HasOne(e => e.Assignee).WithMany().HasForeignKey(e => e.AssigneeId).OnDelete(DeleteBehavior.Restrict);
    entity.HasOne(e => e.Reporter).WithMany().HasForeignKey(e => e.ReporterId).OnDelete(DeleteBehavior.Restrict);
    entity.HasOne(e => e.Creator).WithMany().HasForeignKey(e => e.CreatedBy).OnDelete(DeleteBehavior.Restrict);
    entity.HasOne(e => e.Updater).WithMany().HasForeignKey(e => e.UpdatedBy).OnDelete(DeleteBehavior.Restrict);
});
```

#### 4. Configured Board Relationships
```csharp
modelBuilder.Entity<Board>(entity =>
{
    entity.HasOne(b => b.Creator).WithMany().HasForeignKey(b => b.CreatedBy).OnDelete(DeleteBehavior.Restrict);
    entity.HasOne(b => b.Updater).WithMany().HasForeignKey(b => b.UpdatedBy).OnDelete(DeleteBehavior.Restrict);
});
```

#### 5. Configured Issue Relationships
```csharp
modelBuilder.Entity<Issue>(entity =>
{
    entity.HasOne(i => i.Assignee).WithMany().HasForeignKey(i => i.AssigneeId).OnDelete(DeleteBehavior.Restrict);
    entity.HasOne(i => i.Reporter).WithMany().HasForeignKey(i => i.ReporterId).OnDelete(DeleteBehavior.Restrict);
    entity.HasOne(i => i.Creator).WithMany().HasForeignKey(i => i.CreatedBy).OnDelete(DeleteBehavior.Restrict);
    entity.HasOne(i => i.Updater).WithMany().HasForeignKey(i => i.UpdatedBy).OnDelete(DeleteBehavior.Restrict);
});
```

#### 6. Configured Teams Relationships
Teams entity uses `ProjectMembers` for audit fields instead of `Users`:
```csharp
modelBuilder.Entity<Teams>(entity =>
{
    entity.HasOne(t => t.Lead).WithMany().HasForeignKey(t => t.LeadId).OnDelete(DeleteBehavior.Restrict);
    entity.HasOne(t => t.CreatedByMember).WithMany().HasForeignKey(t => t.CreatedBy).OnDelete(DeleteBehavior.Restrict);
    entity.HasOne(t => t.UpdatedByMember).WithMany().HasForeignKey(t => t.UpdatedBy).OnDelete(DeleteBehavior.Restrict);
});
```

#### 7. Configured Mention Relationships
```csharp
modelBuilder.Entity<Mention>(entity =>
{
    entity.HasOne(m => m.MentionedUser).WithMany().HasForeignKey(m => m.MentionUserId).OnDelete(DeleteBehavior.Restrict);
    entity.HasOne(m => m.Creator).WithMany().HasForeignKey(m => m.CreatedBy).OnDelete(DeleteBehavior.Restrict);
    entity.HasOne(m => m.Updater).WithMany().HasForeignKey(m => m.UpdatedBy).OnDelete(DeleteBehavior.Restrict);
});
```

#### 8. Configured Message Relationships
```csharp
modelBuilder.Entity<Message>(entity =>
{
    entity.HasOne(msg => msg.MentionedUser).WithMany().HasForeignKey(msg => msg.MentionUserId).OnDelete(DeleteBehavior.Restrict);
    entity.HasOne(msg => msg.Creator).WithMany().HasForeignKey(msg => msg.CreatedBy).OnDelete(DeleteBehavior.Restrict);
    entity.HasOne(msg => msg.Updater).WithMany().HasForeignKey(msg => msg.UpdatedBy).OnDelete(DeleteBehavior.Restrict);
});
```

## Key Patterns Used

### Pattern 1: One-to-Many with Inverse Collection
When the target entity has a collection property:
```csharp
entity.HasOne(x => x.NavigationProperty)
      .WithMany(y => y.CollectionProperty)
      .HasForeignKey(x => x.ForeignKeyProperty)
      .OnDelete(DeleteBehavior.Restrict);
```

### Pattern 2: One-to-Many without Inverse Collection
When the target entity doesn't have a corresponding collection:
```csharp
entity.HasOne(x => x.NavigationProperty)
      .WithMany()  // Empty - no inverse collection
      .HasForeignKey(x => x.ForeignKeyProperty)
      .OnDelete(DeleteBehavior.Restrict);
```

### Pattern 3: NotMapped Properties
When entity properties don't exist in the database:
```csharp
[NotMapped]
public int? PropertyName { get; set; }

[NotMapped]
public RelatedEntity? NavigationProperty { get; set; }
```

## Benefits
1. **Explicit Configuration**: Clear and maintainable relationship definitions
2. **No Ambiguity**: Entity Framework knows exactly how to map relationships
3. **Cascade Behavior Control**: All relationships use `Restrict` to prevent accidental deletions
4. **Runtime Stability**: Eliminates configuration errors during model initialization
5. **Database Schema Alignment**: Entity model now matches the actual database schema

## Testing
- ? Build successful
- ? All entity relationships explicitly configured
- ? Non-existent columns marked as NotMapped
- ? Login functionality should now work correctly

## Important Notes

### Database Schema vs Entity Model
The `Users` table in the database does **NOT** have the following columns:
- `created_by`
- `updated_by`
- `deleted_by`

These properties exist in the entity model for potential future use but are marked as `[NotMapped]` to prevent EF Core from trying to query them.

### Self-Referencing Audit
If you need to track who created/updated/deleted users in the future, you'll need to:
1. Add migration to create these columns in the database
2. Remove the `[NotMapped]` attributes from the properties
3. Add back the configuration in `OnModelCreating` for the self-referencing relationships

## Next Steps

### If You Encounter Similar Errors:
1. **Check the error message** - It will tell you which entity/navigation is problematic
2. **Verify database schema** - Use pgAdmin or similar tool to confirm column existence
3. **Add explicit configuration** in `OnModelCreating` method
4. **Use NotMapped** for properties that shouldn't be persisted to the database

### Pattern to Follow:
```csharp
// In entity class
[NotMapped]
public PropertyType PropertyName { get; set; }

// OR in DbContext.OnModelCreating
modelBuilder.Entity<EntityName>(entity =>
{
    entity.HasOne(e => e.NavigationProperty)
          .WithMany() // or .WithMany(x => x.Collection)
          .HasForeignKey(e => e.ForeignKeyProperty)
          .OnDelete(DeleteBehavior.Restrict);
});
```

## Files Modified
1. `BACKEND_CQRS.Infrastructure/Context/AppDbContext.cs` - Added explicit relationship configurations
2. `BACKEND_CQRS.Domain/Entities/Users.cs` - Marked non-existent properties as NotMapped

