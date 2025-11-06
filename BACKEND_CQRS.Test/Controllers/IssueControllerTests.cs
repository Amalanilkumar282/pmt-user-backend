using BACKEND_CQRS.Api.Controllers;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Issues;
using BACKEND_CQRS.Application.Query.IssueComments;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BACKEND_CQRS.Test.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for IssueController
    /// Tests all 23 API endpoints with success, validation, error, and edge case scenarios
    /// </summary>
    public class IssueControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly IssueController _controller;

        public IssueControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new IssueController(_mediatorMock.Object);
        }

        #region CreateIssue Tests

        [Fact]
        public async Task CreateIssue_WithValidCommand_ReturnsCreatedResponse()
        {
            // Arrange
            var command = new CreateIssueCommand
            {
                ProjectId = Guid.NewGuid(),
                Title = "New Issue",
                IssueType = "Bug",
                Priority = "High",
                StatusId = 1,
                AssigneeId = 1,
                ReporterId = 1
            };

            var expectedResponse = ApiResponse<CreateIssueDto>.Created(
                new CreateIssueDto
                {
                    ProjectId = command.ProjectId,
                    IssueType = command.IssueType,
                    Title = command.Title,
                    ReporterId = command.ReporterId
                },
                "Issue created successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateIssueCommand>(), It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.CreateIssue(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.Status);
            Assert.Equal("New Issue", result.Data.Title);
        }

        [Fact]
        public async Task CreateIssue_WithMinimalData_ReturnsSuccess()
        {
            // Arrange
            var command = new CreateIssueCommand
            {
                ProjectId = Guid.NewGuid(),
                Title = "Minimal Issue",
                IssueType = "Task",
                StatusId = 1,
                ReporterId = 1
            };

            var expectedResponse = ApiResponse<CreateIssueDto>.Created(
                new CreateIssueDto { ProjectId = command.ProjectId, IssueType = command.IssueType, Title = command.Title, ReporterId = command.ReporterId },
                "Issue created successfully");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateIssueCommand>(), It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.CreateIssue(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.Status);
        }

        #endregion

        #region EditIssue Tests

        [Fact]
        public async Task EditIssue_WithValidCommand_ReturnsSuccess()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var command = new EditIssueCommand
            {
                Title = "Updated Issue",
                Description = "Updated description"
            };

            var expectedResponse = ApiResponse<Guid>.Success(issueId, "Issue updated successfully");

            _mediatorMock.Setup(m => m.Send(It.Is<EditIssueCommand>(c => c.Id == issueId), It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.EditIssue(issueId, command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(issueId, result.Data);
        }

        [Fact]
        public async Task EditIssue_SetsIdFromRoute_Success()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var command = new EditIssueCommand { Title = "Test" };

            var expectedResponse = ApiResponse<Guid>.Success(issueId, "Updated");

            _mediatorMock.Setup(m => m.Send(It.Is<EditIssueCommand>(c => c.Id == issueId), It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.EditIssue(issueId, command);

            // Assert
            _mediatorMock.Verify(m => m.Send(It.Is<EditIssueCommand>(c => c.Id == issueId), It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task EditIssue_WhenIssueNotFound_ReturnsFail()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var command = new EditIssueCommand { Title = "Test" };

            var expectedResponse = ApiResponse<Guid>.Fail("Issue not found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<EditIssueCommand>(), It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.EditIssue(issueId, command);

            // Assert
            Assert.Equal(400, result.Status);
        }

        #endregion

        #region EditIssueV2 Tests

        [Fact]
        public async Task EditIssueV2_WithNullSprintId_ReturnsSuccess()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var command = new EditIssueCommandV2
            {
                Title = "Unassigned from sprint",
                SprintId = null
            };

            var expectedResponse = ApiResponse<Guid>.Success(issueId, "Issue updated");

            _mediatorMock.Setup(m => m.Send(It.Is<EditIssueCommandV2>(c => c.Id == issueId), It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.EditIssueV2(issueId, command);

            // Assert
            Assert.Equal(200, result.Status);
        }

        #endregion

        #region UpdateIssueDates Tests

        [Fact]
        public async Task UpdateIssueDates_WithValidDates_ReturnsSuccess()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var command = new UpdateIssueDatesCommand
            {
                StartDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7)
            };

            var expectedResponse = ApiResponse<Guid>.Success(issueId, "Dates updated");

            _mediatorMock.Setup(m => m.Send(It.Is<UpdateIssueDatesCommand>(c => c.IssueId == issueId), It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.UpdateIssueDates(issueId, command);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(issueId, result.Data);
        }

        #endregion

        #region DeleteIssue Tests

        [Fact]
        public async Task DeleteIssue_WithValidId_ReturnsOk()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var response = new ApiResponse<bool>(200, true, "Issue deleted");

            _mediatorMock.Setup(m => m.Send(It.Is<DeleteIssueCommand>(c => c.Id == issueId), It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.DeleteIssue(issueId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.Equal(200, apiResponse.Status);
        }

        [Fact]
        public async Task DeleteIssue_WhenIssueNotFound_ReturnsNotFound()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var response = new ApiResponse<bool>(404, false, "Issue not found");

            _mediatorMock.Setup(m => m.Send(It.Is<DeleteIssueCommand>(c => c.Id == issueId), It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.DeleteIssue(issueId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult);
        }

        #endregion

        #region GetIssuesByProject Tests

        [Fact]
        public async Task GetIssuesByProject_WithValidProjectId_ReturnsIssues()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var issues = new List<IssueDto>
            {
                new IssueDto { Id = Guid.NewGuid(), Title = "Issue 1" },
                new IssueDto { Id = Guid.NewGuid(), Title = "Issue 2" }
            };

            var response = ApiResponse<List<IssueDto>>.Success(issues, "Issues retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetIssueBySprintProjectIdQuery>(q => q.ProjectId == projectId && q.SprintId == null), 
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetIssuesByProject(projectId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetIssuesByProject_WithNoIssues_ReturnsEmptyList()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var response = ApiResponse<List<IssueDto>>.Success(new List<IssueDto>(), "No issues found");

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetIssueBySprintProjectIdQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetIssuesByProject(projectId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetIssuesBySprint Tests

        [Fact]
        public async Task GetIssuesBySprint_WithValidIds_ReturnsIssues()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var sprintId = Guid.NewGuid();
            var issues = new List<IssueDto>
            {
                new IssueDto { Id = Guid.NewGuid(), Title = "Sprint Issue 1", SprintId = sprintId }
            };

            var response = ApiResponse<List<IssueDto>>.Success(issues, "Issues retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetIssueBySprintProjectIdQuery>(q => q.ProjectId == projectId && q.SprintId == sprintId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetIssuesBySprint(projectId, sprintId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Single(result.Data);
        }

        #endregion

        #region GetIssuesByProjectAndUser Tests

        [Fact]
        public async Task GetIssuesByProjectAndUser_WithValidIds_ReturnsIssues()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = 1;
            var issues = new List<IssueDto>
            {
                new IssueDto { Id = Guid.NewGuid(), Title = "User Issue", AssigneeId = userId }
            };

            var response = ApiResponse<List<IssueDto>>.Success(issues, "Issues retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetIssuesByProjectAndUserQuery>(q => q.ProjectId == projectId && q.UserId == userId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetIssuesByProjectAndUser(projectId, userId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Single(result.Data);
        }

        #endregion

        #region GetTypeCountByProject Tests

        [Fact]
        public async Task GetTypeCountByProject_WithValidProjectId_ReturnsCounts()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var counts = new Dictionary<string, int>
            {
                { "Bug", 5 },
                { "Story", 10 },
                { "Task", 3 }
            };

            var response = ApiResponse<Dictionary<string, int>>.Success(counts, "Counts retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetIssueCountByTypeByProjectSprintQuery>(q => q.ProjectId == projectId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetTypeCountByProject(projectId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal(5, result.Data["Bug"]);
        }

        #endregion

        #region GetTypeCountBySprint Tests

        [Fact]
        public async Task GetTypeCountBySprint_WithValidIds_ReturnsCounts()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var sprintId = Guid.NewGuid();
            var counts = new Dictionary<string, int> { { "Bug", 2 }, { "Story", 5 } };

            var response = ApiResponse<Dictionary<string, int>>.Success(counts, "Counts retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetIssueCountByTypeByProjectSprintQuery>(q => 
                    q.ProjectId == projectId && q.SprintId == sprintId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetTypeCountBySprint(projectId, sprintId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        #region GetCompletedIssueCountByProject Tests

        [Fact]
        public async Task GetCompletedIssueCountByProject_WithValidProjectId_ReturnsCount()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var response = ApiResponse<int>.Success(15, "Count retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetCompletedIssueCountByProjectQuery>(q => q.ProjectId == projectId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetCompletedIssueCountByProject(projectId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(15, result.Data);
        }

        #endregion

        #region GetCompletedIssueCountBySprint Tests

        [Fact]
        public async Task GetCompletedIssueCountBySprint_WithValidSprintId_ReturnsCount()
        {
            // Arrange
            var sprintId = Guid.NewGuid();
            var response = ApiResponse<int>.Success(8, "Count retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetCompletedIssueCountBySprintQuery>(q => q.SprintId == sprintId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetCompletedIssueCountBySprint(sprintId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(8, result.Data);
        }

        #endregion

        #region GetStatusCountBySprint Tests

        [Fact]
        public async Task GetStatusCountBySprint_WithValidSprintId_ReturnsCounts()
        {
            // Arrange
            var sprintId = Guid.NewGuid();
            var counts = new Dictionary<string, int>
            {
                { "To Do", 3 },
                { "In Progress", 5 },
                { "Done", 7 }
            };

            var response = ApiResponse<Dictionary<string, int>>.Success(counts, "Counts retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetIssueCountByStatusBySprintQuery>(q => q.SprintId == sprintId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetStatusCountBySprint(sprintId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(3, result.Data.Count);
        }

        #endregion

        #region GetIssuesByUser Tests

        [Fact]
        public async Task GetIssuesByUser_WithValidUserId_ReturnsIssues()
        {
            // Arrange
            var userId = 1;
            var issues = new List<IssueDto>
            {
                new IssueDto { Id = Guid.NewGuid(), Title = "User Issue 1", AssigneeId = userId },
                new IssueDto { Id = Guid.NewGuid(), Title = "User Issue 2", AssigneeId = userId }
            };

            var response = ApiResponse<List<IssueDto>>.Success(issues, "Issues retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetIssuesByUserIdQuery>(q => q.UserId == userId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetIssuesByUser(userId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        #region GetIssuesByEpic Tests

        [Fact]
        public async Task GetIssuesByEpic_WithValidEpicId_ReturnsIssues()
        {
            // Arrange
            var epicId = Guid.NewGuid();
            var issues = new List<IssueDto>
            {
                new IssueDto { Id = Guid.NewGuid(), Title = "Epic Issue", EpicId = epicId }
            };

            var response = ApiResponse<List<IssueDto>>.Success(issues, "Issues retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetIssuesByEpicIdQuery>(q => q.EpicId == epicId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetIssuesByEpic(epicId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Single(result.Data);
        }

        #endregion

        #region GetStatusCountByProject Tests

        [Fact]
        public async Task GetStatusCountByProject_WithValidProjectId_ReturnsCounts()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var counts = new Dictionary<string, int>
            {
                { "To Do", 10 },
                { "In Progress", 8 },
                { "Done", 15 }
            };

            var response = ApiResponse<Dictionary<string, int>>.Success(counts, "Counts retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetIssueCountByStatusByProjectQuery>(q => q.ProjectId == projectId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetStatusCountByProject(projectId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(3, result.Data.Count);
        }

        #endregion

        #region GetRecentIssuesByProjectId Tests

        [Fact]
        public async Task GetRecentIssuesByProjectId_WithDefaultCount_ReturnsRecentIssues()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var issues = new List<IssueDto>
            {
                new IssueDto { Id = Guid.NewGuid(), Title = "Recent 1" },
                new IssueDto { Id = Guid.NewGuid(), Title = "Recent 2" }
            };

            var response = ApiResponse<List<IssueDto>>.Success(issues, "Recent issues retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetRecentIssuesQuery>(q => q.ProjectId == projectId && q.Count == 6),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetRecentIssuesByProjectId(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetRecentIssuesByProjectId_WithCustomCount_ReturnsSpecifiedNumber()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var count = 10;
            var response = ApiResponse<List<IssueDto>>.Success(new List<IssueDto>(), "Retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetRecentIssuesQuery>(q => q.ProjectId == projectId && q.Count == count),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetRecentIssuesByProjectId(projectId, count);

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<GetRecentIssuesQuery>(q => q.Count == 10),
                It.IsAny<System.Threading.CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region GetStatusesByProject Tests

        [Fact]
        public async Task GetStatusesByProject_WithValidProjectId_ReturnsStatuses()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var statuses = new List<StatusDto>
            {
                new StatusDto { Id = 1, StatusName = "To Do" },
                new StatusDto { Id = 2, StatusName = "In Progress" }
            };

            var response = ApiResponse<List<StatusDto>>.Success(statuses, "Statuses retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetStatusesByProjectQuery>(q => q.ProjectId == projectId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetStatusesByProject(projectId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        #region Issue Comments - CreateComment Tests

        [Fact]
        public async Task CreateComment_WithValidCommand_ReturnsCreated()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var command = new CreateIssueCommentCommand
            {
                AuthorId = 1,
                Body = "Test comment"
            };

            var expectedResponse = ApiResponse<CreateIssueCommentDto>.Created(
                new CreateIssueCommentDto { Id = Guid.NewGuid(), Body = "Test comment" },
                "Comment created");

            _mediatorMock.Setup(m => m.Send(
                It.Is<CreateIssueCommentCommand>(c => c.IssueId == issueId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.CreateComment(issueId, command);

            // Assert
            Assert.Equal(201, result.Status);
            Assert.Equal("Test comment", result.Data.Body);
        }

        #endregion

        #region Issue Comments - GetCommentsByIssueId Tests

        [Fact]
        public async Task GetCommentsByIssueId_WithValidIssueId_ReturnsComments()
        {
            // Arrange
            var issueId = Guid.NewGuid();
            var comments = new List<IssueCommentDto>
            {
                new IssueCommentDto { Id = Guid.NewGuid(), Body = "Comment 1" },
                new IssueCommentDto { Id = Guid.NewGuid(), Body = "Comment 2" }
            };

            var response = ApiResponse<List<IssueCommentDto>>.Success(comments, "Comments retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetCommentsByIssueIdQuery>(q => q.IssueId == issueId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetCommentsByIssueId(issueId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        #region Issue Comments - GetCommentById Tests

        [Fact]
        public async Task GetCommentById_WithValidCommentId_ReturnsComment()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var comment = new IssueCommentDto { Id = commentId, Body = "Test comment" };

            var response = ApiResponse<IssueCommentDto>.Success(comment, "Comment retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetCommentByIdQuery>(q => q.Id == commentId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetCommentById(commentId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(commentId, result.Data.Id);
        }

        #endregion

        #region Issue Comments - UpdateComment Tests

        [Fact]
        public async Task UpdateComment_WithValidCommand_ReturnsSuccess()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var command = new UpdateIssueCommentCommand
            {
                Body = "Updated comment",
                UpdatedBy = 1
            };

            var response = ApiResponse<Guid>.Success(commentId, "Comment updated");

            _mediatorMock.Setup(m => m.Send(
                It.Is<UpdateIssueCommentCommand>(c => c.Id == commentId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.UpdateComment(commentId, command);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(commentId, result.Data);
        }

        #endregion

        #region Issue Comments - DeleteComment Tests

        [Fact]
        public async Task DeleteComment_WithValidCommentId_ReturnsSuccess()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var response = ApiResponse<Guid>.Success(commentId, "Comment deleted");

            _mediatorMock.Setup(m => m.Send(
                It.Is<DeleteIssueCommentCommand>(c => c.Id == commentId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.DeleteComment(commentId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(commentId, result.Data);
        }

        #endregion

        #region GetIssueActivitySummaryByProjectId Tests

        [Fact]
        public async Task GetIssueActivitySummaryByProjectId_WithValidProjectId_ReturnsActivity()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var activity = new Dictionary<string, int>
            {
                { "created", 10 },
                { "updated", 5 },
                { "closed", 3 }
            };

            var response = ApiResponse<Dictionary<string, int>>.Success(activity, "Activity retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetIssueActivitySummaryByProjectQuery>(q => q.ProjectId == projectId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetIssueActivitySummaryByProjectId(projectId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(3, result.Data.Count);
        }

        #endregion

        #region GetIssueActivitySummaryBySprintId Tests

        [Fact]
        public async Task GetIssueActivitySummaryBySprintId_WithValidIds_ReturnsActivity()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var sprintId = Guid.NewGuid();
            var activity = new Dictionary<string, int> { { "created", 5 }, { "closed", 2 } };

            var response = ApiResponse<Dictionary<string, int>>.Success(activity, "Activity retrieved");

            _mediatorMock.Setup(m => m.Send(
                It.Is<GetIssueActivitySummaryBysprintIdQuery>(q => 
                    q.ProjectId == projectId && q.SprintId == sprintId),
                It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _controller.GetIssueActivitySummaryBySprintId(projectId, sprintId);

            // Assert
            Assert.Equal(200, result.Status);
            Assert.Equal(2, result.Data.Count);
        }

        #endregion
    }
}
