using System.ComponentModel.DataAnnotations;

public class CV
{
    public int Id { get; set; }

    [Required]
    public int JobSeekerId { get; set; }

    [Required]
    public string FileUrl { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }

    public JobSeekerAccount JobSeeker { get; set; } = null!;

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
}
