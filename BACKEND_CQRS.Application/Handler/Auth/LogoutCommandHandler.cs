using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Application.Handler.Auth
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResponse<bool>>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<LogoutCommandHandler> _logger;

        public LogoutCommandHandler(
            IRefreshTokenRepository refreshTokenRepository,
            ILogger<LogoutCommandHandler> logger)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Logout request for user: {UserId}", request.UserId);

                // Revoke all refresh tokens for the user
                await _refreshTokenRepository.RevokeAllUserTokensAsync(request.UserId);

                _logger.LogInformation("Logout successful for user: {UserId}", request.UserId);

                return ApiResponse<bool>.Success(true, "Logout successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user: {UserId}", request.UserId);
                return ApiResponse<bool>.Fail("An error occurred during logout");
            }
        }
    }
}
