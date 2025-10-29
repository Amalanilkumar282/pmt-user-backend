using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace BACKEND_CQRS.Application.Command
{
    public class CreateBoardColumnCommand : IRequest<ApiResponse<CreateBoardColumnResponseDto>>
    {
        [Required(ErrorMessage = "Board ID is required")]
        public int BoardId { get; set; }

        [Required(ErrorMessage = "Board column name is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Board column name must be between 1 and 255 characters")]
        public string BoardColumnName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Board color is required")]
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", 
            ErrorMessage = "Board color must be a valid hex color (e.g., #FF5733 or #F57)")]
        public string BoardColor { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Status name must be between 1 and 100 characters")]
        public string StatusName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Position is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Position must be greater than 0")]
        public int Position { get; set; }
    }
}
