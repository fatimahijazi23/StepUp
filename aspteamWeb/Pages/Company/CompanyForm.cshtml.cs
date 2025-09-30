using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspteamWeb.Pages.Company
{
        public class CompanyFormModel : PageModel
        {
            public List<JobPostingDto> JobPostings { get; set; } = new();

            public class JobPostingDto
            {
                public int Id { get; set; }
                public string Title { get; set; } = string.Empty;
                public string Location { get; set; } = string.Empty;
                public string Type { get; set; } = "Full-time";
                public int ApplicantsCount { get; set; }
                public DateTime PostedDate { get; set; } = DateTime.Now;
                public string Status { get; set; } = "Active";
            }

            public void OnGet()
            {
                // Mock data – replace with DB or API call later
                JobPostings = new List<JobPostingDto>
            {
                new JobPostingDto
                {
                    Id = 1,
                    Title = "Sample Software Engineer",
                    Location = "Remote",
                    Type = "Full-time",
                    ApplicantsCount = 2,
                    PostedDate = DateTime.Now,
                    Status = "Active"
                }
            };
            }

            public IActionResult OnPostCloseJob(int id)
            {
                // TODO: Update DB to mark job as closed
                var job = JobPostings.FirstOrDefault(j => j.Id == id);
                if (job != null)
                {
                    job.Status = "Closed";
                }

                TempData["Success"] = "Job closed successfully!";
                return RedirectToPage();
            }
        }
    }

