namespace aspteamAPI.DTOs
{
    public class JobApplicationResponseDto
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public int JobId { get; set; }
        public int CvId { get; set; }
        public DateTime CreatedAt { get; set; }
        public ApplicationStatusBadge Status { get; set; }
        public JobSeekerBasicInfoDto? Applicant { get; set; }
        public JobBasicInfoDto? Job { get; set; }
        public CvBasicInfoDto? Cv { get; set; }
        public EvaluationBasicInfoDto? Evaluation { get; set; }
    }
}
