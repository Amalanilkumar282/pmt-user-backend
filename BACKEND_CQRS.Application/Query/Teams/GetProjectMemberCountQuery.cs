using BACKEND_CQRS.Application.Dto;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Query.Teams
{
    public class GetProjectMemberCountQuery : IRequest<ProjectMemberCountDto>
    {
        public Guid ProjectId { get; }

        public GetProjectMemberCountQuery(Guid projectId)
        {
            ProjectId = projectId;
        }
    }
}
