namespace aspteamAPI.DTOs
{
    public class CVEvaluationResponseDto
    {
        public int Id { get; set; }
        public int CvId { get; set; }
        public int? Score { get; set; }
        public string? Notes { get; set; }
        public string? CandidateStrengths { get; set; }
        public string? CandidateWeaknesses { get; set; }
        public int? OverallFitRating { get; set; }
        public string? JustificationForRating { get; set; }
    }
}
