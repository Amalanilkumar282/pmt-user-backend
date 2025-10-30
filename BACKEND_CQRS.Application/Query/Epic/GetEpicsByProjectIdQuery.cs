using BACKEND_CQRS.Application.Dto;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Query.Epic
{
    public class GetEpicsByProjectIdQuery : IRequest<List<EpicDto>>
    {
        public Guid ProjectId { get; }

        public GetEpicsByProjectIdQuery(Guid projectId)
        {
            ProjectId = projectId;
        }
    }
}
