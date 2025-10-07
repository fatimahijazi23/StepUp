using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspteamWeb.Pages.JobSeeker
{ 
    public class ResumeTestModel : PageModel
    {
        [BindProperty]
        public IFormFile? ResumeFile { get; set; }

        [BindProperty]
        public string JobDescription { get; set; } = string.Empty;

        public string? ResultMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ResumeFile == null || ResumeFile.Length == 0)
            {
                ResultMessage = "⚠️ Please upload a resume file.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(JobDescription))
            {
                ResultMessage = "⚠️ Please enter a job description.";
                return Page();
            }

            // 🔹 Here you can implement your API call / AI analysis logic
            // For now, just mock the response
            await Task.Delay(500); // simulate processing
            ResultMessage = $"✅ Your resume '{ResumeFile.FileName}' was analyzed against the job description.";

            return Page();
        }
    }
}
