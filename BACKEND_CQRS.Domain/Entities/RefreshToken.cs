using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BACKEND_CQRS.Domain.Entities
{
    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("user_id")]
        [Required]
        public int UserId { get; set; }

        [Column("token")]
        [Required]
        [MaxLength(500)]
        public string Token { get; set; }

        [Column("expires_at")]
        [Required]
        public DateTimeOffset ExpiresAt { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Column("revoked_at")]
        public DateTimeOffset? RevokedAt { get; set; }

        [Column("replaced_by_token")]
        [MaxLength(500)]
        public string? ReplacedByToken { get; set; }

        [NotMapped]
        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;

        [NotMapped]
        public bool IsRevoked => RevokedAt != null;

        [NotMapped]
        public bool IsActive => !IsRevoked && !IsExpired;

        // Navigation property
        [ForeignKey("UserId")]
        public Users User { get; set; }
    }
}
