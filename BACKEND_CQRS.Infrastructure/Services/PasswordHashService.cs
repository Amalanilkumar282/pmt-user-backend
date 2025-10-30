using BACKEND_CQRS.Domain.Services;
using BCrypt.Net;

namespace BACKEND_CQRS.Infrastructure.Services
{
    public class PasswordHashService : IPasswordHashService
    {
        /// <summary>
        /// Hashes a password using BCrypt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>BCrypt hashed password</returns>
        public string HashPassword(string password)
        {
            // Hash password using BCrypt with work factor 12 (default)
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        /// <summary>
        /// Verifies a password against a BCrypt hash
        /// </summary>
        /// <param name="password">Plain text password to verify</param>
        /// <param name="passwordHash">BCrypt hash to verify against</param>
        /// <returns>True if password matches hash, false otherwise</returns>
        public bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                // Verify password using BCrypt
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch
            {
                // Return false if hash is invalid or verification fails
                return false;
            }
        }
    }
}
