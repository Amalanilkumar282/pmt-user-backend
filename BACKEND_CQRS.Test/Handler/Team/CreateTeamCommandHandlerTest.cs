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
    public class CreateTeamCommandHandlerTest
    {
        private readonly Mock<ITeamRepository> _mockRepo;
        private readonly CreateTeamCommandHandler _handler;

        public CreateTeamCommandHandlerTest()
        {
            _mockRepo = new Mock<ITeamRepository>();
            _handler = new CreateTeamCommandHandler(_mockRepo.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateTeam_AndAddMembersIncludingLead()
        {
            // Arrange
            var command = new CreateTeamCommand
            {
                ProjectId = Guid.NewGuid(),
                Name = "Core Team",
                Description = "Handles core modules",
                LeadId = 5,
                Label = new List<string> { "Backend", "API" },
                MemberIds = new List<int> { 2, 3 },
                CreatedBy = 1
            };

            var createdTeamId = 10;

            _mockRepo.Setup(r => r.CreateTeamAsync(It.IsAny<Teams>()))
                     .ReturnsAsync(createdTeamId);

            _mockRepo.Setup(r => r.AddMembersAsync(It.IsAny<int>(), It.IsAny<List<int>>()))
                     .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(createdTeamId, result);

            _mockRepo.Verify(r => r.CreateTeamAsync(It.Is<Teams>(
    t => t.Name == command.Name &&
         t.Description == command.Description &&
         t.ProjectId == command.ProjectId &&
         t.CreatedBy == command.CreatedBy &&
         t.IsActive == true
)), Times.Once);

            _mockRepo.Verify(r => r.AddMembersAsync(createdTeamId,
                It.Is<List<int>>(members => members.Contains(2) && members.Contains(3) && members.Contains(5))),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldNotDuplicateLeadIfAlreadyInMembers()
        {
            // Arrange
            var command = new CreateTeamCommand
            {
                ProjectId = Guid.NewGuid(),
                Name = "Test Team",
                Description = "Test Description",
                LeadId = 7,
                MemberIds = new List<int> { 7, 8 },
                CreatedBy = 2
            };

            _mockRepo.Setup(r => r.CreateTeamAsync(It.IsAny<Teams>()))
                     .ReturnsAsync(22);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(22, result);

            _mockRepo.Verify(r => r.AddMembersAsync(22,
                It.Is<List<int>>(m => m.Count == 2 && m.Contains(7) && m.Contains(8))),
                Times.Once);
        }
    }
}
