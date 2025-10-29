namespace BACKEND_CQRS.Application.Dto
{
    public class CreateBoardColumnResponseDto
    {
        public Guid ColumnId { get; set; }
        public int BoardId { get; set; }
        public string BoardColumnName { get; set; } = string.Empty;
        public string BoardColor { get; set; } = string.Empty;
        public int Position { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public bool IsNewStatus { get; set; }
        public int ShiftedColumnsCount { get; set; }
    }
}
