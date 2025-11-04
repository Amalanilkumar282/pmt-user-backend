using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System.Text.Json.Serialization;

namespace BACKEND_CQRS.Application.Command
{
    public class UpdateEpicDatesCommand : IRequest<ApiResponse<Guid>>
    {
        [JsonIgnore] // Don't expect this from request body
        public Guid EpicId { get; set; }
        
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
    }
}