using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;

namespace BACKEND_CQRS.Test.Mock.Data
{
    /// <summary>
    /// Mock data builder for Sprint-related DTOs and Commands
    /// </summary>
    public static class SprintMockData
    {
        public static CreateSprintCommand GetDefaultCreateCommand()
        {
            return new CreateSprintCommand
            {
                ProjectId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                SprintName = "Sprint 1",
                SprintGoal = "Complete user authentication module",
                TeamAssigned = 1,
                StartDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14),
                Status = "Planned",
                StoryPoint = 25.5m
            };
        }

        public static CreateSprintDto GetDefaultSprintDto()
        {
            return new CreateSprintDto
            {
                ProjectId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                SprintName = "Sprint 1",
                SprintGoal = "Complete user authentication module",
                TeamAssigned = 1,
                Status = "Planned"
            };
        }

        public static List<SprintDto> GetMultipleSprints()
        {
            return new List<SprintDto>
            {
                new SprintDto 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Sprint 1", 
                    Status = "Active",
                    StartDate = DateTime.UtcNow.AddDays(-7),
                    DueDate = DateTime.UtcNow.AddDays(7)
                },
                new SprintDto 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Sprint 2", 
                    Status = "Planned",
                    StartDate = DateTime.UtcNow.AddDays(7),
                    DueDate = DateTime.UtcNow.AddDays(21)
                },
                new SprintDto 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Sprint 3", 
                    Status = "Completed",
                    StartDate = DateTime.UtcNow.AddDays(-21),
                    DueDate = DateTime.UtcNow.AddDays(-7)
                }
            };
        }

        public static UpdateSprintCommand GetDefaultUpdateCommand()
        {
            return new UpdateSprintCommand
            {
                Id = Guid.NewGuid(),
                SprintName = "Updated Sprint Name",
                Status = "Active"
            };
        }
    }
}
