namespace aspteamAPI.DTOs
{
    public class CvBasicInfoDto
    {
        public int Id { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
}
