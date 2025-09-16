namespace aspteamAPI.DTOs
{

    // DTOs/JobSeekerDtos.cs
    namespace aspteamAPI.DTOs
    {
        public class FollowCompanyDto
        {
            public int CompanyId { get; set; }
        }

        public class FollowedCompanyDto
        {
            public int Id { get; set; }
            public string CompanyName { get; set; } = string.Empty;
            public string? About { get; set; }
            public string? Industry { get; set; }
            public DateTime FollowedAt { get; set; }
        }

    }
}
