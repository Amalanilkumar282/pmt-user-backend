using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Command
{
    public class DeleteTeamCommand : IRequest<bool>
    {
        public int TeamId { get; set; }

        public DeleteTeamCommand(int teamId)
        {
            TeamId = teamId;
        }
    }
}
