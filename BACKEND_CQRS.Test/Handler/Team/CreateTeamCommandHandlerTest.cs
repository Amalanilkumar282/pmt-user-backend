using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Handlers.TeamHandlers;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Test.Handler.Team
{
    public class CreateTeamCommandHandlerTests
    {
        private readonly Mock<ITeamRepository> _teamRepositoryMock;
        private readonly CreateTeamCommandHandler _handler;

        public CreateTeamCommandHandlerTests()
        {
            _teamRepositoryMock = new Mock<ITeamRepository>();
            _handler = new CreateTeamCommandHandler(_teamRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_Should_CreateTeam_And_AddMembersIncludingLead()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var leadId = 101;
            var memberIds = new List<int> { 201, 202 };

            var command = new CreateTeamCommand
            {
                ProjectId = projectId,
                Name = "Alpha Team",
                Description = "Testing team creation",
                LeadId = leadId,
               
                CreatedBy = 1,
                MemberIds = memberIds
            };

            _teamRepositoryMock
                .Setup(repo => repo.CreateTeamAsync(It.IsAny<Teams>()))
                .ReturnsAsync(1);

            _teamRepositoryMock
                .Setup(repo => repo.AddMembersAsync(It.IsAny<int>(), It.IsAny<List<int>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(1, result); // returned teamId

            _teamRepositoryMock.Verify(
                repo => repo.CreateTeamAsync(It.Is<Teams>(t =>
                    t.ProjectId == projectId &&
                    t.Name == "Alpha Team" &&
                    t.Description == "Testing team creation" &&
                    t.LeadId == leadId &&
                    
                    t.CreatedBy == 1 &&
                    t.IsActive == true
                )),
                Times.Once);

            _teamRepositoryMock.Verify(
                repo => repo.AddMembersAsync(1,
                    It.Is<List<int>>(members =>
                        members.Contains(leadId) &&
                        members.Contains(201) &&
                        members.Contains(202)
                    )),
                Times.Once);
        }
    }
}
