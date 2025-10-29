# Board API Implementation - CQRS Architecture

## Overview
This implementation provides a complete API to fetch all boards by ProjectId using CQRS (Command Query Responsibility Segregation) architecture pattern with MediatR, AutoMapper, and Entity Framework Core.

## API Endpoint

### Get Boards by Project ID
**Endpoint:** `GET /api/board/project/{projectId}`

**Description:** Fetches all active boards for a specific project, including their associated columns with status information.

**Request:**
- **Method:** GET
- **Route:** `/api/board/project/{projectId:guid}`
- **Parameters:**
  - `projectId` (required): The project GUID

**Response Example:**
```json
{
  "status": 200,
  "data": [
    {
      "id": 1,
      "projectId": "550e8400-e29b-41d4-a716-446655440000",
      "projectName": "My Project",
      "teamId": 5,
      "teamName": "Development Team",
      "name": "Main Board",
      "description": "Primary kanban board",
      "type": "kanban",
      "isActive": true,
      "createdBy": 10,
      "createdByName": "John Doe",
      "updatedBy": 12,
      "updatedByName": "Jane Smith",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-20T14:45:00Z",
      "metadata": "{}",
      "columns": [
        {
          "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
          "statusId": 1,
          "statusName": "To Do",
          "boardColumnName": "Backlog",
          "boardColor": "#FF5733",
          "position": 1
        },
        {
          "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
          "statusId": 2,
          "statusName": "In Progress",
          "boardColumnName": "In Progress",
          "boardColor": "#3498DB",
          "position": 2
        },
        {
          "id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
          "statusId": 3,
          "statusName": "Done",
          "boardColumnName": "Completed",
          "boardColor": "#27AE60",
          "position": 3
        }
      ]
    }
  ],
  "message": "Successfully fetched 1 board(s) for project 550e8400-e29b-41d4-a716-446655440000"
}
```

**Status Codes:**
- `200 OK` - Successfully retrieved boards
- `400 Bad Request` - Invalid project ID
- `500 Internal Server Error` - Server error occurred

## Architecture Components

### 1. Domain Layer (`BACKEND_CQRS.Domain`)

#### Entities Created/Updated:

**Board Entity** (`Entities\Board.cs`)
- Represents a board in the system
- Added `BoardColumns` collection property for eager loading
- Properties: Id, ProjectId, TeamId, Name, Description, Type, IsActive, etc.

**BoardColumn Entity** (`Entities\BoardColumn.cs`)
- Fixed to match database schema
- Changed `board_name` ? `board_column_name`
- Changed `StatusId` type from `Guid` ? `int`

**BoardBoardColumnMap Entity** (`Entities\BoardBoardColumnMap.cs`) ? NEW
- Many-to-many mapping table between boards and board columns
- Properties: Id, BoardId, BoardColumnId, CreatedAt, UpdatedAt

**Status Entity** (`Entities\Status.cs`)
- Fixed to match database schema
- Changed `Id` type from `Guid` ? `int`
- Added `Required` attribute to StatusName

#### Repository Interface (`Persistance\IBoardRepository.cs`) ? NEW
```csharp
public interface IBoardRepository : IGenericRepository<Board>
{
    Task<List<Board>> GetBoardsByProjectIdWithColumnsAsync(Guid projectId);
}
```

### 2. Infrastructure Layer (`BACKEND_CQRS.Infrastructure`)

#### Repository Implementation (`Repository\BoardRepository.cs`) ? NEW
- Implements `IBoardRepository`
- Includes comprehensive error handling
- Eagerly loads related entities (Project, Team, Creator, Updater)
- Fetches board columns through the mapping table
- Orders columns by position

#### DbContext Updates (`Context\AppDbContext.cs`)
- Added DbSet for `Board`
- Added DbSet for `BoardColumn`
- Added DbSet for `BoardBoardColumnMap`
- Added DbSet for `Status`

#### Dependency Injection (`PersistanceServiceRegistration.cs`)
- Registered `IBoardRepository` ? `BoardRepository` mapping

### 3. Application Layer (`BACKEND_CQRS.Application`)

#### DTOs Created:

**BoardColumnDto** (`Dto\BoardColumnDto.cs`) ? NEW
```csharp
public class BoardColumnDto
{
    public Guid Id { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public string? BoardColumnName { get; set; }
    public string? BoardColor { get; set; }
    public int? Position { get; set; }
}
```

**BoardWithColumnsDto** (`Dto\BoardWithColumnsDto.cs`) ? NEW
```csharp
public class BoardWithColumnsDto
{
    public int Id { get; set; }
    public Guid ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; }
    public bool IsActive { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public int? UpdatedBy { get; set; }
    public string? UpdatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? Metadata { get; set; }
    public List<BoardColumnDto> Columns { get; set; }
}
```

#### Query (`Query\GetBoardsByProjectIdQuery.cs`) ? NEW
```csharp
public class GetBoardsByProjectIdQuery : IRequest<List<BoardWithColumnsDto>>
{
    public Guid ProjectId { get; }
    
    public GetBoardsByProjectIdQuery(Guid projectId)
    {
        ProjectId = projectId;
    }
}
```

