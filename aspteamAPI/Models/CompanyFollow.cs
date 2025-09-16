namespace aspteamAPI.Models
{
    public class CompanyFollow
    {
        public int Id { get; set; }
        public int JobSeekerId { get; set; }
        public int CompanyId { get; set; }
        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public JobSeekerAccount JobSeeker { get; set; } = null!;
        public CompanyAccount Company { get; set; } = null!;
    }
}
