using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace aspteamWeb.Pages.Company
{
    public class CreateJobModel : PageModel
    {
        [BindProperty]
        public JobInputModel Input { get; set; } = new();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            // TODO: Save the job to the database
            TempData["Success"] = $"Job '{Input.JobTitle}' created successfully!";

            // Redirect back to dashboard
            return RedirectToPage("/Company/CompanyDashboard");
        }

        public class JobInputModel
        {
            [Required, StringLength(100)]
            public string JobTitle { get; set; } = string.Empty;

            [Required, StringLength(100)]
            public string Location { get; set; } = string.Empty;

            [Required]
            public string JobType { get; set; } = string.Empty;

            [Required]
            public string Industry { get; set; } = string.Empty;

            [Required, StringLength(1000)]
            public string JobDescription { get; set; } = string.Empty;

            [Required, StringLength(1000)]
            public string Requirements { get; set; } = string.Empty;

            [StringLength(1000)]
            public string Benefits { get; set; } = string.Empty;

            [Required, StringLength(50)]
            public string SalaryRange { get; set; } = string.Empty;
        }
    }
}
