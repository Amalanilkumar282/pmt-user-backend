namespace BACKEND_CQRS.Application.Dto
{
    public class BoardWithColumnsDto
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = "kanban";
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public int? UpdatedBy { get; set; }
        public string? UpdatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Metadata { get; set; }
        public List<BoardColumnDto> Columns { get; set; } = new();
    }
}
