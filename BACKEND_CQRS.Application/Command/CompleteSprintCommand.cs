using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;

namespace BACKEND_CQRS.Application.Command
{
    public class CompleteSprintCommand : IRequest<ApiResponse<bool>>
    {
        public Guid SprintId { get; set; }

        public CompleteSprintCommand(Guid sprintId)
        {
            SprintId = sprintId;
        }
    }
}