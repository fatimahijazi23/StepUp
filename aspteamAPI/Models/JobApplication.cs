using System.ComponentModel.DataAnnotations;

public class JobApplication
{
    public int Id { get; set; }

    [Required]
    public int ApplicantId { get; set; }

    [Required]
    public int JobId { get; set; }

    [Required]
    public int CvId { get; set; }

    public DateTime CreatedAt { get; set; }
    public ApplicationStatusBadge Status { get; set; }

    public JobSeekerAccount Applicant { get; set; } = null!;
    public Job Job { get; set; } = null!;
    public CV Cv { get; set; } = null!;

    public Evaluation? Evaluation { get; set; }
}
