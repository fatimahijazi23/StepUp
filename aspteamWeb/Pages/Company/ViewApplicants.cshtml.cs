using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aspteamWeb.Pages.Company
{
    public class ViewApplicantsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int JobId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FitStatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public string JobTitle { get; set; } = "Sample Job";
        public List<ApplicantViewModel> Applicants { get; set; } = new();
        public int TotalApplicants => Applicants.Count;

        public void OnGet()
        {
            // Sample data
            Applicants = new List<ApplicantViewModel>
            {
                new ApplicantViewModel { Id = 1, Name = "Alice", FitScore = 8, FitScoreColor="#16a34a", Status="In-progress", CvUrl="/cvs/alice.pdf", AppliedDaysAgo=3 },
                new ApplicantViewModel { Id = 2, Name = "Bob", FitScore = 5, FitScoreColor="#facc15", Status="Reviewed", CvUrl="/cvs/bob.pdf", AppliedDaysAgo=5 }
            };
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int applicantId, int jobId, string status)
        {
            // Update status logic here
            await Task.CompletedTask;
            return RedirectToPage(new { jobId, FitStatusFilter, StatusFilter });
        }

        public class ApplicantViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int FitScore { get; set; }
            public string FitScoreColor { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string CvUrl { get; set; } = string.Empty;
            public int AppliedDaysAgo { get; set; }
        }
    }
}
