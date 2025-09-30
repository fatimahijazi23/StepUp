using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspteamWeb.Pages.JobSeeker
{
    public class ProfileModel : PageModel
    {
        // Displayed profile data
        public string UserName { get; set; } = "rasha";
        public string Email { get; set; } = "tr@gmail.com";
        public string? About { get; set; } = null;
        public int FollowingCount { get; set; } = 0;

        [BindProperty]
        public ProfileInput Input { get; set; } = new ProfileInput();

        public class ProfileInput
        {
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? About { get; set; }
        }

        public void OnGet()
        {
            // Load user profile (mocked for now)
            Input.UserName = UserName;
            Input.Email = Email;
            Input.About = About;
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            // Simulate saving to DB
            UserName = Input.UserName;
            Email = Input.Email;
            About = Input.About;

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToPage();
        }
    }
}
