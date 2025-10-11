using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspteamWeb.Pages.Company
{
    public class JobDetailsModel : PageModel
    {
        public JobDetails? Job { get; set; }
        public int ApplicantCount { get; set; }

        public IActionResult OnGet(int id)
        {
            // Simulated database lookup (replace with your DB logic)
            var fakeJobs = new List<JobDetails>
            {
                new JobDetails
                {
                    Id = 1,
                    JobTitle = "Senior Software Engineer",
                    CompanyName = "TechCorp",
                    Location = "San Francisco, CA",
                    JobType = "Full-time",
                    Industry = "Technology",
                    SalaryRange = "$120,000 - $160,000",
                    JobDescription = "We are looking for a Senior Software Engineer to join our growing team.",
                    Requirements = "Bachelor's degree in Computer Science, 5+ years of experience with React and Node.js.",
                    Benefits = "Health insurance, flexible PTO, remote work options.",
                    PostedDaysAgo = 2
                }
            };

            Job = fakeJobs.FirstOrDefault(j => j.Id == id);

            if (Job == null)
                return NotFound();

            // Simulated applicant count
            ApplicantCount = new Random().Next(1, 10);

            return Page();
        }

        public class JobDetails
        {
            public int Id { get; set; }
            public string JobTitle { get; set; } = string.Empty;
            public string CompanyName { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public string JobType { get; set; } = string.Empty;
            public string Industry { get; set; } = string.Empty;
            public string SalaryRange { get; set; } = string.Empty;
            public string JobDescription { get; set; } = string.Empty;
            public string Requirements { get; set; } = string.Empty;
            public string Benefits { get; set; } = string.Empty;
            public int PostedDaysAgo { get; set; }
        }
    }
}
