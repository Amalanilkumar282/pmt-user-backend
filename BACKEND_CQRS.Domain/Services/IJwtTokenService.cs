using BACKEND_CQRS.Domain.Entities;

namespace BACKEND_CQRS.Domain.Services
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(Users user);
        string GenerateRefreshToken();
        int? ValidateToken(string token);
    }
}
