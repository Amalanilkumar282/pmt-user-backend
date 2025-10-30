using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ApiResponse<string>> UploadFile(IFormFile file, string bucketName = "attachments")
        {
            var command = new UploadFileCommand 
            { 
                File = file,
                BucketName = bucketName
            };
            var result = await _mediator.Send(command);
            return result;
        }
    }
}
