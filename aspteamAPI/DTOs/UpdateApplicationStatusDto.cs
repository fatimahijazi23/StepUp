using System.ComponentModel.DataAnnotations;

namespace aspteamAPI.DTOs
{
    public class UpdateApplicationStatusDto
    {
        [Required]
        public ApplicationStatusBadge Status { get; set; }
        public string? Notes { get; set; }
    }
}
