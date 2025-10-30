using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;

namespace BACKEND_CQRS.Application.Command
{
    public class DeleteSprintCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }

        public DeleteSprintCommand(Guid id)
        {
            Id = id;
        }
    }
}