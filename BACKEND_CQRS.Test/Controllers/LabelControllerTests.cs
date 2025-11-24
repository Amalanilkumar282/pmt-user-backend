using BACKEND_CQRS.Api.Controllers;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Moq;

namespace BACKEND_CQRS.Test.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for LabelController
    /// Tests all API endpoints with success, validation, error, and edge case scenarios
    /// </summary>
    public class LabelControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly LabelController _controller;

        public LabelControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new LabelController(_mediatorMock.Object);
        }

        #region CreateLabel Tests

        [Fact]
        public async Task CreateLabel_WithValidCommand_ReturnsCreatedLabelId()
        {
            // Arrange
            var command = new CreateLabelCommand
            {
                Name = "Bug",
                Colour = "#FF0000"
            };

            var expectedLabelId = 1;
            var response = ApiResponse<int>.Created(expectedLabelId, "Label created successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateLabelCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateLabel(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.Status);
            Assert.Equal(expectedLabelId, result.Data);
            Assert.Contains("created successfully", result.Message);
        }

        [Fact]
        public async Task CreateLabel_WithHexColor_ReturnsSuccess()
        {
            // Arrange
            var command = new CreateLabelCommand
            {
                Name = "Feature",
                Colour = "#00FF00"
            };

            var response = ApiResponse<int>.Created(2, "Label created successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateLabelCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateLabel(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.Status);
        }

        [Fact]
        public async Task CreateLabel_WithLongName_ReturnsSuccess()
        {
            // Arrange
            var command = new CreateLabelCommand
            {
                Name = "Very Long Label Name For Testing Purposes",
                Colour = "#0000FF"
            };

            var response = ApiResponse<int>.Created(3, "Label created successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateLabelCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateLabel(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.Status);
        }

        [Fact]
        public async Task CreateLabel_WhenDuplicateName_ReturnsFailure()
        {
            // Arrange
            var command = new CreateLabelCommand
            {
                Name = "Bug",
                Colour = "#FF0000"
            };

            var response = ApiResponse<int>.Fail("Label with this name already exists");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateLabelCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateLabel(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("already exists", result.Message);
        }

        [Fact]
        public async Task CreateLabel_WithInvalidColor_HandledByMediator()
        {
            // Arrange
            var command = new CreateLabelCommand
            {
                Name = "Label",
                Colour = "INVALID"
            };

            var response = ApiResponse<int>.Fail("Invalid color format");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateLabelCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateLabel(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
        }

        #endregion

        #region GetAllLabels Tests

        [Fact]
        public async Task GetAllLabels_WithLabels_ReturnsSuccess()
        {
            // Arrange
            var labels = new List<LabelDto>
            {
                new LabelDto { Id = 1, Name = "Bug", Colour = "#FF0000" },
                new LabelDto { Id = 2, Name = "Feature", Colour = "#00FF00" },
                new LabelDto { Id = 3, Name = "Enhancement", Colour = "#0000FF" }
            };

            var response = ApiResponse<List<LabelDto>>.Success(labels, "Labels fetched successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllLabelsQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllLabels();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(3, result.Data.Count);
        }

        [Fact]
        public async Task GetAllLabels_WithNoLabels_ReturnsEmptyList()
        {
            // Arrange
            var emptyList = new List<LabelDto>();
            var response = ApiResponse<List<LabelDto>>.Success(emptyList, "No labels found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllLabelsQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllLabels();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllLabels_WhenExceptionThrown_ReturnsFailure()
        {
            // Arrange
            var response = ApiResponse<List<LabelDto>>.Fail("Database error occurred");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllLabelsQuery>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllLabels();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("error", result.Message);
        }

        [Fact]
        public async Task GetAllLabels_VerifiesCorrectQuery_IsSent()
        {
            // Arrange
            var labels = new List<LabelDto>();
            var response = ApiResponse<List<LabelDto>>.Success(labels, "Success");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllLabelsQuery>(), default))
                .ReturnsAsync(response);

            // Act
            await _controller.GetAllLabels();

            // Assert
            _mediatorMock.Verify(m => m.Send(It.IsAny<GetAllLabelsQuery>(), default), Times.Once);
        }

        #endregion

        #region EditLabel Tests

        [Fact]
        public async Task EditLabel_WithValidCommand_ReturnsSuccess()
        {
            // Arrange
            var command = new EditLabelCommand
            {
                Id = 1,
                Name = "Updated Bug",
                Colour = "#FF5733"
            };

            var response = ApiResponse<int>.Success(1, "Label updated successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<EditLabelCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.EditLabel(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(1, result.Data);
            Assert.Contains("updated successfully", result.Message);
        }

        [Fact]
        public async Task EditLabel_WhenLabelNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new EditLabelCommand
            {
                Id = 999,
                Name = "Non-existent Label",
                Colour = "#000000"
            };

            var response = ApiResponse<int>.Fail("Label not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<EditLabelCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.EditLabel(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("not found", result.Message);
        }

        [Fact]
        public async Task EditLabel_WithOnlyNameChange_ReturnsSuccess()
        {
            // Arrange
            var command = new EditLabelCommand
            {
                Id = 1,
                Name = "New Name",
                Colour = "#FF0000" // Same color
            };

            var response = ApiResponse<int>.Success(1, "Label updated successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<EditLabelCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.EditLabel(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
        }

        [Fact]
        public async Task EditLabel_WithOnlyColorChange_ReturnsSuccess()
        {
            // Arrange
            var command = new EditLabelCommand
            {
                Id = 1,
                Name = "Bug", // Same name
                Colour = "#00FF00" // New color
            };

            var response = ApiResponse<int>.Success(1, "Label updated successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<EditLabelCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.EditLabel(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
        }

        [Fact]
        public async Task EditLabel_WithZeroId_HandledByMediator()
        {
            // Arrange
            var command = new EditLabelCommand
            {
                Id = 0,
                Name = "Label",
                Colour = "#FF0000"
            };

            var response = ApiResponse<int>.Fail("Invalid label ID");

            _mediatorMock.Setup(m => m.Send(It.IsAny<EditLabelCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.EditLabel(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
        }

        [Fact]
        public async Task EditLabel_WithNegativeId_HandledByMediator()
        {
            // Arrange
            var command = new EditLabelCommand
            {
                Id = -1,
                Name = "Label",
                Colour = "#FF0000"
            };

            var response = ApiResponse<int>.Fail("Invalid label ID");

            _mediatorMock.Setup(m => m.Send(It.IsAny<EditLabelCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.EditLabel(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
        }

        [Fact]
        public async Task EditLabel_WithDuplicateName_ReturnsFailure()
        {
            // Arrange
            var command = new EditLabelCommand
            {
                Id = 1,
                Name = "Feature", // Name already used by another label
                Colour = "#FF0000"
            };

            var response = ApiResponse<int>.Fail("Label with this name already exists");

            _mediatorMock.Setup(m => m.Send(It.IsAny<EditLabelCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.EditLabel(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Status);
            Assert.Contains("already exists", result.Message);
        }

        [Fact]
        public async Task EditLabel_VerifiesCorrectCommand_IsSent()
        {
            // Arrange
            var command = new EditLabelCommand
            {
                Id = 1,
                Name = "Test",
                Colour = "#FF0000"
            };

            var response = ApiResponse<int>.Success(1, "Success");

            _mediatorMock.Setup(m => m.Send(It.Is<EditLabelCommand>(c => 
                c.Id == command.Id && 
                c.Name == command.Name && 
                c.Colour == command.Colour), default))
                .ReturnsAsync(response);

            // Act
            await _controller.EditLabel(command);

            // Assert
            _mediatorMock.Verify(m => m.Send(It.Is<EditLabelCommand>(c => 
                c.Id == 1 && 
                c.Name == "Test" && 
                c.Colour == "#FF0000"), default), Times.Once);
        }

        #endregion
    }
}
