using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BACKEND_CQRS.Application.Handlers.TeamHandlers
{
    public class DeleteTeamCommandHandler : IRequestHandler<DeleteTeamCommand, bool>
    {
        private readonly ITeamRepository _teamRepository;

        public DeleteTeamCommandHandler(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<bool> Handle(DeleteTeamCommand request, CancellationToken cancellationToken)
        {
            // You can choose soft delete or hard delete
            return await _teamRepository.DeleteTeamAsync(request.TeamId);
        }
    }
}
