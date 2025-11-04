using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Epic
{
    public class UpdateEpicDatesCommandHandler : IRequestHandler<UpdateEpicDatesCommand, ApiResponse<Guid>>
    {
        private readonly IEpicRepository _epicRepository;

        public UpdateEpicDatesCommandHandler(IEpicRepository epicRepository)
        {
            _epicRepository = epicRepository;
        }

        public async Task<ApiResponse<Guid>> Handle(UpdateEpicDatesCommand request, CancellationToken cancellationToken)
        {
            var epics = await _epicRepository.FindAsync(e => e.Id == request.EpicId);
            var epic = epics.FirstOrDefault();

            if (epic == null)
                return ApiResponse<Guid>.Fail("Epic not found");

            // Update dates
            if (request.StartDate.HasValue)
                epic.StartDate = request.StartDate;

            if (request.DueDate.HasValue)
                epic.DueDate = request.DueDate;

            // Validate that StartDate is before DueDate if both are provided
            if (epic.StartDate.HasValue && epic.DueDate.HasValue && epic.StartDate > epic.DueDate)
                return ApiResponse<Guid>.Fail("Start date cannot be after due date");

            epic.UpdatedAt = DateTimeOffset.UtcNow;

            await _epicRepository.UpdateAsync(epic);
            return ApiResponse<Guid>.Success(epic.Id, "Epic dates updated successfully");
        }
    }
}