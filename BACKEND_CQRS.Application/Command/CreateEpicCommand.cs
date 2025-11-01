using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;

namespace BACKEND_CQRS.Application.Command
{
    public class CreateEpicCommand : IRequest<ApiResponse<CreateEpicDto>>
    {
        public Guid ProjectId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int? AssigneeId { get; set; }
        public int? ReporterId { get; set; }
        public List<string>? Labels { get; set; }
    }
}
