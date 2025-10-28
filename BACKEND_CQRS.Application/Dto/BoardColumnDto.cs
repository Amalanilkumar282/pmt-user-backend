namespace BACKEND_CQRS.Application.Dto
{
    public class BoardColumnDto
    {
        public Guid Id { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public string? BoardColumnName { get; set; }
        public string? BoardColor { get; set; }
        public int? Position { get; set; }
    }
}
