using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Epic
{
    public class DeleteEpicCommandHandler : IRequestHandler<DeleteEpicByIdCommand, ApiResponse<Guid>>
    {
        private readonly IEpicRepository _epicRepository;

        public DeleteEpicCommandHandler(IEpicRepository epicRepository)
        {
            _epicRepository = epicRepository;
        }

        public async Task<ApiResponse<Guid>> Handle(DeleteEpicByIdCommand request, CancellationToken cancellationToken)
        {
            var epics = await _epicRepository.FindAsync(e => e.Id == request.EpicId);
            var epic = epics.FirstOrDefault();

            if (epic == null)
                return ApiResponse<Guid>.Fail("Epic not found");

            await _epicRepository.DeleteAsync(epic);
            return ApiResponse<Guid>.Success(request.EpicId, "Epic deleted successfully");
        }
    }
}
