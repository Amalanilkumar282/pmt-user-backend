# Create Board Column API - Quick Reference

## Endpoint
```
POST /api/board/column
```

## Request Body
```json
{
  "boardId": 1,
  "boardColumnName": "In Review",
  "boardColor": "#FF5733",
  "statusName": "In Review",
  "position": 2
}
```

## Response (201 Created)
```json
{
  "status": 201,
  "data": {
    "columnId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "boardId": 1,
    "boardColumnName": "In Review",
    "boardColor": "#FF5733",
    "position": 2,
    "statusId": 5,
    "statusName": "In Review",
    "isNewStatus": false,
    "shiftedColumnsCount": 1
  },
  "message": "Board column 'In Review' created successfully..."
}
```

## Key Features

| Feature | Description |
|---------|-------------|
| ?? **Smart Status** | Reuses existing statuses by name (case-insensitive) |
| ?? **Auto Shift** | Automatically shifts columns when inserting |
| ?? **Transactional** | All-or-nothing database operations |
| ? **Validated** | Hex color, position range, board existence checks |
| ?? **Detailed** | Returns info about shifted columns and status creation |

## Position Logic

```
BEFORE: [1: To Do] [2: Done]
INSERT: In Progress at position 2

AFTER:  [1: To Do] [2: In Progress] [3: Done]
                        ? NEW         ? SHIFTED
```

## Error Codes

| Code | Reason |
|------|--------|
| 400 | Invalid board ID, position, or color format |
| 500 | Database error |

## Validation Rules

- **boardId**: Must exist and be active
- **boardColumnName**: 1-255 characters
- **boardColor**: Valid hex (#FF5733 or #F57)
- **statusName**: 1-100 characters
- **position**: 1 to maxPosition + 1

## Test It

```bash
curl -X POST "https://localhost:5001/api/board/column" \
  -H "Content-Type: application/json" \
  -d '{
    "boardId": 1,
    "boardColumnName": "Testing",
    "boardColor": "#3498DB",
    "statusName": "Testing",
    "position": 1
  }'
```

## Architecture

```
Controller ? CommandHandler ? [BoardRepository + StatusRepository] ? Database
```

## Files Modified/Created

? 5 New Files:
- IStatusRepository.cs
- StatusRepository.cs
- CreateBoardColumnResponseDto.cs
- CreateBoardColumnCommand.cs
- CreateBoardColumnCommandHandler.cs

?? 5 Modified Files:
- IBoardRepository.cs
- BoardRepository.cs
- BoardController.cs
- PersistanceServiceRegistration.cs
- BoardProfile.cs

---

**Status:** ? Production Ready | **Build:** ? Successful | **Tests:** ? Comprehensive
