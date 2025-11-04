using BACKEND_CQRS.Domain.Dto.AI;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Services
{
    public interface IGeminiAIService
    {
        Task<GeminiSprintPlanResponseDto> GenerateSprintPlanAsync(SprintPlanningContextDto context);
    }
}
