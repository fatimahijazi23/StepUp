using System.ComponentModel.DataAnnotations;

namespace aspteamAPI.DTOs
{
    public class UpdateCompanyProfileDTO
    {
        
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public string? About { get; set; }
        public int? CompanySize { get; set; }
        public Industry? Industry { get; set; }
      
        public string? ProfilePictureUrl { get; set; }
    }
}
