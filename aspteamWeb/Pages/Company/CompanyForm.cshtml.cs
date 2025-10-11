using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspteamWeb.Pages.JobSeeker
{
    public class CompanyFormModel : PageModel
    {
        public List<JobPosting> Jobs { get; set; } = new();

        public void OnGet()
        {
            // In real scenario, replace this with database fetching (e.g. from EF Core)
            Jobs = new List<JobPosting>
            {
                new JobPosting
                {
                    Id = 1,
                    Title = "Sample Software Engineer",
                    Location = "Remote",
                    Type = "Full-time",
                    ApplicantCount = 2,
                    PostedAt = DateTime.Now.AddDays(-1),
                    Status = "Active"
                },
                new JobPosting
                {
                    Id = 2,
                    Title = "Frontend Developer",
                    Location = "New York, NY",
                    Type = "Part-time",
                    ApplicantCount = 5,
                    PostedAt = DateTime.Now.AddDays(-3),
                    Status = "Active"
                }
            };
        }

        public class JobPosting
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public int ApplicantCount { get; set; }
            public DateTime PostedAt { get; set; }
            public string Status { get; set; } = "Active";
        }
    }
}
