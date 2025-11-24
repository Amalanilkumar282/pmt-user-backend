using BACKEND_CQRS.Application.Wrapper;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BACKEND_CQRS.Test.Helpers
{
    /// <summary>
    /// Helper methods for extracting and asserting ApiResponse<T> from controller action results
    /// </summary>
    public static class TestResponseHelper
    {
        /// <summary>
        /// Extracts ApiResponse<T> from ActionResult<ApiResponse<T>>
        /// </summary>
        public static ApiResponse<T> ExtractApiResponse<T>(ActionResult<ApiResponse<T>> actionResult)
        {
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            return Assert.IsType<ApiResponse<T>>(objectResult.Value);
        }

        /// <summary>
        /// Extracts ApiResponse<T> from IActionResult (OkObjectResult, BadRequestObjectResult, etc.)
        /// </summary>
        public static ApiResponse<T> ExtractApiResponse<T>(IActionResult actionResult)
        {
            var objectResult = Assert.IsType<ObjectResult>(actionResult);
            return Assert.IsType<ApiResponse<T>>(objectResult.Value);
        }

        /// <summary>
        /// Extracts ApiResponse<T> from OkObjectResult
        /// </summary>
        public static ApiResponse<T> ExtractFromOkResult<T>(IActionResult actionResult)
        {
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            return Assert.IsType<ApiResponse<T>>(okResult.Value);
        }

        /// <summary>
        /// Extracts ApiResponse<T> from BadRequestObjectResult
        /// </summary>
        public static ApiResponse<T> ExtractFromBadRequestResult<T>(IActionResult actionResult)
        {
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            return Assert.IsType<ApiResponse<T>>(badRequestResult.Value);
        }

        /// <summary>
        /// Extracts ApiResponse<T> from NotFoundObjectResult
        /// </summary>
        public static ApiResponse<T> ExtractFromNotFoundResult<T>(IActionResult actionResult)
        {
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            return Assert.IsType<ApiResponse<T>>(notFoundResult.Value);
        }
    }
}
