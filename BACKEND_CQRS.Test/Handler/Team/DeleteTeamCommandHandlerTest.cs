using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Handlers.TeamHandlers;
using BACKEND_CQRS.Domain.Persistance;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Test.Handler.Team
{
    public class DeleteTeamCommandHandlerTests
    {
        private readonly Mock<ITeamRepository> _mockRepo;
        private readonly DeleteTeamCommandHandler _handler;

        public DeleteTeamCommandHandlerTests()
        {
            _mockRepo = new Mock<ITeamRepository>();
            _handler = new DeleteTeamCommandHandler(_mockRepo.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenDeleteSucceeds()
        {
            // Arrange
            int teamId = 1;
            var command = new DeleteTeamCommand(teamId);
            _mockRepo.Setup(r => r.DeleteTeamAsync(teamId)).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockRepo.Verify(r => r.DeleteTeamAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenDeleteFails()
        {
            // Arrange
            int teamId = 2;
            var command = new DeleteTeamCommand(teamId);
            _mockRepo.Setup(r => r.DeleteTeamAsync(teamId)).ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
            _mockRepo.Verify(r => r.DeleteTeamAsync(teamId), Times.Once);
        }
    }
}
