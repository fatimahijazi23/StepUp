using System.ComponentModel.DataAnnotations;

namespace aspteamAPI.DTOs
{
    public class CreateJobDto
    {
        [Required]
        public int PostedBy { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        public string? Requirements { get; set; }
        public string? Location { get; set; }
        public Industry? Industry { get; set; }
        public ExperienceLevel? ExperienceLevel { get; set; }
        public EmploymentType? EmploymentType { get; set; }
        public WorkArrangement? WorkArrangement { get; set; }
        public decimal? MaxSalaryRange { get; set; }
        public decimal? MinSalaryRange { get; set; }
    }
}
