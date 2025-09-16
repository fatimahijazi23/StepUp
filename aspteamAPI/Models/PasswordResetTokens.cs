using System.ComponentModel.DataAnnotations;

namespace aspteamAPI.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public string Token { get; set; } = string.Empty; // Random secure token
        [Required]
        public DateTime ExpiresAt { get; set; } // Usually 1-2 hours from creation
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsUsed { get; set; } = false; // Prevent token reuse
        public DateTime? UsedAt { get; set; }

        // Navigation property
        public User User { get; set; } = null!;
    }
}
