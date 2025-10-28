using MediatR;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Query
{
    public class GetAllLabelsQuery : IRequest<ApiResponse<List<LabelDto>>>
    {
    }
}
