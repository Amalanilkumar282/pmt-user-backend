using BCrypt.Net;

namespace BACKEND_CQRS.Infrastructure.Helpers
{
    /// <summary>
    /// Helper class for testing password hashing
    /// This can be used to generate BCrypt hashes for testing purposes
    /// </summary>
    public static class PasswordHashHelper
    {
        /// <summary>
        /// Generates a BCrypt hash for a given password
        /// Use this for creating test users or debugging
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>BCrypt hash string</returns>
        public static string GenerateHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        /// <summary>
        /// Verifies if a password matches a hash
        /// Use this for debugging authentication issues
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="hash">BCrypt hash</param>
        /// <returns>True if password matches, false otherwise</returns>
        public static bool VerifyHash(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Example usage - can be called from Program.cs during development
        /// </summary>
        public static void PrintExampleHashes()
        {
            var testPasswords = new[]
            {
                "Admin@123",
                "User@123",
                "Test@123"
            };

            Console.WriteLine("=== BCrypt Password Hashes (Work Factor: 12) ===");
            Console.WriteLine();

            foreach (var password in testPasswords)
            {
                var hash = GenerateHash(password);
                Console.WriteLine($"Password: {password}");
                Console.WriteLine($"Hash:     {hash}");
                Console.WriteLine($"Verified: {VerifyHash(password, hash)}");
                Console.WriteLine();
            }
        }
    }
}
