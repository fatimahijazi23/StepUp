using System.ComponentModel.DataAnnotations;

public class Follow
{
    public int Id { get; set; }

    [Required]
    public int JobSeekerId { get; set; }

    [Required]
    public int CompanyId { get; set; }

    public DateTime FollowedAt { get; set; }

    public JobSeekerAccount JobSeeker { get; set; } = null!;
    public CompanyAccount Company { get; set; } = null!;
}
