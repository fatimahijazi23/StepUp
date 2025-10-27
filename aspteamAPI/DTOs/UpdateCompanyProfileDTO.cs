using System.Text.Json.Serialization;

namespace aspteamAPI.DTOs
{
    public class UpdateCompanyProfileDTO
    {
        public int Id { get; set; }

        // Support both Name and CompanyName for compatibility
        [JsonPropertyName("name")]
        public string? Name
        {
            get => CompanyName;
            set => CompanyName = value;
        }

        [JsonIgnore]
        public string? CompanyName { get; set; }

        public string? Email { get; set; }

        // Support both LogoUrl and ProfilePictureUrl
        [JsonPropertyName("logoUrl")]
        public string? LogoUrl
        {
            get => ProfilePictureUrl;
            set => ProfilePictureUrl = value;
        }

        [JsonIgnore]
        public string? ProfilePictureUrl { get; set; }

        public Industry? Industry { get; set; }
        public int? CompanySize { get; set; }
        public string? About { get; set; }
    }
}