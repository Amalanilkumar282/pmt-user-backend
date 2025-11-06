using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Handler.Statuses;
using BACKEND_CQRS.Application.Query.Statuses;
using BACKEND_CQRS.Application.Wrapper;
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
    /// Unit tests for Status Query Handlers
    /// </summary>
    public class StatusQueryHandlerTests
    {
        private readonly Mock<IStatusRepository> _statusRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<GetAllStatusesQueryHandler>> _getAllLoggerMock;
        private readonly Mock<ILogger<GetStatusByIdQueryHandler>> _getByIdLoggerMock;

        public StatusQueryHandlerTests()
        {
            _statusRepositoryMock = new Mock<IStatusRepository>();
            _mapperMock = new Mock<IMapper>();
            _getAllLoggerMock = new Mock<ILogger<GetAllStatusesQueryHandler>>();
            _getByIdLoggerMock = new Mock<ILogger<GetStatusByIdQueryHandler>>();
        }

        #region GetAllStatusesQueryHandler Tests

        [Fact]
        public async Task GetAllStatusesQueryHandler_WithStatuses_ReturnsSuccessWithData()
        {
            // Arrange
            var statuses = new List<Status>
            {
                new Status { Id = 1, StatusName = "To Do" },
                new Status { Id = 2, StatusName = "In Progress" },
                new Status { Id = 3, StatusName = "Done" }
            };

            var statusDtos = new List<StatusDto>
            {
                new StatusDto { Id = 1, StatusName = "To Do" },
                new StatusDto { Id = 2, StatusName = "In Progress" },
                new StatusDto { Id = 3, StatusName = "Done" }
            };

            _statusRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(statuses);

            _mapperMock
                .Setup(m => m.Map<List<StatusDto>>(statuses))
                .Returns(statusDtos);

            var handler = new GetAllStatusesQueryHandler(
                _statusRepositoryMock.Object, 
                _mapperMock.Object, 
                _getAllLoggerMock.Object);
            
            var query = new GetAllStatusesQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Contains("Successfully fetched 3 status(es)", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal("To Do", result.Data[0].StatusName);

            _statusRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<List<StatusDto>>(statuses), Times.Once);
        }

        [Fact]
        public async Task GetAllStatusesQueryHandler_WithNoStatuses_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var statuses = new List<Status>();
            var statusDtos = new List<StatusDto>();

            _statusRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(statuses);

            _mapperMock
                .Setup(m => m.Map<List<StatusDto>>(statuses))
                .Returns(statusDtos);

            var handler = new GetAllStatusesQueryHandler(
                _statusRepositoryMock.Object, 
                _mapperMock.Object, 
                _getAllLoggerMock.Object);
            
            var query = new GetAllStatusesQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal("No statuses found", result.Message);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);

            _statusRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllStatusesQueryHandler_RepositoryThrowsException_ReturnsFail()
        {
            // Arrange
            _statusRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ThrowsAsync(new Exception("Database error"));

            var handler = new GetAllStatusesQueryHandler(
                _statusRepositoryMock.Object, 
                _mapperMock.Object, 
                _getAllLoggerMock.Object);
            
            var query = new GetAllStatusesQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("An error occurred while fetching statuses", result.Message);

            _statusRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        #endregion

        #region GetStatusByIdQueryHandler Tests

        [Fact]
        public async Task GetStatusByIdQueryHandler_WithValidId_ReturnsSuccessWithData()
        {
            // Arrange
            var statusId = 1;
            var status = new Status { Id = statusId, StatusName = "In Review" };
            var statusDto = new StatusDto { Id = statusId, StatusName = "In Review" };

            _statusRepositoryMock
                .Setup(repo => repo.GetByIdAsync(statusId))
                .ReturnsAsync(status);

            _mapperMock
                .Setup(m => m.Map<StatusDto>(status))
                .Returns(statusDto);

            var handler = new GetStatusByIdQueryHandler(
                _statusRepositoryMock.Object, 
                _mapperMock.Object, 
                _getByIdLoggerMock.Object);
            
            var query = new GetStatusByIdQuery(statusId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Contains($"Successfully fetched status with ID {statusId}", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(statusId, result.Data.Id);
            Assert.Equal("In Review", result.Data.StatusName);

            _statusRepositoryMock.Verify(repo => repo.GetByIdAsync(statusId), Times.Once);
            _mapperMock.Verify(m => m.Map<StatusDto>(status), Times.Once);
        }

        [Fact]
        public async Task GetStatusByIdQueryHandler_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var statusId = 999;

            _statusRepositoryMock
                .Setup(repo => repo.GetByIdAsync(statusId))
                .ReturnsAsync((Status)null!);

            var handler = new GetStatusByIdQueryHandler(
                _statusRepositoryMock.Object, 
                _mapperMock.Object, 
                _getByIdLoggerMock.Object);
            
            var query = new GetStatusByIdQuery(statusId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.Status);
            Assert.Contains($"Status with ID {statusId} not found", result.Message);

            _statusRepositoryMock.Verify(repo => repo.GetByIdAsync(statusId), Times.Once);
            _mapperMock.Verify(m => m.Map<StatusDto>(It.IsAny<Status>()), Times.Never);
        }

        [Fact]
        public async Task GetStatusByIdQueryHandler_WithZeroId_ReturnsNotFound()
        {
            // Arrange
            var statusId = 0;

            _statusRepositoryMock
                .Setup(repo => repo.GetByIdAsync(statusId))
                .ReturnsAsync((Status)null!);

            var handler = new GetStatusByIdQueryHandler(
                _statusRepositoryMock.Object, 
                _mapperMock.Object, 
                _getByIdLoggerMock.Object);
            
            var query = new GetStatusByIdQuery(statusId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.Status);

            _statusRepositoryMock.Verify(repo => repo.GetByIdAsync(statusId), Times.Once);
        }

        [Fact]
        public async Task GetStatusByIdQueryHandler_WithNegativeId_ReturnsNotFound()
        {
            // Arrange
            var statusId = -1;

            _statusRepositoryMock
                .Setup(repo => repo.GetByIdAsync(statusId))
                .ReturnsAsync((Status)null!);

            var handler = new GetStatusByIdQueryHandler(
                _statusRepositoryMock.Object, 
                _mapperMock.Object, 
                _getByIdLoggerMock.Object);
            
            var query = new GetStatusByIdQuery(statusId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.Status);

            _statusRepositoryMock.Verify(repo => repo.GetByIdAsync(statusId), Times.Once);
        }

        [Fact]
        public async Task GetStatusByIdQueryHandler_RepositoryThrowsException_ReturnsFail()
        {
            // Arrange
            var statusId = 1;

            _statusRepositoryMock
                .Setup(repo => repo.GetByIdAsync(statusId))
                .ThrowsAsync(new Exception("Database connection error"));

            var handler = new GetStatusByIdQueryHandler(
                _statusRepositoryMock.Object, 
                _mapperMock.Object, 
                _getByIdLoggerMock.Object);
            
            var query = new GetStatusByIdQuery(statusId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("An error occurred while fetching the status", result.Message);

            _statusRepositoryMock.Verify(repo => repo.GetByIdAsync(statusId), Times.Once);
        }

        #endregion
    }
}
