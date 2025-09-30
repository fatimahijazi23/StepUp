using System.ComponentModel.DataAnnotations;


public class CompanyAccount
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public string? CompanyName { get; set; }
    public string? About { get; set; }
    public int? CompanySize { get; set; }
    public Industry? Industry { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ProfilePictureUrl { get; set; }

    public User User { get; set; } = null!;

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
}
