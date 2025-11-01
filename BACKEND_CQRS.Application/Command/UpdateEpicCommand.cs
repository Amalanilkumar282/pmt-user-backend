using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Command
{
    public class UpdateEpicCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int? AssigneeId { get; set; }
        public int? ReporterId { get; set; }
        public List<string>? Labels { get; set; }
    }
}
