using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BACKEND_CQRS.Application.Command
{
    public class UploadFileCommand : IRequest<ApiResponse<string>>
    {
        public IFormFile File { get; set; }
        public string BucketName { get; set; } = "attachments";
    }
}
