using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Query.Teams
{
    public class GetTeamCountByProjectIdQuery : IRequest<int>
    {
        public Guid ProjectId { get; set; }

        public GetTeamCountByProjectIdQuery(Guid projectId)
        {
            ProjectId = projectId;
        }
    }
}