#### Query Handler (`Handler\GetBoardsByProjectIdQueryHandler.cs`) ? NEW
- Implements `IRequestHandler<GetBoardsByProjectIdQuery, List<BoardWithColumnsDto>>`
- Includes comprehensive logging
- Implements error handling with try-catch
- Returns empty list if no boards found

#### AutoMapper Profile (`MappingProfile\BoardProfile.cs`) ? NEW
```csharp
public class BoardProfile : Profile
{
    public BoardProfile()
    {
        // BoardColumn ? BoardColumnDto
        CreateMap<BoardColumn, BoardColumnDto>()
            .ForMember(dest => dest.StatusName, 
                opt => opt.MapFrom(src => src.Status != null ? src.Status.StatusName : null));

        // Board ? BoardWithColumnsDto
        CreateMap<Board, BoardWithColumnsDto>()
            .ForMember(dest => dest.ProjectName, 
                opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : null))
            .ForMember(dest => dest.TeamName, 
                opt => opt.MapFrom(src => src.Team != null ? src.Team.Name : null))
            .ForMember(dest => dest.CreatedByName, 
                opt => opt.MapFrom(src => src.Creator != null ? src.Creator.Name : null))
            .ForMember(dest => dest.UpdatedByName, 
                opt => opt.MapFrom(src => src.Updater != null ? src.Updater.Name : null))
            .ForMember(dest => dest.Columns, 
                opt => opt.MapFrom(src => src.BoardColumns));
    }
}
```

### 4. API Layer (`BACKEND_CQRS.Api`)

#### Controller (`Controllers\BoardController.cs`) ? NEW
- RESTful API controller
- Implements input validation
- Returns `ApiResponse<T>` wrapper
- Includes comprehensive error handling
- Swagger documentation attributes

## Database Schema

### Tables Involved:

1. **boards**
   - id (integer, PK)
   - project_id (uuid, FK to projects)
   - team_id (integer, FK to teams)
   - name (varchar)
   - description (text)
   - type (varchar)
   - is_active (boolean)
   - created_by, updated_by (integer, FK to users)
   - created_at, updated_at (timestamp)
   - metadata (jsonb)

2. **board_columns**
   - id (uuid, PK)
   - status_id (integer, FK to status)
   - board_column_name (varchar)
   - board_color (varchar)
   - position (integer)

3. **board_boardcolumn_map** ? NEW ENTITY
   - id (bigint, PK, auto-increment)
   - board_id (integer, FK to boards)
   - board_column_id (uuid, FK to board_columns)
   - created_at (timestamp)
   - updated_at (timestamp)

4. **status**
   - id (integer, PK)
   - status_name (varchar)

## Key Features

? **CQRS Pattern**: Clean separation of queries using MediatR
? **Repository Pattern**: Abstraction of data access logic
? **AutoMapper**: Automatic entity-to-DTO mapping
? **Eager Loading**: Efficient loading of related entities
? **Error Handling**: Comprehensive try-catch blocks with logging
? **Null Safety**: Null-conditional operators throughout
? **Input Validation**: Project ID validation in controller
? **Logging**: Structured logging using ILogger
? **API Response Wrapper**: Consistent response format
? **Swagger Documentation**: API documentation attributes
? **Dependency Injection**: Proper DI container registration
? **Async/Await**: Asynchronous programming throughout

## Testing the API

### Using cURL:
```bash
curl -X GET "https://localhost:5001/api/board/project/550e8400-e29b-41d4-a716-446655440000" -H "accept: application/json"
```

### Using PowerShell:
```powershell
$projectId = "550e8400-e29b-41d4-a716-446655440000"
Invoke-RestMethod -Uri "https://localhost:5001/api/board/project/$projectId" -Method Get
```

### Using Swagger UI:
1. Navigate to `https://localhost:5001/swagger`
2. Find `GET /api/board/project/{projectId}`
3. Click "Try it out"
4. Enter a valid project GUID
5. Click "Execute"

## Build and Run

```bash
# Build the solution
dotnet build

# Run the API
dotnet run --project BACKEND_CQRS.Api

# Run tests (if available)
dotnet test
```

## Error Handling

The API includes multiple layers of error handling:

1. **Controller Level**: Validates input and catches exceptions
2. **Handler Level**: Logs operations and handles domain errors
3. **Repository Level**: Wraps database operations with try-catch

All errors return appropriate HTTP status codes and descriptive messages.

## Future Enhancements

- Add pagination support for large result sets
- Implement caching (Redis/In-Memory)
- Add filtering capabilities (by board type, team, etc.)
- Implement board CRUD operations (Create, Update, Delete)
- Add authorization/authentication
- Implement unit tests and integration tests
- Add request validation using FluentValidation
- Implement soft delete for boards

## Notes

- The API only returns active boards (`is_active = true`)
- Board columns are ordered by their position
- All timestamps are in UTC
- The response includes denormalized data (names) for convenience
- AutoMapper automatically registers all profiles on startup
- MediatR automatically discovers and registers all handlers

---
**Author:** AI Assistant  
**Date:** 2024  
**Version:** 1.0
