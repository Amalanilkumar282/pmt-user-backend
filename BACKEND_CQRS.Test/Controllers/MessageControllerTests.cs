using BACKEND_CQRS.Api.Controllers;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BACKEND_CQRS.Test.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for MessageController
    /// Tests message creation endpoint with success, validation, error, and edge case scenarios
    /// </summary>
    public class MessageControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<MessageController>> _loggerMock;
        private readonly MessageController _controller;

        public MessageControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<MessageController>>();
            _controller = new MessageController(_mediatorMock.Object, _loggerMock.Object);
        }

        #region CreateMessage Tests

        [Fact]
        public async Task CreateMessage_WithValidCommand_ReturnsOk()
        {
            // Arrange
            var command = new CreateMessageCommand
            {
                ChannelId = Guid.NewGuid(),
                CreatedBy = 1,
                Body = "Test message"
            };

            var createdMessage = new MessageDto
            {
                Id = Guid.NewGuid(),
                ChannelId = command.ChannelId,
                CreatedBy = command.CreatedBy,
                Body = "Test message",
                CreatedAt = DateTimeOffset.UtcNow
            };

            var response = ApiResponse<MessageDto>.Success(createdMessage, "Message created successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateMessageCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateMessage(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<MessageDto>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
            Assert.Equal("Test message", apiResponse.Data.Body);
        }

        [Fact]
        public async Task CreateMessage_WithEmptyContent_SendsToMediator()
        {
            // Arrange
            var command = new CreateMessageCommand
            {
                ChannelId = Guid.NewGuid(),
                CreatedBy = 1,
                Body = ""
            };

            var response = ApiResponse<MessageDto>.Fail("Message body is required");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateMessageCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateMessage(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<MessageDto>>(badRequestResult.Value);
            Assert.Equal(400, apiResponse.Status);
        }

        [Fact]
        public async Task CreateMessage_WithInvalidChannelId_ReturnsFailure()
        {
            // Arrange
            var command = new CreateMessageCommand
            {
                ChannelId = Guid.Empty,
                CreatedBy = 1,
                Body = "Test message"
            };

            var response = ApiResponse<MessageDto>.Fail("Channel not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateMessageCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateMessage(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<MessageDto>>(badRequestResult.Value);
            Assert.Contains("not found", apiResponse.Message);
        }

        [Fact]
        public async Task CreateMessage_WithInvalidSenderId_ReturnsFailure()
        {
            // Arrange
            var command = new CreateMessageCommand
            {
                ChannelId = Guid.NewGuid(),
                CreatedBy = 999,
                Body = "Test message"
            };

            var response = ApiResponse<MessageDto>.Fail("Sender not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateMessageCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateMessage(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task CreateMessage_WithLongContent_ReturnsSuccess()
        {
            // Arrange
            var longContent = new string('a', 5000);
            var command = new CreateMessageCommand
            {
                ChannelId = Guid.NewGuid(),
                CreatedBy = 1,
                Body = longContent
            };

            var createdMessage = new MessageDto
            {
                Id = Guid.NewGuid(),
                Body = longContent
            };

            var response = ApiResponse<MessageDto>.Success(createdMessage, "Message created");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateMessageCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateMessage(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<MessageDto>>(okResult.Value);
            Assert.Equal(longContent.Length, apiResponse.Data.Body!.Length);
        }

        [Fact]
        public async Task CreateMessage_WithSpecialCharacters_ReturnsSuccess()
        {
            // Arrange
            var specialContent = "Test @user, #channel, emojis ????, quotes \"test\", apostrophes 'test'";
            var command = new CreateMessageCommand
            {
                ChannelId = Guid.NewGuid(),
                CreatedBy = 1,
                Body = specialContent
            };

            var createdMessage = new MessageDto
            {
                Id = Guid.NewGuid(),
                Body = specialContent
            };

            var response = ApiResponse<MessageDto>.Success(createdMessage, "Message created");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateMessageCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateMessage(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<MessageDto>>(okResult.Value);
            Assert.Equal(specialContent, apiResponse.Data.Body);
        }

        [Fact]
        public async Task CreateMessage_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var command = new CreateMessageCommand
            {
                ChannelId = Guid.NewGuid(),
                CreatedBy = 1,
                Body = "Test message"
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateMessageCommand>(), default))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateMessage(command);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateMessage_LogsErrorOnException()
        {
            // Arrange
            var command = new CreateMessageCommand
            {
                ChannelId = Guid.NewGuid(),
                CreatedBy = 1,
                Body = "Test"
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateMessageCommand>(), default))
                .ThrowsAsync(new Exception("Error"));

            // Act
            await _controller.CreateMessage(command);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateMessage_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var command = new CreateMessageCommand
            {
                ChannelId = Guid.NewGuid(),
                CreatedBy = 1,
                Body = "Test"
            };

            _controller.ModelState.AddModelError("Body", "Body is required");

            // Act
            var result = await _controller.CreateMessage(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<MessageDto>>(badRequestResult.Value);
            Assert.Contains("Validation failed", apiResponse.Message);
        }

        [Fact]
        public async Task CreateMessage_WithInvalidModelState_LogsWarning()
        {
            // Arrange
            var command = new CreateMessageCommand();
            _controller.ModelState.AddModelError("Body", "Required");

            // Act
            await _controller.CreateMessage(command);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid model state")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateMessage_CallsMediatorOnce()
        {
            // Arrange
            var command = new CreateMessageCommand
            {
                ChannelId = Guid.NewGuid(),
                CreatedBy = 1,
                Body = "Test"
            };

            var response = ApiResponse<MessageDto>.Success(new MessageDto(), "Success");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateMessageCommand>(), default))
                .ReturnsAsync(response);

            // Act
            await _controller.CreateMessage(command);

            // Assert
            _mediatorMock.Verify(m => m.Send(It.IsAny<CreateMessageCommand>(), default), Times.Once);
        }

        [Fact]
        public async Task CreateMessage_WithNullContent_SendsToMediator()
        {
            // Arrange
            var command = new CreateMessageCommand
            {
                ChannelId = Guid.NewGuid(),
                CreatedBy = 1,
                Body = null!
            };

            var response = ApiResponse<MessageDto>.Fail("Body is required");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateMessageCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateMessage(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task CreateMessage_VerifiesResponseStructure()
        {
            // Arrange
            var command = new CreateMessageCommand
            {
                ChannelId = Guid.NewGuid(),
                CreatedBy = 1,
                Body = "Test message"
            };

            var messageId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            var createdMessage = new MessageDto
            {
                Id = messageId,
                ChannelId = command.ChannelId,
                CreatedBy = command.CreatedBy,
                Body = "Test message",
                CreatedAt = now
            };

            var response = ApiResponse<MessageDto>.Success(createdMessage, "Message created");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateMessageCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateMessage(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<MessageDto>>(okResult.Value);
            Assert.Equal(messageId, apiResponse.Data.Id);
            Assert.Equal(command.ChannelId, apiResponse.Data.ChannelId);
            Assert.Equal(command.CreatedBy, apiResponse.Data.CreatedBy);
            Assert.Equal("Test message", apiResponse.Data.Body);
        }

        #endregion
    }
}
