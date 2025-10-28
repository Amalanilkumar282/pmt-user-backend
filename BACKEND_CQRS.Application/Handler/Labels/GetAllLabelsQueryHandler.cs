using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Labels
{
    public class GetAllLabelsQueryHandler : IRequestHandler<GetAllLabelsQuery, ApiResponse<List<LabelDto>>>
    {
        private readonly ILabelRepository _labelRepository;
        private readonly IMapper _mapper;

        public GetAllLabelsQueryHandler(ILabelRepository labelRepository, IMapper mapper)
        {
            _labelRepository = labelRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<LabelDto>>> Handle(GetAllLabelsQuery request, CancellationToken cancellationToken)
        {
            var labels = await _labelRepository.GetAllAsync();
            var dtoList = _mapper.Map<List<LabelDto>>(labels);
            return ApiResponse<List<LabelDto>>.Success(dtoList, "Labels fetched successfully");
        }
    }
}
