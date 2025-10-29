namespace BACKEND_CQRS.Application.Dto
{
    public class DeleteBoardColumnResponseDto
    {
        public Guid ColumnId { get; set; }
        public int BoardId { get; set; }
        public string BoardColumnName { get; set; } = string.Empty;
        public int Position { get; set; }
        public int ReorderedColumnsCount { get; set; }
        public bool WasDeleted { get; set; }
    }
}
