using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;

namespace BACKEND_CQRS.Application.Command
{
    public class CreateLabelCommand : IRequest<ApiResponse<int>>
    {
        public string Name { get; set; }
        public string Colour { get; set; }
    }
}
