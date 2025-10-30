namespace BACKEND_CQRS.Application.Dto
{
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTimeOffset AccessTokenExpires { get; set; }
        public DateTimeOffset RefreshTokenExpires { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuperAdmin { get; set; }
    }
}
