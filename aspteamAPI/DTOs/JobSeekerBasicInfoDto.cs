namespace aspteamAPI.DTOs
{
    public class JobSeekerBasicInfoDto
    {
        public int Id { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public UserBasicInfoDto? User { get; set; }
    }
}
