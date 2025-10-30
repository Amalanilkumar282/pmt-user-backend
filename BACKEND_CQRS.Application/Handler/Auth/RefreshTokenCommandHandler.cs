using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Domain.Services;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Application.Handler.Auth
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<LoginResponseDto>>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;

        public RefreshTokenCommandHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IConfiguration configuration,
            ILogger<RefreshTokenCommandHandler> logger)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Refresh token request received");

                // Find the refresh token
                var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

                if (refreshToken == null)
                {
                    _logger.LogWarning("Refresh token not found");
                    return ApiResponse<LoginResponseDto>.Fail("Invalid refresh token");
                }

                // Check if token is active
                if (!refreshToken.IsActive)
                {
                    _logger.LogWarning("Refresh token is not active. Token: {Token}", request.RefreshToken);
                    return ApiResponse<LoginResponseDto>.Fail("Refresh token has expired or been revoked");
                }

                // Get the user
                var users = await _userRepository.FindAsync(u => u.Id == refreshToken.UserId);
                var user = users.FirstOrDefault();

                if (user == null)
                {
                    _logger.LogWarning("User not found for refresh token. UserId: {UserId}", refreshToken.UserId);
                    return ApiResponse<LoginResponseDto>.Fail("User not found");
                }

                // Check if user is active
                if (user.IsActive == false)
                {
                    _logger.LogWarning("User account is inactive. UserId: {UserId}", user.Id);
                    return ApiResponse<LoginResponseDto>.Fail("Account is inactive");
                }

                // Generate new tokens
                var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
                var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

                // Get token expiration settings
                var refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
                var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

                // Revoke old refresh token
                refreshToken.RevokedAt = DateTimeOffset.UtcNow;
                refreshToken.ReplacedByToken = newRefreshToken;
                await _refreshTokenRepository.UpdateAsync(refreshToken);

                // Save new refresh token
                var newRefreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = newRefreshToken,
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(refreshTokenExpirationDays),
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _refreshTokenRepository.CreateAsync(newRefreshTokenEntity);

                _logger.LogInformation("Refresh token successful for user: {UserId}", user.Id);

                // Return response
                var response = new LoginResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    AccessTokenExpires = DateTimeOffset.UtcNow.AddMinutes(accessTokenExpirationMinutes),
                    RefreshTokenExpires = newRefreshTokenEntity.ExpiresAt,
                    IsActive = user.IsActive ?? false,
                    IsSuperAdmin = user.IsSuperAdmin ?? false
                };

                return ApiResponse<LoginResponseDto>.Success(response, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return ApiResponse<LoginResponseDto>.Fail("An error occurred while refreshing token");
            }
        }
    }
}
