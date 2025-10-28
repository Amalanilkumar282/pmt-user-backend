using AutoMapper;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Labels
{
    public class EditLabelCommandHandler : IRequestHandler<EditLabelCommand, ApiResponse<int>>
    {
        private readonly ILabelRepository _labelRepository;
        private readonly IMapper _mapper;

        public EditLabelCommandHandler(ILabelRepository labelRepository, IMapper mapper)
        {
            _labelRepository = labelRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<int>> Handle(EditLabelCommand request, CancellationToken cancellationToken)
        {
            var label = await _labelRepository.GetByIdAsync(request.Id);
            if (label == null)
                return ApiResponse<int>.Fail("Label not found");

            label.Name = request.Name;
            label.Colour = request.Colour;
            await _labelRepository.UpdateAsync(label);
            return ApiResponse<int>.Success(label.Id, "Label updated successfully");
        }
    }
}
