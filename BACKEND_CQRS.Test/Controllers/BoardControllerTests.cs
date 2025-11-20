using BACKEND_CQRS.Api.Controllers;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Query.Boards;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BACKEND_CQRS.Test.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for BoardController
    /// Tests all 9 API endpoints with success, validation, error, and edge case scenarios
    /// </summary>
    public class BoardControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<BoardController>> _loggerMock;
        private readonly BoardController _controller;

        public BoardControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<BoardController>>();
            _controller = new BoardController(_mediatorMock.Object, _loggerMock.Object);
        }

        #region 1. CreateBoard Tests

        [Fact]
        public async Task CreateBoard_WithValidCommand_ReturnsCreatedAtAction()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var command = new CreateBoardCommand(
                projectId: projectId,
                name: "Test Board",
                type: "kanban",
                description: "Test Description",
                createdBy: 1
            );

            var expectedResponse = ApiResponse<CreateBoardResponseDto>.Created(
                new CreateBoardResponseDto
                {
                    BoardId = 1,
                    ProjectId = projectId,
                    ProjectName = "Test Project",
                    Name = "Test Board",
                    Type = "kanban",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateBoardCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateBoard(command);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdResult.StatusCode);
            var response = Assert.IsType<ApiResponse<CreateBoardResponseDto>>(createdResult.Value);
            Assert.Equal(201, response.Status);
            Assert.Equal("Test Board", response.Data.Name);
        }

        [Fact]
        public async Task CreateBoard_WithEmptyProjectId_ReturnsBadRequest()
        {
            // Arrange
            var command = new CreateBoardCommand(
                projectId: Guid.Empty,
                name: "Test Board",
                type: "kanban"
            );

            // Act
            var result = await _controller.CreateBoard(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<CreateBoardResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("Invalid project ID", response.Message);
        }

        [Fact]
        public async Task CreateBoard_WithTeamType_AndTeamId_ReturnsCreated()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var command = new CreateBoardCommand(
                projectId: projectId,
                name: "Team Board",
                type: "team",
                teamId: 5,
                createdBy: 1
            );

            var expectedResponse = ApiResponse<CreateBoardResponseDto>.Created(
                new CreateBoardResponseDto
                {
                    BoardId = 1,
                    ProjectId = projectId,
                    Name = "Team Board",
                    Type = "team",
                    TeamId = 5,
                    TeamName = "Alpha Team",
                    IsTeamBased = true
                }
            );

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateBoardCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateBoard(command);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdResult.StatusCode);
            var response = Assert.IsType<ApiResponse<CreateBoardResponseDto>>(createdResult.Value);
            Assert.True(response.Data.IsTeamBased);
            Assert.Equal(5, response.Data.TeamId);
        }

        [Fact]
        public async Task CreateBoard_WhenMediatorReturns400_ReturnsBadRequest()
        {
            // Arrange
            var command = new CreateBoardCommand(
                projectId: Guid.NewGuid(),
                name: "Test Board",
                type: "kanban"
            );

            var failureResponse = ApiResponse<CreateBoardResponseDto>.Fail("Project does not exist");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateBoardCommand>(), default))
                .ReturnsAsync(failureResponse);

            // Act
            var result = await _controller.CreateBoard(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<CreateBoardResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
        }

        [Fact]
        public async Task CreateBoard_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var command = new CreateBoardCommand(
                projectId: Guid.NewGuid(),
                name: "Test Board",
                type: "kanban"
            );

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateBoardCommand>(), default))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateBoard(command);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        #endregion

        #region 2. GetBoardsByProjectId Tests

        [Fact]
        public async Task GetBoardsByProjectId_WithValidProjectId_ReturnsOkWithBoards()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var expectedBoards = new List<BoardWithColumnsDto>
            {
                new BoardWithColumnsDto
                {
                    Id = 1,
                    ProjectId = projectId,
                    Name = "Board 1",
                    Type = "kanban",
                    IsActive = true,
                    Columns = new List<BoardColumnDto>()
                },
                new BoardWithColumnsDto
                {
                    Id = 2,
                    ProjectId = projectId,
                    Name = "Board 2",
                    Type = "scrum",
                    IsActive = true,
                    Columns = new List<BoardColumnDto>()
                }
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetBoardsByProjectIdQuery>(), default))
                .ReturnsAsync(expectedBoards);

            // Act
            var result = await _controller.GetBoardsByProjectId(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<List<BoardWithColumnsDto>>>(okResult.Value);
            Assert.Equal(200, response.Status);
            Assert.Equal(2, response.Data.Count);
            Assert.Contains("Successfully fetched 2 board(s)", response.Message);
        }

        [Fact]
        public async Task GetBoardsByProjectId_WithEmptyProjectId_ReturnsBadRequest()
        {
            // Arrange
            var projectId = Guid.Empty;

            // Act
            var result = await _controller.GetBoardsByProjectId(projectId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("Invalid project ID", response.Message);
        }

        [Fact]
        public async Task GetBoardsByProjectId_WithNoBoards_ReturnsOkWithEmptyList()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var emptyList = new List<BoardWithColumnsDto>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetBoardsByProjectIdQuery>(), default))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _controller.GetBoardsByProjectId(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<List<BoardWithColumnsDto>>>(okResult.Value);
            Assert.Equal(200, response.Status);
            Assert.Empty(response.Data);
            Assert.Contains("No active boards found", response.Message);
        }

        [Fact]
        public async Task GetBoardsByProjectId_WhenProjectNotFound_ReturnsNotFound()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetBoardsByProjectIdQuery>(), default))
                .ThrowsAsync(new KeyNotFoundException($"Project not found"));

            // Act
            var result = await _controller.GetBoardsByProjectId(projectId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<object>>(notFoundResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("does not exist", response.Message);
        }

        [Fact]
        public async Task GetBoardsByProjectId_WhenDatabaseError_ReturnsInternalServerError()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetBoardsByProjectIdQuery>(), default))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act
            var result = await _controller.GetBoardsByProjectId(projectId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        #endregion

        #region 3. GetBoardColumnsByBoardId Tests

        [Fact]
        public async Task GetBoardColumnsByBoardId_WithValidBoardId_ReturnsOkWithColumns()
        {
            // Arrange
            var boardId = 1;
            var expectedColumns = new List<BoardColumnDto>
            {
                new BoardColumnDto
                {
                    Id = Guid.NewGuid(),
                    BoardColumnName = "To Do",
                    BoardColor = "#FF0000",
                    Position = 1,
                    StatusId = 1,
                    StatusName = "To Do"
                },
                new BoardColumnDto
                {
                    Id = Guid.NewGuid(),
                    BoardColumnName = "In Progress",
                    BoardColor = "#00FF00",
                    Position = 2,
                    StatusId = 2,
                    StatusName = "In Progress"
                }
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetBoardColumnsByBoardIdQuery>(), default))
                .ReturnsAsync(expectedColumns);

            // Act
            var result = await _controller.GetBoardColumnsByBoardId(boardId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<List<BoardColumnDto>>>(okResult.Value);
            Assert.Equal(200, response.Status);
            Assert.Equal(2, response.Data.Count);
            Assert.Contains("Successfully fetched 2 column(s)", response.Message);
        }

        [Fact]
        public async Task GetBoardColumnsByBoardId_WithInvalidBoardId_ReturnsBadRequest()
        {
            // Arrange
            var boardId = 0;

            // Act
            var result = await _controller.GetBoardColumnsByBoardId(boardId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("Invalid board ID", response.Message);
        }

        [Fact]
        public async Task GetBoardColumnsByBoardId_WithNegativeBoardId_ReturnsBadRequest()
        {
            // Arrange
            var boardId = -1;

            // Act
            var result = await _controller.GetBoardColumnsByBoardId(boardId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, ((ApiResponse<object>)badRequestResult.Value).Status);
        }

        [Fact]
        public async Task GetBoardColumnsByBoardId_WhenBoardNotFound_ReturnsNotFound()
        {
            // Arrange
            var boardId = 999;

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetBoardColumnsByBoardIdQuery>(), default))
                .ThrowsAsync(new KeyNotFoundException("Board not found"));

            // Act
            var result = await _controller.GetBoardColumnsByBoardId(boardId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<object>>(notFoundResult.Value);
            Assert.Contains("does not exist or is inactive", response.Message);
        }

        #endregion

        #region 4. CreateBoardColumn Tests

        [Fact]
        public async Task CreateBoardColumn_WithValidCommand_ReturnsCreatedAtAction()
        {
            // Arrange
            var command = new CreateBoardColumnCommand
            {
                BoardId = 1,
                BoardColumnName = "To Do",
                BoardColor = "#FF0000",
                StatusName = "To Do",
                Position = 1
            };

            var expectedResponse = ApiResponse<CreateBoardColumnResponseDto>.Created(
                new CreateBoardColumnResponseDto
                {
                    ColumnId = Guid.NewGuid(),
                    BoardId = 1,
                    BoardColumnName = "To Do",
                    BoardColor = "#FF0000",
                    Position = 1,
                    StatusId = 1,
                    StatusName = "To Do",
                    IsNewStatus = false,
                    ShiftedColumnsCount = 0
                }
            );

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateBoardColumnCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateBoardColumn(command);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdResult.StatusCode);
            var response = Assert.IsType<ApiResponse<CreateBoardColumnResponseDto>>(createdResult.Value);
            Assert.Equal(201, response.Status);
            Assert.Equal("To Do", response.Data.BoardColumnName);
        }

        [Fact]
        public async Task CreateBoardColumn_WhenMediatorReturns400_ReturnsBadRequest()
        {
            // Arrange
            var command = new CreateBoardColumnCommand
            {
                BoardId = 999,
                BoardColumnName = "To Do",
                BoardColor = "#FF0000",
                StatusName = "To Do",
                Position = 1
            };

            var failureResponse = ApiResponse<CreateBoardColumnResponseDto>.Fail("Board does not exist");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateBoardColumnCommand>(), default))
                .ReturnsAsync(failureResponse);

            // Act
            var result = await _controller.CreateBoardColumn(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<CreateBoardColumnResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
        }

        [Fact]
        public async Task CreateBoardColumn_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var command = new CreateBoardColumnCommand
            {
                BoardId = 1,
                BoardColumnName = "To Do",
                BoardColor = "#FF0000",
                StatusName = "To Do",
                Position = 1
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateBoardColumnCommand>(), default))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateBoardColumn(command);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        #endregion

        #region 5. DeleteBoardColumn Tests

        [Fact]
        public async Task DeleteBoardColumn_WithValidIds_ReturnsOk()
        {
            // Arrange
            var columnId = Guid.NewGuid();
            var boardId = 1;

            var expectedResponse = ApiResponse<DeleteBoardColumnResponseDto>.Success(
                new DeleteBoardColumnResponseDto
                {
                    ColumnId = columnId,
                    BoardId = boardId,
                    BoardColumnName = "Done",
                    Position = 3,
                    ReorderedColumnsCount = 2,
                    WasDeleted = true
                }
            );

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteBoardColumnCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.DeleteBoardColumn(columnId, boardId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<DeleteBoardColumnResponseDto>>(okResult.Value);
            Assert.Equal(200, response.Status);
            Assert.True(response.Data.WasDeleted);
            Assert.Equal(2, response.Data.ReorderedColumnsCount);
        }

        [Fact]
        public async Task DeleteBoardColumn_WithEmptyColumnId_ReturnsBadRequest()
        {
            // Arrange
            var columnId = Guid.Empty;
            var boardId = 1;

            // Act
            var result = await _controller.DeleteBoardColumn(columnId, boardId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<DeleteBoardColumnResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("Invalid column ID", response.Message);
        }

        [Fact]
        public async Task DeleteBoardColumn_WithInvalidBoardId_ReturnsBadRequest()
        {
            // Arrange
            var columnId = Guid.NewGuid();
            var boardId = 0;

            // Act
            var result = await _controller.DeleteBoardColumn(columnId, boardId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<DeleteBoardColumnResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("Invalid board ID", response.Message);
        }

        [Fact]
        public async Task DeleteBoardColumn_WhenColumnNotFound_ReturnsBadRequest()
        {
            // Arrange
            var columnId = Guid.NewGuid();
            var boardId = 1;

            var failureResponse = ApiResponse<DeleteBoardColumnResponseDto>.Fail("Column not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteBoardColumnCommand>(), default))
                .ReturnsAsync(failureResponse);

            // Act
            var result = await _controller.DeleteBoardColumn(columnId, boardId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<DeleteBoardColumnResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
        }

        [Fact]
        public async Task DeleteBoardColumn_WithDeletedByParameter_ReturnsOk()
        {
            // Arrange
            var columnId = Guid.NewGuid();
            var boardId = 1;
            var deletedBy = 123;

            var expectedResponse = ApiResponse<DeleteBoardColumnResponseDto>.Success(
                new DeleteBoardColumnResponseDto
                {
                    ColumnId = columnId,
                    BoardId = boardId,
                    BoardColumnName = "Done",
                    Position = 3,
                    ReorderedColumnsCount = 2,
                    WasDeleted = true
                }
            );

            _mediatorMock.Setup(m => m.Send(It.Is<DeleteBoardColumnCommand>(c =>
                c.ColumnId == columnId && c.BoardId == boardId && c.DeletedBy == deletedBy), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.DeleteBoardColumn(columnId, boardId, deletedBy);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, ((ApiResponse<DeleteBoardColumnResponseDto>)okResult.Value).Status);
        }

        #endregion

        #region 6. UpdateBoardColumn Tests

        [Fact]
        public async Task UpdateBoardColumn_WithValidCommand_ReturnsOk()
        {
            // Arrange
            var columnId = Guid.NewGuid();
            var command = new UpdateBoardColumnCommand
            {
                BoardId = 1,
                BoardColumnName = "In Progress Updated",
                BoardColor = "#00FF00"
            };

            var expectedResponse = ApiResponse<UpdateBoardColumnResponseDto>.Success(
                new UpdateBoardColumnResponseDto
                {
                    ColumnId = columnId,
                    BoardId = 1
                }
            );

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateBoardColumnCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateBoardColumn(columnId, command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UpdateBoardColumnResponseDto>>(okResult.Value);
            Assert.Equal(200, response.Status);
        }

        [Fact]
        public async Task UpdateBoardColumn_WithEmptyColumnId_ReturnsBadRequest()
        {
            // Arrange
            var columnId = Guid.Empty;
            var command = new UpdateBoardColumnCommand { BoardId = 1 };

            // Act
            var result = await _controller.UpdateBoardColumn(columnId, command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UpdateBoardColumnResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("Invalid column ID", response.Message);
        }

        [Fact]
        public async Task UpdateBoardColumn_WithInvalidBoardId_ReturnsBadRequest()
        {
            // Arrange
            var columnId = Guid.NewGuid();
            var command = new UpdateBoardColumnCommand { BoardId = 0 };

            // Act
            var result = await _controller.UpdateBoardColumn(columnId, command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UpdateBoardColumnResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("BoardId is required", response.Message);
        }

        [Fact]
        public async Task UpdateBoardColumn_WithInvalidColorFormat_ReturnsBadRequest()
        {
            // Arrange
            var columnId = Guid.NewGuid();
            var command = new UpdateBoardColumnCommand
            {
                BoardId = 1,
                BoardColor = "INVALID"
            };

            // Act
            var result = await _controller.UpdateBoardColumn(columnId, command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UpdateBoardColumnResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("hex format", response.Message);
        }

        [Fact]
        public async Task UpdateBoardColumn_WithNameTooLong_ReturnsBadRequest()
        {
            // Arrange
            var columnId = Guid.NewGuid();
            var command = new UpdateBoardColumnCommand
            {
                BoardId = 1,
                BoardColumnName = new string('A', 101) // 101 characters
            };

            // Act
            var result = await _controller.UpdateBoardColumn(columnId, command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UpdateBoardColumnResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("cannot exceed 100 characters", response.Message);
        }

        [Fact]
        public async Task UpdateBoardColumn_WithInvalidPosition_ReturnsBadRequest()
        {
            // Arrange
            var columnId = Guid.NewGuid();
            var command = new UpdateBoardColumnCommand
            {
                BoardId = 1,
                Position = 0
            };

            // Act
            var result = await _controller.UpdateBoardColumn(columnId, command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UpdateBoardColumnResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("Position must be greater than 0", response.Message);
        }

        [Fact]
        public async Task UpdateBoardColumn_WithStatusNameTooLong_ReturnsBadRequest()
        {
            // Arrange
            var columnId = Guid.NewGuid();
            var command = new UpdateBoardColumnCommand
            {
                BoardId = 1,
                StatusName = new string('A', 101) // 101 characters
            };

            // Act
            var result = await _controller.UpdateBoardColumn(columnId, command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UpdateBoardColumnResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("StatusName cannot exceed 100 characters", response.Message);
        }

        #endregion

        #region 7. DeleteBoard Tests

        [Fact]
        public async Task DeleteBoard_WithValidBoardId_ReturnsOk()
        {
            // Arrange
            var boardId = 1;

            var expectedResponse = ApiResponse<bool>.Success(true, "Board deleted successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteBoardCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.DeleteBoard(boardId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.Equal(200, response.Status);
            Assert.True(response.Data);
        }

        [Fact]
        public async Task DeleteBoard_WithInvalidBoardId_ReturnsBadRequest()
        {
            // Arrange
            var boardId = 0;

            // Act
            var result = await _controller.DeleteBoard(boardId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<bool>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("Invalid board ID", response.Message);
        }

        [Fact]
        public async Task DeleteBoard_WithNegativeBoardId_ReturnsBadRequest()
        {
            // Arrange
            var boardId = -1;

            // Act
            var result = await _controller.DeleteBoard(boardId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<bool>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
        }

        [Fact]
        public async Task DeleteBoard_WhenBoardNotFound_ReturnsBadRequest()
        {
            // Arrange
            var boardId = 999;

            var failureResponse = ApiResponse<bool>.Fail("Board not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteBoardCommand>(), default))
                .ReturnsAsync(failureResponse);

            // Act
            var result = await _controller.DeleteBoard(boardId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<bool>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
        }

        [Fact]
        public async Task DeleteBoard_WithDeletedBy_ReturnsOk()
        {
            // Arrange
            var boardId = 1;
            var deletedBy = 123;

            var expectedResponse = ApiResponse<bool>.Success(true, "Board deleted successfully");

            _mediatorMock.Setup(m => m.Send(It.Is<DeleteBoardCommand>(c =>
                c.BoardId == boardId && c.DeletedBy == deletedBy), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.DeleteBoard(boardId, deletedBy);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, ((ApiResponse<bool>)okResult.Value).Status);
        }

        [Fact]
        public async Task DeleteBoard_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var boardId = 1;

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteBoardCommand>(), default))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.DeleteBoard(boardId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        #endregion

        #region 8. UpdateBoard Tests

        [Fact]
        public async Task UpdateBoard_WithValidCommand_ReturnsOk()
        {
            // Arrange
            var boardId = 1;
            var command = new UpdateBoardCommand
            {
                Name = "Updated Board Name",
                Description = "Updated Description",
                Type = "scrum"
            };

            var expectedResponse = ApiResponse<UpdateBoardResponseDto>.Success(
                new UpdateBoardResponseDto
                {
                    BoardId = boardId,
                    ProjectId = Guid.NewGuid(),
                    ProjectName = "Test Project"
                }
            );

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateBoardCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateBoard(boardId, command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UpdateBoardResponseDto>>(okResult.Value);
            Assert.Equal(200, response.Status);
        }

        [Fact]
        public async Task UpdateBoard_WithInvalidBoardId_ReturnsBadRequest()
        {
            // Arrange
            var boardId = 0;
            var command = new UpdateBoardCommand { Name = "Test" };

            // Act
            var result = await _controller.UpdateBoard(boardId, command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UpdateBoardResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("Invalid board ID", response.Message);
        }

        [Fact]
        public async Task UpdateBoard_WithNameTooLong_ReturnsBadRequest()
        {
            // Arrange
            var boardId = 1;
            var command = new UpdateBoardCommand
            {
                Name = new string('A', 151) // 151 characters
            };

            // Act
            var result = await _controller.UpdateBoard(boardId, command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UpdateBoardResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("cannot exceed 150 characters", response.Message);
        }

        [Fact]
        public async Task UpdateBoard_WithEmptyName_ReturnsBadRequest()
        {
            // Arrange
            var boardId = 1;
            var command = new UpdateBoardCommand
            {
                Name = "   " // Whitespace only
            };

            // Act
            var result = await _controller.UpdateBoard(boardId, command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UpdateBoardResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("cannot be empty or whitespace", response.Message);
        }

        [Fact]
        public async Task UpdateBoard_WithDescriptionTooLong_ReturnsBadRequest()
        {
            // Arrange
            var boardId = 1;
            var command = new UpdateBoardCommand
            {
                Description = new string('A', 1001) // 1001 characters
            };

            // Act
            var result = await _controller.UpdateBoard(boardId, command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UpdateBoardResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("Description cannot exceed 1000 characters", response.Message);
        }

        [Fact]
        public async Task UpdateBoard_WithInvalidType_ReturnsBadRequest()
        {
            // Arrange
            var boardId = 1;
            var command = new UpdateBoardCommand
            {
                Type = "invalid_type"
            };

            // Act
            var result = await _controller.UpdateBoard(boardId, command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UpdateBoardResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("Type must be one of", response.Message);
        }

        [Fact]
        public async Task UpdateBoard_WithNegativeTeamId_ReturnsBadRequest()
        {
            // Arrange
            var boardId = 1;
            var command = new UpdateBoardCommand
            {
                TeamId = -1
            };

            // Act
            var result = await _controller.UpdateBoard(boardId, command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UpdateBoardResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("TeamId must be a positive number", response.Message);
        }

        [Fact]
        public async Task UpdateBoard_WithValidType_ReturnsOk()
        {
            // Arrange
            var boardId = 1;
            var command = new UpdateBoardCommand
            {
                Type = "kanban"
            };

            var expectedResponse = ApiResponse<UpdateBoardResponseDto>.Success(
                new UpdateBoardResponseDto
                {
                    BoardId = boardId,
                    ProjectId = Guid.NewGuid(),
                    ProjectName = "Test Project"
                }
            );

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateBoardCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateBoard(boardId, command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, ((ApiResponse<UpdateBoardResponseDto>)okResult.Value).Status);
        }

        [Fact]
        public async Task UpdateBoard_WhenMediatorReturns400_ReturnsBadRequest()
        {
            // Arrange
            var boardId = 1;
            var command = new UpdateBoardCommand
            {
                Name = "Test Board"
            };

            var failureResponse = ApiResponse<UpdateBoardResponseDto>.Fail("Board not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateBoardCommand>(), default))
                .ReturnsAsync(failureResponse);

            // Act
            var result = await _controller.UpdateBoard(boardId, command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UpdateBoardResponseDto>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
        }

        [Fact]
        public async Task UpdateBoard_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var boardId = 1;
            var command = new UpdateBoardCommand
            {
                Name = "Test Board"
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateBoardCommand>(), default))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdateBoard(boardId, command);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        #endregion

        #region 9. GetBoardById Tests

        [Fact]
        public async Task GetBoardById_WithValidBoardId_ReturnsOk()
        {
            // Arrange
            var boardId = 1;
            var expectedBoard = new BoardWithColumnsDto
            {
                Id = boardId,
                Name = "Test Board",
                Type = "kanban",
                IsActive = true,
                ProjectId = Guid.NewGuid(),
                ProjectName = "Test Project",
                Columns = new List<BoardColumnDto>
                {
                    new BoardColumnDto
                    {
                        Id = Guid.NewGuid(),
                        BoardColumnName = "To Do",
                        Position = 1
                    }
                }
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetBoardByIdQuery>(), default))
                .ReturnsAsync(expectedBoard);

            // Act
            var result = await _controller.GetBoardById(boardId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<BoardWithColumnsDto>>(okResult.Value);
            Assert.Equal(200, response.Status);
            Assert.Equal("Test Board", response.Data.Name);
            Assert.Single(response.Data.Columns);
            Assert.Contains("Successfully fetched board", response.Message);
        }

        [Fact]
        public async Task GetBoardById_WithInvalidBoardId_ReturnsBadRequest()
        {
            // Arrange
            var boardId = 0;

            // Act
            var result = await _controller.GetBoardById(boardId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("Invalid board ID", response.Message);
        }

        [Fact]
        public async Task GetBoardById_WithNegativeBoardId_ReturnsBadRequest()
        {
            // Arrange
            var boardId = -5;

            // Act
            var result = await _controller.GetBoardById(boardId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
            Assert.Equal(400, response.Status);
        }

        [Fact]
        public async Task GetBoardById_WhenBoardNotFound_ReturnsNotFound()
        {
            // Arrange
            var boardId = 999;

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetBoardByIdQuery>(), default))
                .ReturnsAsync((BoardWithColumnsDto)null);

            // Act
            var result = await _controller.GetBoardById(boardId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<object>>(notFoundResult.Value);
            Assert.Equal(400, response.Status);
            Assert.Contains("does not exist or is inactive", response.Message);
        }

        [Fact]
        public async Task GetBoardById_WithIncludeInactive_ReturnsOk()
        {
            // Arrange
            var boardId = 1;
            var includeInactive = true;
            var expectedBoard = new BoardWithColumnsDto
            {
                Id = boardId,
                Name = "Inactive Board",
                IsActive = false,
                Columns = new List<BoardColumnDto>()
            };

            _mediatorMock.Setup(m => m.Send(It.Is<GetBoardByIdQuery>(q =>
                q.BoardId == boardId && q.IncludeInactive == includeInactive), default))
                .ReturnsAsync(expectedBoard);

            // Act
            var result = await _controller.GetBoardById(boardId, includeInactive);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<BoardWithColumnsDto>>(okResult.Value);
            Assert.Equal(200, response.Status);
            Assert.False(response.Data.IsActive);
        }

        [Fact]
        public async Task GetBoardById_WhenDatabaseError_ReturnsInternalServerError()
        {
            // Arrange
            var boardId = 1;

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetBoardByIdQuery>(), default))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act
            var result = await _controller.GetBoardById(boardId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetBoardById_WithUnexpectedException_ReturnsInternalServerError()
        {
            // Arrange
            var boardId = 1;

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetBoardByIdQuery>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.GetBoardById(boardId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetBoardById_WithMultipleColumns_ReturnsOkWithAllColumns()
        {
            // Arrange
            var boardId = 1;
            var expectedBoard = new BoardWithColumnsDto
            {
                Id = boardId,
                Name = "Multi-Column Board",
                Type = "kanban",
                IsActive = true,
                ProjectId = Guid.NewGuid(),
                ProjectName = "Test Project",
                Columns = new List<BoardColumnDto>
                {
                    new BoardColumnDto { Id = Guid.NewGuid(), BoardColumnName = "To Do", Position = 1 },
                    new BoardColumnDto { Id = Guid.NewGuid(), BoardColumnName = "In Progress", Position = 2 },
                    new BoardColumnDto { Id = Guid.NewGuid(), BoardColumnName = "Review", Position = 3 },
                    new BoardColumnDto { Id = Guid.NewGuid(), BoardColumnName = "Done", Position = 4 }
                }
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetBoardByIdQuery>(), default))
                .ReturnsAsync(expectedBoard);

            // Act
            var result = await _controller.GetBoardById(boardId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<BoardWithColumnsDto>>(okResult.Value);
            Assert.Equal(200, response.Status);
            Assert.Equal(4, response.Data.Columns.Count);
            Assert.Contains("4 column(s)", response.Message);
        }

        #endregion
    }
}
