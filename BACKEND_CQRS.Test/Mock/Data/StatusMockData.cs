using BACKEND_CQRS.Application.Dto;

namespace BACKEND_CQRS.Test.Mock.Data
{
    /// <summary>
    /// Mock data builder for Status-related DTOs
    /// </summary>
    public static class StatusMockData
    {
        public static StatusDto GetDefaultStatus()
        {
            return new StatusDto
            {
                Id = 1,
                StatusName = "To Do"
            };
        }

        public static List<StatusDto> GetMultipleStatuses()
        {
            return new List<StatusDto>
            {
                new StatusDto { Id = 1, StatusName = "To Do" },
                new StatusDto { Id = 2, StatusName = "In Progress" },
                new StatusDto { Id = 3, StatusName = "In Review" },
                new StatusDto { Id = 4, StatusName = "Done" },
                new StatusDto { Id = 5, StatusName = "Blocked" }
            };
        }

        public static StatusDto GetStatusById(int id)
        {
            var statuses = GetMultipleStatuses();
            return statuses.FirstOrDefault(s => s.Id == id) ?? GetDefaultStatus();
        }

        public static List<StatusDto> GetEmptyStatusList()
        {
            return new List<StatusDto>();
        }
    }
}
