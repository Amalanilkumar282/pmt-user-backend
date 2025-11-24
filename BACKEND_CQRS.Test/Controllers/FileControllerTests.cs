using BACKEND_CQRS.Api.Controllers;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;

namespace BACKEND_CQRS.Test.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for FileController
    /// Tests file upload endpoint with success, validation, error, and edge case scenarios
    /// </summary>
    public class FileControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly FileController _controller;

        public FileControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new FileController(_mediatorMock.Object);
        }

        #region UploadFile Tests

        [Fact]
        public async Task UploadFile_WithValidFile_ReturnsSuccess()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var content = "Hello World from a Fake File";
            var fileName = "test.txt";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.ContentType).Returns("text/plain");

            var expectedUrl = "https://storage.example.com/attachments/test.txt";
            var response = ApiResponse<string>.Success(expectedUrl, "File uploaded successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UploadFile(fileMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(expectedUrl, result.Data);
            Assert.Contains("successfully", result.Message);
        }

        [Fact]
        public async Task UploadFile_WithCustomBucketName_UsesSpecifiedBucket()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("document.pdf");
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var bucketName = "documents";
            var expectedUrl = "https://storage.example.com/documents/document.pdf";
            var response = ApiResponse<string>.Success(expectedUrl, "File uploaded");

            _mediatorMock.Setup(m => m.Send(
                It.Is<UploadFileCommand>(c => c.BucketName == bucketName),
                default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UploadFile(fileMock.Object, bucketName);

            // Assert
            Assert.Equal(200, result.Status);
            _mediatorMock.Verify(m => m.Send(
                It.Is<UploadFileCommand>(c => c.BucketName == bucketName),
                default),
                Times.Once);
        }

        [Fact]
        public async Task UploadFile_WithDefaultBucketName_UsesAttachmentsBucket()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("image.jpg");
            fileMock.Setup(f => f.Length).Returns(2048);
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var response = ApiResponse<string>.Success("url", "Uploaded");

            _mediatorMock.Setup(m => m.Send(
                It.Is<UploadFileCommand>(c => c.BucketName == "attachments"),
                default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UploadFile(fileMock.Object);

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<UploadFileCommand>(c => c.BucketName == "attachments"),
                default),
                Times.Once);
        }

        [Fact]
        public async Task UploadFile_WithImageFile_ReturnsSuccess()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("avatar.png");
            fileMock.Setup(f => f.Length).Returns(5000);
            fileMock.Setup(f => f.ContentType).Returns("image/png");
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var expectedUrl = "https://storage.example.com/attachments/avatar.png";
            var response = ApiResponse<string>.Success(expectedUrl, "Image uploaded");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UploadFile(fileMock.Object);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Contains("storage.example.com", result.Data);
        }

        [Fact]
        public async Task UploadFile_WithPdfFile_ReturnsSuccess()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("document.pdf");
            fileMock.Setup(f => f.Length).Returns(10000);
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var expectedUrl = "https://storage.example.com/attachments/document.pdf";
            var response = ApiResponse<string>.Success(expectedUrl, "PDF uploaded");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UploadFile(fileMock.Object);

            // Assert
            Assert.Equal(200, result.Status);
        }

        [Fact]
        public async Task UploadFile_WithNullFile_SendsToMediator()
        {
            // Arrange
            var response = ApiResponse<string>.Fail("File is required");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UploadFile(null!);

            // Assert
            Assert.Equal(400, result.Status);
            _mediatorMock.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), default), Times.Once);
        }

        [Fact]
        public async Task UploadFile_WithEmptyFile_ReturnsFailure()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("empty.txt");
            fileMock.Setup(f => f.Length).Returns(0);
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var response = ApiResponse<string>.Fail("File is empty");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UploadFile(fileMock.Object);

            // Assert
            Assert.Equal(400, result.Status);
            Assert.Contains("empty", result.Message);
        }

        [Fact]
        public async Task UploadFile_WithLargeFile_ReturnsSuccess()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("large-file.zip");
            fileMock.Setup(f => f.Length).Returns(50 * 1024 * 1024); // 50 MB
            fileMock.Setup(f => f.ContentType).Returns("application/zip");
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var expectedUrl = "https://storage.example.com/attachments/large-file.zip";
            var response = ApiResponse<string>.Success(expectedUrl, "Large file uploaded");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UploadFile(fileMock.Object);

            // Assert
            Assert.Equal(200, result.Status);
        }

        [Fact]
        public async Task UploadFile_WithSpecialCharactersInFileName_ReturnsSuccess()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("file with spaces & special-chars.txt");
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var expectedUrl = "https://storage.example.com/attachments/file-with-spaces-special-chars.txt";
            var response = ApiResponse<string>.Success(expectedUrl, "File uploaded");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UploadFile(fileMock.Object);

            // Assert
            Assert.Equal(200, result.Status);
        }

        [Fact]
        public async Task UploadFile_WhenStorageFails_ReturnsFailure()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.txt");
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var response = ApiResponse<string>.Fail("Storage service unavailable");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), default))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UploadFile(fileMock.Object);

            // Assert
            Assert.Equal(400, result.Status);
            Assert.Contains("unavailable", result.Message);
        }

        [Fact]
        public async Task UploadFile_VerifiesCommandProperties()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.txt");
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var bucketName = "custom-bucket";
            var response = ApiResponse<string>.Success("url", "Success");

            UploadFileCommand? capturedCommand = null;
            _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), default))
                .Callback<IRequest<ApiResponse<string>>, CancellationToken>((cmd, _) =>
                {
                    capturedCommand = cmd as UploadFileCommand;
                })
                .ReturnsAsync(response);

            // Act
            await _controller.UploadFile(fileMock.Object, bucketName);

            // Assert
            Assert.NotNull(capturedCommand);
            Assert.Equal(bucketName, capturedCommand.BucketName);
            Assert.Equal(fileMock.Object, capturedCommand.File);
        }

        [Fact]
        public async Task UploadFile_CallsMediatorOnce()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.txt");
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var response = ApiResponse<string>.Success("url", "Success");

            _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), default))
                .ReturnsAsync(response);

            // Act
            await _controller.UploadFile(fileMock.Object);

            // Assert
            _mediatorMock.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), default), Times.Once);
        }

        #endregion
    }
}
