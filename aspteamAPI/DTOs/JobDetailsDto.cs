namespace aspteamAPI.DTOs
{
    public class JobDetailsDto : JobListDto
    {
        public string Requirements { get; set; }
        public Industry? Industry { get; set; }
        public ExperienceLevel? ExperienceLevel { get; set; }
        public EmploymentType? EmploymentType { get; set; }
        public WorkArrangement? WorkArrangement { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
