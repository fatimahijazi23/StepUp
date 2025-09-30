using System.ComponentModel.DataAnnotations;

public class Evaluation
{
    public int Id { get; set; }

    [Required]
    public int JobApplicationId { get; set; }

    [Required]
    public int CvId { get; set; }

    public int? Score { get; set; }
    public string? Notes { get; set; }
    public string? CandidateStrengths { get; set; }
    public string? CandidateWeaknesses { get; set; }
    public int? RiskScore { get; set; }
    public string? RiskExplanation { get; set; }
    public int? RewardScore { get; set; }
    public string? RewardExplanation { get; set; }
    public int? OverallFitRating { get; set; }
    public string? JustificationForRating { get; set; }

    public JobApplication JobApplication { get; set; } = null!;
    public CV Cv { get; set; } = null!;
}
