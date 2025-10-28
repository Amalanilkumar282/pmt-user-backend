using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Infrastructure.Services;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Files
{
    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, ApiResponse<string>>
    {
        private readonly ISupabaseStorageService _storageService;

        public UploadFileCommandHandler(ISupabaseStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<ApiResponse<string>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
                return ApiResponse<string>.Fail("No file provided");

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}_{request.File.FileName}";

            using var stream = request.File.OpenReadStream();
            var url = await _storageService.UploadFileAsync(stream, fileName, request.BucketName);

            return ApiResponse<string>.Success(url, "File uploaded successfully");
        }
    }
}
