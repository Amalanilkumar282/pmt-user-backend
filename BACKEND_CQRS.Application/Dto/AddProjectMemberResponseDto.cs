namespace BACKEND_CQRS.Application.Dto
{
    /// <summary>
    /// Response DTO for adding a project member
    /// </summary>
    public class AddProjectMemberResponseDto
    {
        public int MemberId { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        /// <summary>
        /// Indicates if this user is a project owner (has admin access)
        /// Same as IsProjectManager - this field determines admin privileges
        /// </summary>
        public bool IsOwner { get; set; }
        public DateTimeOffset AddedAt { get; set; }
        public int? AddedBy { get; set; }
        public string? AddedByName { get; set; }
    }
}
