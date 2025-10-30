namespace BACKEND_CQRS.Domain.Services
{
    public interface IPasswordHashService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}
