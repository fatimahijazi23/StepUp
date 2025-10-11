using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace aspteamWeb.Pages.Company
{
    public class ProfileCompanyModel : PageModel
    {
        [BindProperty]
        public CompanyViewModel Company { get; set; } = new();

        public int FollowersCount { get; set; } = 0; // Replace with real followers logic

        [TempData]
        public string? SuccessMessage { get; set; }

        // GET request
        public void OnGet()
        {
            // Example: Populate with company data (replace with DB fetch)
            Company = new CompanyViewModel
            {
                Name = "Acme Corp",
                Email = "info@acme.com",
                Industry = "Technology",
                CompanySize = "50-100",
                About = "We build innovative solutions."
            };

            FollowersCount = 1250; // Example
        }

        // POST request
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page(); // Show validation errors
            }

            // TODO: Save updated Company data to the database here

            SuccessMessage = "Company profile updated successfully!";
            return RedirectToPage(); // Refresh page to show success message
        }

        public class CompanyViewModel
        {
            [Required(ErrorMessage = "Company name is required.")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Industry is required.")]
            public string Industry { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Invalid email address.")]
            public string Email { get; set; } = string.Empty;

            public string CompanySize { get; set; } = string.Empty;

            public string About { get; set; } = string.Empty;
        }
    }
}
