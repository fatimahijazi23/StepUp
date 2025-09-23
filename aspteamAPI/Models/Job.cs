using System.ComponentModel.DataAnnotations;

public class Job
{
    public int Id { get; set; }

    [Required]
    public int PostedBy { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? Location { get; set; }
    public Industry? Industry { get; set; }
    public ExperienceLevel? ExperienceLevel { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    public WorkArrangement? WorkArrangement { get; set; }
    public decimal? MaxSalaryRange { get; set; }
    public decimal? MinSalaryRange { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    public CompanyAccount Company { get; set; } = null!;

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
