using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Handler.Boards;
using BACKEND_CQRS.Application.Query.Boards;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BACKEND_CQRS.Test.Handler.QueryHandlers
{
    /// <summary>
    /// Unit tests for Board Query Handlers
    /// Tests board retrieval operations including boards by project and board by ID with columns
    /// </summary>
    public class BoardQueryHandlerTests
    {
        private readonly Mock<IBoardRepository> _boardRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<GetBoardByIdQueryHandler>> _loggerMock;
        private readonly Mock<ILogger<GetBoardColumnsByBoardIdQueryHandler>> _columnLoggerMock; // ADDED

        public BoardQueryHandlerTests()
        {
            _boardRepositoryMock = new Mock<IBoardRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<GetBoardByIdQueryHandler>>();
            _columnLoggerMock = new Mock<ILogger<GetBoardColumnsByBoardIdQueryHandler>>(); // ADDED
        }

        #region GetBoardByIdQueryHandler Tests

        [Fact]
        public async Task GetBoardByIdQueryHandler_WithValidId_ReturnsBoard()
        {
            // Arrange
            var boardId = 1; // FIXED: Changed to int
            var board = new Board
            {
                Id = boardId, // FIXED: Now int
                Name = "Main Board",
                ProjectId = Guid.NewGuid(),
                IsActive = true,
                BoardColumns = new List<BoardColumn>
                {
                    new BoardColumn { Id = Guid.NewGuid(), BoardColumnName = "To Do", Position = 1 }, // FIXED: BoardColumnName
                    new BoardColumn { Id = Guid.NewGuid(), BoardColumnName = "In Progress", Position = 2 }, // FIXED: BoardColumnName
                    new BoardColumn { Id = Guid.NewGuid(), BoardColumnName = "Done", Position = 3 } // FIXED: BoardColumnName
                }
            };

            var boardDto = new BoardWithColumnsDto
            {
                Id = boardId, // FIXED: Now int
                Name = board.Name,
                ProjectId = board.ProjectId,
                IsActive = board.IsActive,
                Columns = new List<BoardColumnDto>
                {
                    new BoardColumnDto { Id = board.BoardColumns.ElementAt(0).Id, BoardColumnName = "To Do", Position = 1 }, // FIXED: BoardColumnName
                    new BoardColumnDto { Id = board.BoardColumns.ElementAt(1).Id, BoardColumnName = "In Progress", Position = 2 }, // FIXED: BoardColumnName
                    new BoardColumnDto { Id = board.BoardColumns.ElementAt(2).Id, BoardColumnName = "Done", Position = 3 } // FIXED: BoardColumnName
                }
            };

            _boardRepositoryMock
                .Setup(repo => repo.GetBoardByIdWithColumnsAsync(boardId, false)) // FIXED: int boardId
                .ReturnsAsync(board);

            _mapperMock.Setup(m => m.Map<BoardWithColumnsDto>(board)).Returns(boardDto);

            var query = new GetBoardByIdQuery(boardId, false); // FIXED: Use constructor with int
            var handler = new GetBoardByIdQueryHandler(_boardRepositoryMock.Object, _mapperMock.Object, _loggerMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(boardId, result.Id); // FIXED: Compare ints
            Assert.Equal("Main Board", result.Name);
            Assert.Equal(3, result.Columns.Count);
            Assert.Equal("To Do", result.Columns[0].BoardColumnName); // FIXED: BoardColumnName

            _boardRepositoryMock.Verify(repo => repo.GetBoardByIdWithColumnsAsync(boardId, false), Times.Once); // FIXED: int boardId
            _mapperMock.Verify(m => m.Map<BoardWithColumnsDto>(board), Times.Once);
        }

        [Fact]
        public async Task GetBoardByIdQueryHandler_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            var boardId = 999; // FIXED: Changed to int
            _boardRepositoryMock
                .Setup(repo => repo.GetBoardByIdWithColumnsAsync(boardId, false)) // FIXED: int boardId
                .ReturnsAsync((Board?)null);

            var query = new GetBoardByIdQuery(boardId, false); // FIXED: Use constructor with int
            var handler = new GetBoardByIdQueryHandler(_boardRepositoryMock.Object, _mapperMock.Object, _loggerMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
            _boardRepositoryMock.Verify(repo => repo.GetBoardByIdWithColumnsAsync(boardId, false), Times.Once); // FIXED: int boardId
        }

        [Fact]
        public async Task GetBoardByIdQueryHandler_WithIncludeInactive_ReturnsInactiveBoard()
        {
            // Arrange
            var boardId = 1; // FIXED: Changed to int
            var board = new Board
            {
                Id = boardId, // FIXED: Now int
                Name = "Inactive Board",
                ProjectId = Guid.NewGuid(),
                IsActive = false,
                BoardColumns = new List<BoardColumn>()
            };

            var boardDto = new BoardWithColumnsDto
            {
                Id = boardId, // FIXED: Now int
                Name = board.Name,
                ProjectId = board.ProjectId,
                IsActive = false,
                Columns = new List<BoardColumnDto>()
            };

            _boardRepositoryMock
                .Setup(repo => repo.GetBoardByIdWithColumnsAsync(boardId, true)) // FIXED: int boardId
                .ReturnsAsync(board);

            _mapperMock.Setup(m => m.Map<BoardWithColumnsDto>(board)).Returns(boardDto);

            var query = new GetBoardByIdQuery(boardId, true); // FIXED: Use constructor with int
            var handler = new GetBoardByIdQueryHandler(_boardRepositoryMock.Object, _mapperMock.Object, _loggerMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(boardId, result.Id); // FIXED: Compare ints
            Assert.False(result.IsActive);
            _boardRepositoryMock.Verify(repo => repo.GetBoardByIdWithColumnsAsync(boardId, true), Times.Once); // FIXED: int boardId
        }

        [Fact]
        public async Task GetBoardByIdQueryHandler_WithoutIncludeInactive_ReturnsNull()
        {
            // Arrange
            var boardId = 2; // FIXED: Changed to int
            _boardRepositoryMock
                .Setup(repo => repo.GetBoardByIdWithColumnsAsync(boardId, false)) // FIXED: int boardId
                .ReturnsAsync((Board?)null);

            var query = new GetBoardByIdQuery(boardId, false); // FIXED: Use constructor with int
            var handler = new GetBoardByIdQueryHandler(_boardRepositoryMock.Object, _mapperMock.Object, _loggerMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetBoardByIdQueryHandler_WithEmptyColumns_ReturnsEmptyList()
        {
            // Arrange
            var boardId = 3; // FIXED: Changed to int
            var board = new Board
            {
                Id = boardId, // FIXED: Now int
                Name = "Board without Columns",
                ProjectId = Guid.NewGuid(),
                IsActive = true,
                BoardColumns = new List<BoardColumn>()
            };

            var boardDto = new BoardWithColumnsDto
            {
                Id = boardId, // FIXED: Now int
                Name = board.Name,
                ProjectId = board.ProjectId,
                IsActive = true,
                Columns = new List<BoardColumnDto>()
            };

            _boardRepositoryMock
                .Setup(repo => repo.GetBoardByIdWithColumnsAsync(boardId, false)) // FIXED: int boardId
                .ReturnsAsync(board);

            _mapperMock.Setup(m => m.Map<BoardWithColumnsDto>(board)).Returns(boardDto);

            var query = new GetBoardByIdQuery(boardId, false); // FIXED: Use constructor with int
            var handler = new GetBoardByIdQueryHandler(_boardRepositoryMock.Object, _mapperMock.Object, _loggerMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Columns);
        }

        [Fact]
        public async Task GetBoardByIdQueryHandler_ThrowsInvalidOperationException()
        {
            // Arrange
            var boardId = 1; // FIXED: Changed to int
            _boardRepositoryMock
                .Setup(repo => repo.GetBoardByIdWithColumnsAsync(boardId, false)) // FIXED: int boardId
                .ThrowsAsync(new InvalidOperationException("Database error"));

            var query = new GetBoardByIdQuery(boardId, false); // FIXED: Use constructor with int
            var handler = new GetBoardByIdQueryHandler(_boardRepositoryMock.Object, _mapperMock.Object, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await handler.Handle(query, CancellationToken.None));
        }

        #endregion

        #region GetBoardColumnsByBoardIdQueryHandler Tests

        [Fact]
        public async Task GetBoardColumnsByBoardIdQueryHandler_WithColumns_ReturnsSuccessWithData()
        {
            // Arrange
            var boardId = 1; // FIXED: Changed to int
            var columns = new List<BoardColumn>
            {
                new BoardColumn { Id = Guid.NewGuid(), BoardColumnName = "Backlog", Position = 1 }, // FIXED: BoardColumnName
                new BoardColumn { Id = Guid.NewGuid(), BoardColumnName = "To Do", Position = 2 }, // FIXED: BoardColumnName
                new BoardColumn { Id = Guid.NewGuid(), BoardColumnName = "Done", Position = 3 } // FIXED: BoardColumnName
            };

            var columnDtos = new List<BoardColumnDto>
            {
                new BoardColumnDto { Id = columns[0].Id, BoardColumnName = "Backlog", Position = 1 }, // FIXED: BoardColumnName
                new BoardColumnDto { Id = columns[1].Id, BoardColumnName = "To Do", Position = 2 }, // FIXED: BoardColumnName
                new BoardColumnDto { Id = columns[2].Id, BoardColumnName = "Done", Position = 3 } // FIXED: BoardColumnName
            };

            _boardRepositoryMock
                .Setup(repo => repo.GetBoardColumnsAsync(boardId)) // FIXED: int boardId, correct method name
                .ReturnsAsync(columns);

            _mapperMock.Setup(m => m.Map<List<BoardColumnDto>>(columns)).Returns(columnDtos);

            var query = new GetBoardColumnsByBoardIdQuery(boardId); // FIXED: Use constructor with int
            var handler = new GetBoardColumnsByBoardIdQueryHandler(_boardRepositoryMock.Object, _mapperMock.Object, _columnLoggerMock.Object); // FIXED: Added logger

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("Backlog", result[0].BoardColumnName); // FIXED: BoardColumnName

            _boardRepositoryMock.Verify(repo => repo.GetBoardColumnsAsync(boardId), Times.Once); // FIXED: int boardId, correct method name
        }

        [Fact]
        public async Task GetBoardColumnsByBoardIdQueryHandler_WithNoColumns_ReturnsEmptyList()
        {
            // Arrange
            var boardId = 2;
            var columns = new List<BoardColumn>();

            _boardRepositoryMock
                .Setup(repo => repo.GetBoardColumnsAsync(boardId))
                .ReturnsAsync(columns);

            _mapperMock.Setup(m => m.Map<List<BoardColumnDto>>(columns)).Returns(new List<BoardColumnDto>());

            var query = new GetBoardColumnsByBoardIdQuery(boardId);
            var handler = new GetBoardColumnsByBoardIdQueryHandler(_boardRepositoryMock.Object, _mapperMock.Object, _columnLoggerMock.Object); // FIXED

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            _boardRepositoryMock.Verify(repo => repo.GetBoardColumnsAsync(boardId), Times.Once);
        }

        [Fact]
        public async Task GetBoardColumnsByBoardIdQueryHandler_ReturnsSortedByPosition()
        {
            // Arrange
            var boardId = 3;
            var columns = new List<BoardColumn>
            {
                new BoardColumn { Id = Guid.NewGuid(), BoardColumnName = "Third", Position = 3 },
                new BoardColumn { Id = Guid.NewGuid(), BoardColumnName = "First", Position = 1 },
                new BoardColumn { Id = Guid.NewGuid(), BoardColumnName = "Second", Position = 2 }
            };

            var columnDtos = columns.OrderBy(c => c.Position).Select(c => new BoardColumnDto
            {
                Id = c.Id,
                BoardColumnName = c.BoardColumnName,
                Position = c.Position
            }).ToList();

            _boardRepositoryMock
                .Setup(repo => repo.GetBoardColumnsAsync(boardId))
                .ReturnsAsync(columns.OrderBy(c => c.Position).ToList());

            _mapperMock.Setup(m => m.Map<List<BoardColumnDto>>(It.IsAny<List<BoardColumn>>())).Returns(columnDtos);

            var query = new GetBoardColumnsByBoardIdQuery(boardId);
            var handler = new GetBoardColumnsByBoardIdQueryHandler(_boardRepositoryMock.Object, _mapperMock.Object, _columnLoggerMock.Object); // FIXED

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal("First", result[0].BoardColumnName);
            Assert.Equal("Second", result[1].BoardColumnName);
            Assert.Equal("Third", result[2].BoardColumnName);
        }

        [Fact]
        public async Task GetBoardColumnsByBoardIdQueryHandler_ThrowsException()
        {
            // Arrange
            var boardId = 4;
            _boardRepositoryMock
                .Setup(repo => repo.GetBoardColumnsAsync(boardId))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            var query = new GetBoardColumnsByBoardIdQuery(boardId);
            var handler = new GetBoardColumnsByBoardIdQueryHandler(_boardRepositoryMock.Object, _mapperMock.Object, _columnLoggerMock.Object); // FIXED

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await handler.Handle(query, CancellationToken.None));
        }

        #endregion
    }
}
