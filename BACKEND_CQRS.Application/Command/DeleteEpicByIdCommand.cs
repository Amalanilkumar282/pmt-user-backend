using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;

namespace BACKEND_CQRS.Application.Command
{
    public class DeleteEpicByIdCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid EpicId { get; set; }

        public DeleteEpicByIdCommand(Guid epicId)
        {
            EpicId = epicId;
        }
    }
}
