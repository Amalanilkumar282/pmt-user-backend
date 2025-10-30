namespace BACKEND_CQRS.Application.Dto
{
    /// <summary>
    /// Response DTO for updating a board
    /// </summary>
    public class UpdateBoardResponseDto
    {
        /// <summary>
        /// The ID of the updated board
        /// </summary>
        public int BoardId { get; set; }

        /// <summary>
        /// The project ID that contains this board
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// The project name
        /// </summary>
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>
        /// The team ID associated with this board (null if not team-based)
        /// </summary>
        public int? TeamId { get; set; }

        /// <summary>
        /// The team name (null if not team-based)
        /// </summary>
        public string? TeamName { get; set; }

        /// <summary>
        /// The updated board name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The updated board description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The updated board type
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the board is team-based
        /// </summary>
        public bool IsTeamBased { get; set; }

        /// <summary>
        /// Indicates if the board is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The user ID who updated the board
        /// </summary>
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// The name of the user who updated the board
        /// </summary>
        public string? UpdatedByName { get; set; }

        /// <summary>
        /// The date and time when the board was updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// The board metadata
        /// </summary>
        public string? Metadata { get; set; }

        /// <summary>
        /// Previous values of updated fields
        /// </summary>
        public Dictionary<string, string> PreviousValues { get; set; } = new();

        /// <summary>
        /// List of fields that were updated
        /// </summary>
        public List<string> UpdatedFields { get; set; } = new();

        /// <summary>
        /// Indicates if team association was removed
        /// </summary>
        public bool TeamAssociationRemoved { get; set; }

        /// <summary>
        /// Indicates if team association was added
        /// </summary>
        public bool TeamAssociationAdded { get; set; }
    }
}
