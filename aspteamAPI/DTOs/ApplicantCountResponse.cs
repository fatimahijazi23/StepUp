using System.ComponentModel.DataAnnotations;

namespace aspteamAPI.DTOs
{
    public class ApplicantCountResponse
    {
        public int JobId { get; set; }
        public int ApplicantCount { get; set; }
        public bool Success { get; set; }
    }
}
