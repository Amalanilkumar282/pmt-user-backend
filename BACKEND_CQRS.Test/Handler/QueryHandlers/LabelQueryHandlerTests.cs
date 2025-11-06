using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Handler.Labels;
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
    /// Unit tests for Label Query Handlers
    /// </summary>
    public class LabelQueryHandlerTests
    {
        private readonly Mock<ILabelRepository> _labelRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        public LabelQueryHandlerTests()
        {
            _labelRepositoryMock = new Mock<ILabelRepository>();
            _mapperMock = new Mock<IMapper>();
        }

        #region GetAllLabelsQueryHandler Tests

        [Fact]
        public async Task GetAllLabelsQueryHandler_WithLabels_ReturnsSuccessWithData()
        {
            // Arrange
            var labels = new List<Label>
            {
                new Label { Id = 1, Name = "Bug", Colour = "#FF0000" },
                new Label { Id = 2, Name = "Feature", Colour = "#00FF00" },
                new Label { Id = 3, Name = "Enhancement", Colour = "#0000FF" }
            };

            var labelDtos = new List<LabelDto>
            {
                new LabelDto { Id = 1, Name = "Bug", Colour = "#FF0000" },
                new LabelDto { Id = 2, Name = "Feature", Colour = "#00FF00" },
                new LabelDto { Id = 3, Name = "Enhancement", Colour = "#0000FF" }
            };

            _labelRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(labels);

            _mapperMock
                .Setup(m => m.Map<List<LabelDto>>(labels))
                .Returns(labelDtos);

            var handler = new GetAllLabelsQueryHandler(_labelRepositoryMock.Object, _mapperMock.Object);
            var query = new GetAllLabelsQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal("Labels fetched successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal("Bug", result.Data[0].Name);
            Assert.Equal("#FF0000", result.Data[0].Colour);

            _labelRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<List<LabelDto>>(labels), Times.Once);
        }

        [Fact]
        public async Task GetAllLabelsQueryHandler_WithNoLabels_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var labels = new List<Label>();
            var labelDtos = new List<LabelDto>();

            _labelRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(labels);

            _mapperMock
                .Setup(m => m.Map<List<LabelDto>>(labels))
                .Returns(labelDtos);

            var handler = new GetAllLabelsQueryHandler(_labelRepositoryMock.Object, _mapperMock.Object);
            var query = new GetAllLabelsQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);

            _labelRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllLabelsQueryHandler_WithSingleLabel_ReturnsSuccessWithOneItem()
        {
            // Arrange
            var labels = new List<Label>
            {
                new Label { Id = 1, Name = "Priority High", Colour = "#FF5733" }
            };

            var labelDtos = new List<LabelDto>
            {
                new LabelDto { Id = 1, Name = "Priority High", Colour = "#FF5733" }
            };

            _labelRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(labels);

            _mapperMock
                .Setup(m => m.Map<List<LabelDto>>(labels))
                .Returns(labelDtos);

            var handler = new GetAllLabelsQueryHandler(_labelRepositoryMock.Object, _mapperMock.Object);
            var query = new GetAllLabelsQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Single(result.Data);
            Assert.Equal("Priority High", result.Data[0].Name);

            _labelRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllLabelsQueryHandler_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            _labelRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ThrowsAsync(new Exception("Database connection error"));

            var handler = new GetAllLabelsQueryHandler(_labelRepositoryMock.Object, _mapperMock.Object);
            var query = new GetAllLabelsQuery();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await handler.Handle(query, CancellationToken.None));

            _labelRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        #endregion
    }
}
