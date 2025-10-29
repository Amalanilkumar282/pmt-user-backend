namespace BACKEND_CQRS.Application.Dto
{
    /// <summary>
    /// Response DTO for board creation
    /// </summary>
    public class CreateBoardResponseDto
    {
        public int BoardId { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsTeamBased { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Metadata { get; set; }
    }
}
