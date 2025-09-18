using System.ComponentModel.DataAnnotations;

namespace aspteamAPI.DTOs
{
    public class CvDTO
    {
        public int JobSeekerId { get; set; }

        public string FileUrl { get; set; } 

        public DateTime UploadedAt { get; set; }
    }
}
