

namespace aspteamAPI.DTOs
{
    public class UpdateJobSeekerProfileDTO 
    {
            public int Id {  get; set; }
            public string? Name { get; set; }  // References User table
                                               // jobseeker.User.email
            public string ? Email { get; set; }  // References User table

            public string? Bio { get; set; }
            public string? ProfilePictureUrl { get; set; }

            
        }
    }
