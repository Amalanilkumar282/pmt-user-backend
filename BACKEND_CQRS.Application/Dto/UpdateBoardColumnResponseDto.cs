namespace BACKEND_CQRS.Application.Dto
{
    /// <summary>
    /// Response DTO for updating a board column
    /// </summary>
    public class UpdateBoardColumnResponseDto
    {
        /// <summary>
        /// The ID of the updated board column
        /// </summary>
        public Guid ColumnId { get; set; }

        /// <summary>
        /// The board ID that contains this column
        /// </summary>
        public int BoardId { get; set; }

        /// <summary>
        /// The updated name of the board column
        /// </summary>
        public string BoardColumnName { get; set; } = string.Empty;

        /// <summary>
        /// The updated color of the board column
        /// </summary>
        public string BoardColor { get; set; } = string.Empty;

        /// <summary>
        /// The updated position of the board column
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// The status ID associated with the column
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// The status name associated with the column
        /// </summary>
        public string StatusName { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if a new status was created during the update
        /// </summary>
        public bool IsNewStatus { get; set; }

        /// <summary>
        /// The previous position of the column (before update)
        /// </summary>
        public int? PreviousPosition { get; set; }

        /// <summary>
        /// The number of columns that were shifted due to position change
        /// </summary>
        public int ShiftedColumnsCount { get; set; }

        /// <summary>
        /// Details of what was updated
        /// </summary>
        public List<string> UpdatedFields { get; set; } = new List<string>();
    }
}
