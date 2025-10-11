using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspteamWeb.Pages.JobSeeker
{
 
    namespace aspteamWeb.Pages.JobSeeker
    {



        public class FormModel : PageModel
        {
            [BindProperty]
            public FormInput Input { get; set; } = new FormInput();

            public class FormInput
            {
                public string Title { get; set; }
                public string CompanyName { get; set; }
                public string Location { get; set; }
                public string Description { get; set; }
                public string JobType { get; set; }
                public string Industry { get; set; }
                public DateTime PostedDate { get; set; }
                public string SalaryRange { get; set; }
            }

            public void OnGet()
            {
                // default date is today
                Input.PostedDate = DateTime.Now;
            }

            public IActionResult OnPost()
            {
                if (!ModelState.IsValid)
                    return Page();

                // TODO: Save Input to database or call an API

                TempData["Success"] = "Job created successfully!";
                return RedirectToPage("/JobSeeker/JobForm");
            }
        }
    }
}
