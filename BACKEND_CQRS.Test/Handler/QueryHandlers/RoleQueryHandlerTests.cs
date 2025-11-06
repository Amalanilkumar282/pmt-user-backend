using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Handler;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
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
    /// Unit tests for Role Query Handlers
    /// </summary>
    public class RoleQueryHandlerTests
    {
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        public RoleQueryHandlerTests()
        {
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _mapperMock = new Mock<IMapper>();
        }

        #region GetAllRolesQueryHandler Tests

        [Fact]
        public async Task GetAllRolesQueryHandler_WithRoles_ReturnsSuccessWithData()
        {
            // Arrange
            var roles = new List<Role>
            {
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Developer" },
                new Role { Id = 3, Name = "Tester" },
                new Role { Id = 4, Name = "Manager" }
            };

            var roleDtos = new List<RoleDto>
            {
                new RoleDto { Id = 1, Name = "Admin" },
                new RoleDto { Id = 2, Name = "Developer" },
                new RoleDto { Id = 3, Name = "Tester" },
                new RoleDto { Id = 4, Name = "Manager" }
            };

            _roleRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(roles);

            _mapperMock
                .Setup(m => m.Map<List<RoleDto>>(roles))
                .Returns(roleDtos);

            var handler = new GetAllRolesQueryHandler(_roleRepositoryMock.Object, _mapperMock.Object);
            var query = new GetAllRolesQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal("Roles fetched successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(4, result.Data.Count);
            Assert.Equal("Admin", result.Data[0].Name);
            Assert.Equal("Developer", result.Data[1].Name);

            _roleRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<List<RoleDto>>(roles), Times.Once);
        }

        [Fact]
        public async Task GetAllRolesQueryHandler_WithNoRoles_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var roles = new List<Role>();
            var roleDtos = new List<RoleDto>();

            _roleRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(roles);

            _mapperMock
                .Setup(m => m.Map<List<RoleDto>>(roles))
                .Returns(roleDtos);

            var handler = new GetAllRolesQueryHandler(_roleRepositoryMock.Object, _mapperMock.Object);
            var query = new GetAllRolesQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);

            _roleRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllRolesQueryHandler_WithSingleRole_ReturnsSuccessWithOneItem()
        {
            // Arrange
            var roles = new List<Role>
            {
                new Role { Id = 1, Name = "SuperAdmin" }
            };

            var roleDtos = new List<RoleDto>
            {
                new RoleDto { Id = 1, Name = "SuperAdmin" }
            };

            _roleRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(roles);

            _mapperMock
                .Setup(m => m.Map<List<RoleDto>>(roles))
                .Returns(roleDtos);

            var handler = new GetAllRolesQueryHandler(_roleRepositoryMock.Object, _mapperMock.Object);
            var query = new GetAllRolesQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Single(result.Data);
            Assert.Equal("SuperAdmin", result.Data[0].Name);

            _roleRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllRolesQueryHandler_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            _roleRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ThrowsAsync(new Exception("Database connection error"));

            var handler = new GetAllRolesQueryHandler(_roleRepositoryMock.Object, _mapperMock.Object);
            var query = new GetAllRolesQuery();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await handler.Handle(query, CancellationToken.None));

            _roleRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllRolesQueryHandler_WithMultipleRoles_MapsCorrectly()
        {
            // Arrange
            var roles = new List<Role>
            {
                new Role { Id = 1, Name = "Owner" },
                new Role { Id = 2, Name = "Contributor" },
                new Role { Id = 3, Name = "Viewer" }
            };

            var roleDtos = new List<RoleDto>
            {
                new RoleDto { Id = 1, Name = "Owner" },
                new RoleDto { Id = 2, Name = "Contributor" },
                new RoleDto { Id = 3, Name = "Viewer" }
            };

            _roleRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(roles);

            _mapperMock
                .Setup(m => m.Map<List<RoleDto>>(roles))
                .Returns(roleDtos);

            var handler = new GetAllRolesQueryHandler(_roleRepositoryMock.Object, _mapperMock.Object);
            var query = new GetAllRolesQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal(1, result.Data[0].Id);
            Assert.Equal(2, result.Data[1].Id);
            Assert.Equal(3, result.Data[2].Id);

            _roleRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<List<RoleDto>>(roles), Times.Once);
        }

        #endregion
    }
}
