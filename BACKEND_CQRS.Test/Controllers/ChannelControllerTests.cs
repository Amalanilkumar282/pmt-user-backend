using BACKEND_CQRS.Api.Controllers;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Query.Messages;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BACKEND_CQRS.Test.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for ChannelController
    /// Tests all channel management endpoints with success, validation, error, and edge case scenarios
    /// </summary>
    public class ChannelControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<ChannelController>> _loggerMock;
        private readonly ChannelController _controller;

        public ChannelControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<ChannelController>>();
            _controller = new ChannelController(_mediatorMock.Object, _loggerMock.Object);
        }

        #region GetChannelsByTeamId Tests

        [Fact]
        public async Task GetChannelsByTeamId_WithValidTeamId_ReturnsChannels()
        {
            // Arrange
            var teamId = 1;
            var channels = new List<ChannelDto>
            {
                new ChannelDto { Id = Guid.NewGuid(), Name = "General" },
                new ChannelDto { Id = Guid.NewGuid(), Name = "Development" }
            };

            var response = ApiResponse<List<ChannelDto>>.Success(channels, "Channels retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetChannelsByTeamIdQuery>(q => q.TeamId == teamId),
                default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetChannelsByTeamId(teamId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetChannelsByTeamId_WithNoChannels_ReturnsEmptyList()
        {
            // Arrange
            var teamId = 999;
            var response = ApiResponse<List<ChannelDto>>.Success(new List<ChannelDto>(), "No channels found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetChannelsByTeamIdQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetChannelsByTeamId(teamId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetChannelsByTeamId_WithZeroTeamId_CallsMediator()
        {
            // Arrange
            var teamId = 0;
            var response = ApiResponse<List<ChannelDto>>.Success(new List<ChannelDto>(), "Retrieved");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetChannelsByTeamIdQuery>(), default))
                .ReturnsAsync(response);

            // Act
            await _controller.GetChannelsByTeamId(teamId);

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<GetChannelsByTeamIdQuery>(q => q.TeamId == 0),
                default),
                Times.Once);
        }

        #endregion

        #region GetMessagesByChannelId Tests

        [Fact]
        public async Task GetMessagesByChannelId_WithDefaultTake_ReturnsMessages()
        {
            // Arrange
            var channelId = Guid.NewGuid();
            var messages = new List<MessageDto>
            {
                new MessageDto { Id = Guid.NewGuid(), Body = "Message 1", ChannelId = channelId },
                new MessageDto { Id = Guid.NewGuid(), Body = "Message 2", ChannelId = channelId }
            };

            var response = ApiResponse<List<MessageDto>>.Success(messages, "Messages retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetMessagesByChannelIdQuery>(q => q.ChannelId == channelId && q.Take == 100),
                default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetMessagesByChannelId(channelId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetMessagesByChannelId_WithCustomTake_ReturnsSpecifiedNumber()
        {
            // Arrange
            var channelId = Guid.NewGuid();
            var take = 50;
            var response = ApiResponse<List<MessageDto>>.Success(new List<MessageDto>(), "Retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetMessagesByChannelIdQuery>(q => q.ChannelId == channelId && q.Take == take),
                default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetMessagesByChannelId(channelId, take);

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<GetMessagesByChannelIdQuery>(q => q.Take == 50),
                default),
                Times.Once);
        }

        [Fact]
        public async Task GetMessagesByChannelId_WithNoMessages_ReturnsEmptyList()
        {
            // Arrange
            var channelId = Guid.NewGuid();
            var response = ApiResponse<List<MessageDto>>.Success(new List<MessageDto>(), "No messages");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetMessagesByChannelIdQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetMessagesByChannelId(channelId);

            // Assert
            Assert.Empty(result.Data);
        }

        #endregion

        #region CreateChannel Tests

        [Fact]
        public async Task CreateChannel_WithValidCommand_ReturnsSuccess()
        {
            // Arrange
            var command = new CreateChannelCommand
            {
                ChannelName = "New Channel",
                TeamId = 1
            };

            var createdChannel = new ChannelDto
            {
                Id = Guid.NewGuid(),
                Name = "New Channel"
            };

            var response = ApiResponse<ChannelDto>.Created(createdChannel, "Channel created");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateChannelCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateChannel(command);

            // Assert
            Assert.Equal(201, result.Status);
            Assert.Equal("New Channel", result.Data.Name);
        }

        [Fact]
        public async Task CreateChannel_WithEmptyName_SendsToMediator()
        {
            // Arrange
            var command = new CreateChannelCommand
            {
                ChannelName = "",
                TeamId = 1
            };

            var response = ApiResponse<ChannelDto>.Fail("Channel name is required");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateChannelCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateChannel(command);

            // Assert
            Assert.Equal(400, result.Status);
            _mediatorMock.Verify(m => m.Send(It.IsAny<CreateChannelCommand>(), default), Times.Once);
        }

        [Fact]
        public async Task CreateChannel_WithInvalidTeamId_ReturnsFailure()
        {
            // Arrange
            var command = new CreateChannelCommand
            {
                ChannelName = "Channel",
                TeamId = 999
            };

            var response = ApiResponse<ChannelDto>.Fail("Team not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateChannelCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateChannel(command);

            // Assert
            Assert.Equal(400, result.Status);
            Assert.Contains("not found", result.Message);
        }

        #endregion

        #region DeleteChannel Tests

        [Fact]
        public async Task DeleteChannel_WithValidId_ReturnsOk()
        {
            // Arrange
            var channelId = Guid.NewGuid();
            var deletedBy = 1;
            var response = ApiResponse<bool>.Success(true, "Channel deleted successfully");

            _mediatorMock.Setup(m => m.Send(
                It.Is<DeleteChannelCommand>(c => c.ChannelId == channelId && c.DeletedBy == deletedBy),
                default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteChannel(channelId, deletedBy);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.True(apiResponse.Data);
        }

        [Fact]
        public async Task DeleteChannel_WithoutDeletedBy_ReturnsOk()
        {
            // Arrange
            var channelId = Guid.NewGuid();
            var response = ApiResponse<bool>.Success(true, "Channel deleted");

            _mediatorMock.Setup(m => m.Send(
                It.Is<DeleteChannelCommand>(c => c.ChannelId == channelId && c.DeletedBy == null),
                default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteChannel(channelId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
        }

        [Fact]
        public async Task DeleteChannel_WhenChannelNotFound_ReturnsBadRequest()
        {
            // Arrange
            var channelId = Guid.NewGuid();
            var response = ApiResponse<bool>.Fail("Channel not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteChannelCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteChannel(channelId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<bool>>(badRequestResult.Value);
            Assert.Equal(400, apiResponse.Status);
        }

        [Fact]
        public async Task DeleteChannel_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var channelId = Guid.NewGuid();

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteChannelCommand>(), default))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.DeleteChannel(channelId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteChannel_LogsInformation()
        {
            // Arrange
            var channelId = Guid.NewGuid();
            var deletedBy = 1;
            var response = ApiResponse<bool>.Success(true, "Deleted");

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteChannelCommand>(), default))
                .ReturnsAsync(response);

            // Act
            await _controller.DeleteChannel(channelId, deletedBy);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("API request to delete channel")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteChannel_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var channelId = Guid.NewGuid();
            _controller.ModelState.AddModelError("ChannelId", "Invalid channel ID");

            // Act
            var result = await _controller.DeleteChannel(channelId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequestResult);
        }

        #endregion
    }
}
