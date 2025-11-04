using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System.Text.Json.Serialization;

namespace BACKEND_CQRS.Application.Command
{
    public class UpdateIssueDatesCommand : IRequest<ApiResponse<Guid>>
    {
        [JsonIgnore] // Don't expect this from request body
        public Guid IssueId { get; set; }
        
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? DueDate { get; set; }
    }
}