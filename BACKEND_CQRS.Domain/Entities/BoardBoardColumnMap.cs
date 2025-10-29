using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BACKEND_CQRS.Domain.Entities
{
    [Table("board_boardcolumn_map")]
    public class BoardBoardColumnMap
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("board_id")]
        public int? BoardId { get; set; }

        [Column("board_column_id")]
        public Guid BoardColumnId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("BoardId")]
        public Board? Board { get; set; }

        [ForeignKey("BoardColumnId")]
        public BoardColumn? BoardColumn { get; set; }
    }
}
