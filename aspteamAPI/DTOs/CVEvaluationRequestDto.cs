namespace aspteamAPI.DTOs
{
    public class CVEvaluationRequestDto
    {
        public int CVId { get; set; }
        public int JobId { get; set; } // Optional: for job-specific evaluation
    }
}
