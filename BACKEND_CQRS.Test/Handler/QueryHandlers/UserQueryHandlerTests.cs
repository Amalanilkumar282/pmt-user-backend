using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Handler.User;
using BACKEND_CQRS.Application.Query.User;
using BACKEND_CQRS.Application.Query.Users;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
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
    /// Unit tests for User Query Handlers
    /// </summary>
    public class UserQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        public UserQueryHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
        }

        #region GetAllUsersQueryHandler Tests

        [Fact]
        public async Task GetAllUsersQueryHandler_WithUsers_ReturnsSuccessWithData()
        {
            // Arrange
            var users = new List<Users>
            {
                new Users { Id = 1, Name = "John Doe", Email = "john@example.com", IsActive = true },
                new Users { Id = 2, Name = "Jane Smith", Email = "jane@example.com", IsActive = true },
                new Users { Id = 3, Name = "Bob Johnson", Email = "bob@example.com", IsActive = false }
            };

            var userDtos = new List<UserDto>
            {
                new UserDto { Id = 1, Name = "John Doe", Email = "john@example.com", IsActive = true },
                new UserDto { Id = 2, Name = "Jane Smith", Email = "jane@example.com", IsActive = true },
                new UserDto { Id = 3, Name = "Bob Johnson", Email = "bob@example.com", IsActive = false }
            };

            _userRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(users);

            _mapperMock
                .Setup(m => m.Map<List<UserDto>>(users))
                .Returns(userDtos);

            var handler = new GetAllUsersQueryHandler(_userRepositoryMock.Object, _mapperMock.Object);
            var query = new GetAllUsersQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal("John Doe", result.Data[0].Name);
            Assert.Equal("john@example.com", result.Data[0].Email);

            _userRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<List<UserDto>>(users), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersQueryHandler_WithNoUsers_ReturnsFail()
        {
            // Arrange
            var users = new List<Users>();

            _userRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(users);

            var handler = new GetAllUsersQueryHandler(_userRepositoryMock.Object, _mapperMock.Object);
            var query = new GetAllUsersQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Equal("No users found.", result.Message);

            _userRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersQueryHandler_WithNullUsers_ReturnsFail()
        {
            // Arrange
            _userRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync((List<Users>)null!);

            var handler = new GetAllUsersQueryHandler(_userRepositoryMock.Object, _mapperMock.Object);
            var query = new GetAllUsersQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Equal("No users found.", result.Message);

            _userRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersQueryHandler_WithActiveUsersOnly_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<Users>
            {
                new Users { Id = 1, Name = "Active User 1", Email = "active1@example.com", IsActive = true },
                new Users { Id = 2, Name = "Active User 2", Email = "active2@example.com", IsActive = true }
            };

            var userDtos = new List<UserDto>
            {
                new UserDto { Id = 1, Name = "Active User 1", Email = "active1@example.com", IsActive = true },
                new UserDto { Id = 2, Name = "Active User 2", Email = "active2@example.com", IsActive = true }
            };

            _userRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(users);

            _mapperMock
                .Setup(m => m.Map<List<UserDto>>(users))
                .Returns(userDtos);

            var handler = new GetAllUsersQueryHandler(_userRepositoryMock.Object, _mapperMock.Object);
            var query = new GetAllUsersQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, user => Assert.True(user.IsActive));

            _userRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersQueryHandler_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            _userRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ThrowsAsync(new Exception("Database error"));

            var handler = new GetAllUsersQueryHandler(_userRepositoryMock.Object, _mapperMock.Object);
            var query = new GetAllUsersQuery();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await handler.Handle(query, CancellationToken.None));

            _userRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        #endregion

        #region GetUserByIdQueryHandler Tests

        [Fact]
        public async Task GetUserByIdQueryHandler_WithValidId_ReturnsSuccessWithData()
        {
            // Arrange
            var userId = 1;
            var user = new Users 
            { 
                Id = userId, 
                Name = "John Doe", 
                Email = "john@example.com", 
                IsActive = true,
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            var userDto = new UserDto 
            { 
                Id = userId, 
                Name = "John Doe", 
                Email = "john@example.com", 
                IsActive = true,
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _mapperMock
                .Setup(m => m.Map<UserDto>(user))
                .Returns(userDto);

            var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object, _mapperMock.Object);
            var query = new GetUserByIdQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.NotNull(result.Data);
            Assert.Equal(userId, result.Data.Id);
            Assert.Equal("John Doe", result.Data.Name);
            Assert.Equal("john@example.com", result.Data.Email);

            _userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
            _mapperMock.Verify(m => m.Map<UserDto>(user), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdQueryHandler_WithInvalidId_ReturnsFail()
        {
            // Arrange
            var userId = 999;

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((Users)null!);

            var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object, _mapperMock.Object);
            var query = new GetUserByIdQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("not found", result.Message);

            _userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
            _mapperMock.Verify(m => m.Map<UserDto>(It.IsAny<Users>()), Times.Never);
        }

        [Fact]
        public async Task GetUserByIdQueryHandler_WithZeroId_ReturnsFail()
        {
            // Arrange
            var userId = 0;

            var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object, _mapperMock.Object);
            var query = new GetUserByIdQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("Invalid user ID", result.Message);

            _userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId), Times.Never);
        }

        [Fact]
        public async Task GetUserByIdQueryHandler_WithNegativeId_ReturnsFail()
        {
            // Arrange
            var userId = -1;

            var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object, _mapperMock.Object);
            var query = new GetUserByIdQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("Invalid user ID", result.Message);

            _userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId), Times.Never);
        }

        [Fact]
        public async Task GetUserByIdQueryHandler_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var userId = 1;

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ThrowsAsync(new Exception("Database connection error"));

            var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object, _mapperMock.Object);
            var query = new GetUserByIdQuery(userId);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await handler.Handle(query, CancellationToken.None));

            _userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        }

        #endregion
    }
}
