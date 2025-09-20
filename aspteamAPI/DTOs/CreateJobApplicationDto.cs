using System.ComponentModel.DataAnnotations;

namespace aspteamAPI.DTOs
{
    public class CreateJobApplicationDto
    {
        [Required]
        public int JobId { get; set; }
        [Required]
        public int CvId { get; set; }
    }
}
