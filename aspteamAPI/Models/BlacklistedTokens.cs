using System.ComponentModel.DataAnnotations;

namespace aspteamAPI.Models
{
    public class BlacklistedToken
    {
        public int Id { get; set; }
        [Required]
        public string TokenId { get; set; } = string.Empty; // JWT "jti" claim
        [Required]
        public DateTime ExpiresAt { get; set; } // When the original token expires
        public DateTime BlacklistedAt { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; } // Who logged out
        public string Reason { get; set; } = "User Logout"; // Optional: reason for blacklisting
    }
}
