using BACKEND_CQRS.Domain.Dto.AI;
using System;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Services
{
    public interface ISprintPlannerService
    {
        Task<GeminiSprintPlanResponseDto> PlanSprintWithAIAsync(
            Guid projectId,
            PlanSprintRequestDto request,
            int userId);
    }
}
