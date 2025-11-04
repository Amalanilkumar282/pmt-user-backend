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
    public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponseDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IJwtTokenService jwtTokenService,
            IPasswordHashService passwordHashService,
            IConfiguration configuration,
            ILogger<LoginCommandHandler> logger)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtTokenService = jwtTokenService;
            _passwordHashService = passwordHashService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", request.Email);

                // Find user by email
                var users = await _userRepository.FindAsync(u => u.Email != null && u.Email.ToLower() == request.Email.ToLower());
                var user = users.FirstOrDefault();

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found with email {Email}", request.Email);
                    return ApiResponse<LoginResponseDto>.Fail("Invalid email or password");
                }

                // Check if user is active
                if (user.IsActive == false)
                {
                    _logger.LogWarning("Login failed: User account is inactive for email {Email}", request.Email);
                    return ApiResponse<LoginResponseDto>.Fail("Account is inactive. Please contact administrator.");
                }

                // Verify password
                if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHashService.VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: Invalid password for email {Email}", request.Email);
                    return ApiResponse<LoginResponseDto>.Fail("Invalid email or password");
                }

                // Generate tokens
                var accessToken = _jwtTokenService.GenerateAccessToken(user);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();

                // Get token expiration settings
                var refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
                var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

                // Revoke old refresh tokens for this user
                await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id);

                // Save new refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpirationDays),
                    CreatedAt = DateTime.UtcNow
                };

                await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

                // Update last login
                user.LastLogin = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Login successful for user: {UserId}, Email: {Email}", user.Id, user.Email);

                // Return response
                var response = new LoginResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    Name = user.Name ?? string.Empty,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AccessTokenExpires = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
                    RefreshTokenExpires = refreshTokenEntity.ExpiresAt,
                    IsActive = user.IsActive ?? false,
                    IsSuperAdmin = user.IsSuperAdmin ?? false
                };

                return ApiResponse<LoginResponseDto>.Success(response, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return ApiResponse<LoginResponseDto>.Fail("An error occurred during login. Please try again.");
            }
        }
    }
}
