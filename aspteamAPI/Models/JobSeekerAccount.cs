using System.ComponentModel.DataAnnotations;

public class JobSeekerAccount
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }

    public User User { get; set; } = null!;

    public ICollection<CV> CVs { get; set; } = new List<CV>();
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    public ICollection<Follow> Follows { get; set; } = new List<Follow>();
}
