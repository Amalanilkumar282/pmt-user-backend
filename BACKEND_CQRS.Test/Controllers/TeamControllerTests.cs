using BACKEND_CQRS.Api.Controllers;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Teams;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Text.Json;

namespace BACKEND_CQRS.Test.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for TeamController
    /// Tests all API endpoints with success, validation, error, and edge case scenarios
    /// </summary>
    public class TeamControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly TeamController _controller;

        public TeamControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new TeamController(_mediatorMock.Object);
        }

        #region GetTeamsByProjectId Tests

        [Fact]
        public async Task GetTeamsByProjectId_WithValidProjectId_ReturnsOk()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var teams = new List<TeamDetailsDto>
            {
                new TeamDetailsDto { TeamId = 1, TeamName = "Team Alpha" },
                new TeamDetailsDto { TeamId = 2, TeamName = "Team Beta" }
            };

            _mediatorMock.Setup(m => m.Send(It.Is<GetTeamsByProjectIdQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(teams);

            // Act
            var result = await _controller.GetTeamsByProjectId(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTeams = Assert.IsType<List<TeamDetailsDto>>(okResult.Value);
            Assert.Equal(2, returnedTeams.Count);
        }

        [Fact]
        public async Task GetTeamsByProjectId_WithNoTeams_ReturnsEmptyList()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var emptyList = new List<TeamDetailsDto>();

            _mediatorMock.Setup(m => m.Send(It.Is<GetTeamsByProjectIdQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _controller.GetTeamsByProjectId(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTeams = Assert.IsType<List<TeamDetailsDto>>(okResult.Value);
            Assert.Empty(returnedTeams);
        }

        [Fact]
        public async Task GetTeamsByProjectId_WithEmptyGuid_CallsMediator()
        {
            // Arrange
            var projectId = Guid.Empty;
            var teams = new List<TeamDetailsDto>();

            _mediatorMock.Setup(m => m.Send(It.Is<GetTeamsByProjectIdQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(teams);

            // Act
            var result = await _controller.GetTeamsByProjectId(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
        }

        #endregion

        #region GetTeamsByProjectIdV2 Tests

        [Fact]
        public async Task GetTeamsByProjectIdV2_WithValidProjectId_ReturnsOkWithWrappedData()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var teams = new List<TeamDetailsV2Dto>
            {
                new TeamDetailsV2Dto { Id = 1, Name = "Team Alpha" },
                new TeamDetailsV2Dto { Id = 2, Name = "Team Beta" }
            };

            _mediatorMock.Setup(m => m.Send(It.Is<GetTeamsByProjectIdV2Query>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(teams);

            // Act
            var result = await _controller.GetTeamsByProjectIdV2(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            
            Assert.True(response["succeeded"].GetBoolean());
            Assert.Equal(200, response["statusCode"].GetInt32());
            Assert.NotNull(response["data"]);
        }

        [Fact]
        public async Task GetTeamsByProjectIdV2_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            _mediatorMock.Setup(m => m.Send(It.Is<GetTeamsByProjectIdV2Query>(q => q.ProjectId == projectId), default))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetTeamsByProjectIdV2(projectId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            
            var json = JsonSerializer.Serialize(statusCodeResult.Value);
            var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            
            Assert.False(response["succeeded"].GetBoolean());
            Assert.Equal(500, response["statusCode"].GetInt32());
        }

        [Fact]
        public async Task GetTeamsByProjectIdV2_WithEmptyList_ReturnsOk()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var emptyList = new List<TeamDetailsV2Dto>();

            _mediatorMock.Setup(m => m.Send(It.Is<GetTeamsByProjectIdV2Query>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _controller.GetTeamsByProjectIdV2(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            
            Assert.True(response["succeeded"].GetBoolean());
        }

        #endregion

        #region CreateTeam Tests

        [Fact]
        public async Task CreateTeam_WithValidDto_ReturnsOk()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var dto = new CreateTeamDto
            {
                ProjectId = projectId,
                Name = "New Team",
                Description = "Test team",
                LeadId = 1,
                MemberIds = new List<int> { 2, 3, 4 },
                Label = new List<string> { "Development" },
                CreatedBy = 1
            };

            var expectedTeamId = 5;

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateTeamCommand>(), default))
                .ReturnsAsync(expectedTeamId);

            // Act
            var result = await _controller.CreateTeam(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            
            Assert.True(response["success"].GetBoolean());
            Assert.Equal("Team created successfully.", response["message"].GetString());
            Assert.Equal(expectedTeamId, response["teamId"].GetInt32());
        }

        [Fact]
        public async Task CreateTeam_WithNullDto_ReturnsBadRequest()
        {
            // Arrange
            CreateTeamDto dto = null;

            // Act
            var result = await _controller.CreateTeam(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var json = JsonSerializer.Serialize(badRequestResult.Value);
            var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            
            Assert.False(response["success"].GetBoolean());
            Assert.Contains("Invalid request data", response["message"].GetString());
        }

        [Fact]
        public async Task CreateTeam_WithEmptyProjectId_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateTeamDto
            {
                ProjectId = Guid.Empty,
                Name = "Test Team",
                CreatedBy = 1
            };

            // Act
            var result = await _controller.CreateTeam(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var json = JsonSerializer.Serialize(badRequestResult.Value);
            var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            
            Assert.False(response["success"].GetBoolean());
            Assert.Contains("ProjectId is required", response["message"].GetString());
        }

        [Fact]
        public async Task CreateTeam_WithEmptyName_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateTeamDto
            {
                ProjectId = Guid.NewGuid(),
                Name = "   ",
                CreatedBy = 1
            };

            // Act
            var result = await _controller.CreateTeam(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var json = JsonSerializer.Serialize(badRequestResult.Value);
            var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            
            Assert.False(response["success"].GetBoolean());
            Assert.Contains("Team name is required", response["message"].GetString());
        }

        [Fact]
        public async Task CreateTeam_WithNullName_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateTeamDto
            {
                ProjectId = Guid.NewGuid(),
                Name = null,
                CreatedBy = 1
            };

            // Act
            var result = await _controller.CreateTeam(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var json = JsonSerializer.Serialize(badRequestResult.Value);
            var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            
            Assert.False(response["success"].GetBoolean());
        }

        [Fact]
        public async Task CreateTeam_WithZeroCreatedBy_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateTeamDto
            {
                ProjectId = Guid.NewGuid(),
                Name = "Test Team",
                CreatedBy = 0
            };

            // Act
            var result = await _controller.CreateTeam(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var json = JsonSerializer.Serialize(badRequestResult.Value);
            var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            
            Assert.False(response["success"].GetBoolean());
            Assert.Contains("CreatedBy is required", response["message"].GetString());
        }

        [Fact]
        public async Task CreateTeam_WithNullMemberIds_ReturnsOk()
        {
            // Arrange
            var dto = new CreateTeamDto
            {
                ProjectId = Guid.NewGuid(),
                Name = "Team without members",
                CreatedBy = 1,
                MemberIds = null
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateTeamCommand>(), default))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateTeam(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            
            Assert.True(response["success"].GetBoolean());
        }

        [Fact]
        public async Task CreateTeam_WithTrimmedDescription_CreatesSuccessfully()
        {
            // Arrange
            var dto = new CreateTeamDto
            {
                ProjectId = Guid.NewGuid(),
                Name = "Team",
                Description = "   Description with spaces   ",
                CreatedBy = 1
            };

            _mediatorMock.Setup(m => m.Send(
                It.Is<CreateTeamCommand>(c => c.Description == "Description with spaces"), 
                default))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateTeam(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mediatorMock.Verify(m => m.Send(
                It.Is<CreateTeamCommand>(c => c.Description == "Description with spaces"), 
                default), 
                Times.Once);
        }

        #endregion

        #region DeleteTeam Tests

        [Fact]
        public async Task DeleteTeam_WithValidId_ReturnsOk()
        {
            // Arrange
            var teamId = 1;

            _mediatorMock.Setup(m => m.Send(It.Is<DeleteTeamCommand>(c => c.TeamId == teamId), default))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteTeam(teamId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            
            Assert.Equal("Team deleted successfully", response["Message"].GetString());
        }

        [Fact]
        public async Task DeleteTeam_WhenTeamNotFound_ReturnsNotFound()
        {
            // Arrange
            var teamId = 999;

            _mediatorMock.Setup(m => m.Send(It.Is<DeleteTeamCommand>(c => c.TeamId == teamId), default))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteTeam(teamId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var json = JsonSerializer.Serialize(notFoundResult.Value);
            var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            
            Assert.Equal("Team not found", response["Message"].GetString());
        }

        [Fact]
        public async Task DeleteTeam_WithZeroId_CallsMediator()
        {
            // Arrange
            var teamId = 0;

            _mediatorMock.Setup(m => m.Send(It.Is<DeleteTeamCommand>(c => c.TeamId == teamId), default))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteTeam(teamId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult);
        }

        #endregion

        #region GetTeamCountByProjectId Tests

        [Fact]
        public async Task GetTeamCountByProjectId_WithValidProjectId_ReturnsOk()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var expectedCount = new TeamCountDto
            {
                TotalTeams = 5,
                ActiveTeams = 4,
                AssignedMembersCount = 20
            };

            _mediatorMock.Setup(m => m.Send(It.Is<GetTeamCountByProjectIdQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _controller.GetTeamCountByProjectId(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var countDto = Assert.IsType<TeamCountDto>(okResult.Value);
            Assert.Equal(5, countDto.TotalTeams);
        }

        [Fact]
        public async Task GetTeamCountByProjectId_WithNoTeams_ReturnsZero()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var expectedCount = new TeamCountDto
            {
                TotalTeams = 0,
                ActiveTeams = 0,
                AssignedMembersCount = 0
            };

            _mediatorMock.Setup(m => m.Send(It.Is<GetTeamCountByProjectIdQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _controller.GetTeamCountByProjectId(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var countDto = Assert.IsType<TeamCountDto>(okResult.Value);
            Assert.Equal(0, countDto.TotalTeams);
        }

        #endregion

        #region GetTeamDetailsByProjectId Tests

        [Fact]
        public async Task GetTeamDetailsByProjectId_WithValidProjectId_ReturnsOk()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var teams = new List<TeamDetailsDto>
            {
                new TeamDetailsDto { TeamId = 1, TeamName = "Team 1" }
            };

            _mediatorMock.Setup(m => m.Send(It.Is<GetTeamsByProjectIdQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(teams);

            // Act
            var result = await _controller.GetTeamDetailsByProjectId(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTeams = Assert.IsType<List<TeamDetailsDto>>(okResult.Value);
            Assert.Single(returnedTeams);
        }

        #endregion

        #region UpdateTeam Tests

        [Fact]
        public async Task UpdateTeam_WithValidData_ReturnsOk()
        {
            // Arrange
            var teamId = 1;
            var updateDto = new UpdateTeamDto
            {
                Name = "Updated Team Name",
                Description = "Updated description"
            };

            _mediatorMock.Setup(m => m.Send(It.Is<UpdateTeamCommand>(c => c.Id == teamId), default))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateTeam(teamId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            
            Assert.Equal("Team updated successfully.", response["message"].GetString());
        }

        [Fact]
        public async Task UpdateTeam_WithNullDto_ReturnsBadRequest()
        {
            // Arrange
            var teamId = 1;
            UpdateTeamDto updateDto = null;

            // Act
            var result = await _controller.UpdateTeam(teamId, updateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var json = JsonSerializer.Serialize(badRequestResult.Value);
            var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            
            Assert.Equal("Invalid request data.", response["message"].GetString());
        }

        [Fact]
        public async Task UpdateTeam_WhenTeamNotFound_ReturnsNotFound()
        {
            // Arrange
            var teamId = 999;
            var updateDto = new UpdateTeamDto { Name = "Test" };

            _mediatorMock.Setup(m => m.Send(It.Is<UpdateTeamCommand>(c => c.Id == teamId), default))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateTeam(teamId, updateDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var json = JsonSerializer.Serialize(notFoundResult.Value);
            var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            
            Assert.Equal("Team not found.", response["message"].GetString());
        }

        #endregion

        #region GetProjectMemberCount Tests

        [Fact]
        public async Task GetProjectMemberCount_WithValidProjectId_ReturnsOk()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var expectedCount = new ProjectMemberCountDto
            {
                TotalMembers = 10,
                ActiveMembers = 8,
                UnassignedMembers = 2
            };

            _mediatorMock.Setup(m => m.Send(It.Is<GetProjectMemberCountQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _controller.GetProjectMemberCount(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var countDto = Assert.IsType<ProjectMemberCountDto>(okResult.Value);
            Assert.Equal(10, countDto.TotalMembers);
        }

        [Fact]
        public async Task GetProjectMemberCount_WithNoMembers_ReturnsZero()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var expectedCount = new ProjectMemberCountDto
            {
                TotalMembers = 0,
                ActiveMembers = 0,
                UnassignedMembers = 0
            };

            _mediatorMock.Setup(m => m.Send(It.Is<GetProjectMemberCountQuery>(q => q.ProjectId == projectId), default))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _controller.GetProjectMemberCount(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var countDto = Assert.IsType<ProjectMemberCountDto>(okResult.Value);
            Assert.Equal(0, countDto.TotalMembers);
        }

        #endregion

        #region GetTeamDetailsByTeamId Tests

        [Fact]
        public async Task GetTeamDetailsByTeamId_WithValidTeamId_ReturnsOk()
        {
            // Arrange
            var teamId = 1;
            var teamDetails = new TeamDetailsDto
            {
                TeamId = teamId,
                TeamName = "Team Alpha",
                Description = "Development team",
                MemberCount = 5
            };

            _mediatorMock.Setup(m => m.Send(It.Is<GetTeamDetailsByTeamIdQuery>(q => q.TeamId == teamId), default))
                .ReturnsAsync(teamDetails);

            // Act
            var result = await _controller.GetTeamDetailsByTeamId(teamId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTeam = Assert.IsType<TeamDetailsDto>(okResult.Value);
            Assert.Equal(teamId, returnedTeam.TeamId);
            Assert.Equal("Team Alpha", returnedTeam.TeamName);
        }

        [Fact]
        public async Task GetTeamDetailsByTeamId_WhenTeamNotFound_ReturnsOk()
        {
            // Arrange
            var teamId = 999;

            _mediatorMock.Setup(m => m.Send(It.Is<GetTeamDetailsByTeamIdQuery>(q => q.TeamId == teamId), default))
                .ReturnsAsync((TeamDetailsDto?)null);

            // Act
            var result = await _controller.GetTeamDetailsByTeamId(teamId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        #endregion
    }
}
