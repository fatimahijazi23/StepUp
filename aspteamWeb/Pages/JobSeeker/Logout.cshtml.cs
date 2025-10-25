using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspteamWeb.Pages.JobSeeker
{
    public class LogoutModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public LogoutModel(HttpClient client)
        {
            _httpClient = client;
        }
        public IActionResult OnGetAsync()
        {
            HttpContext.Session.Clear();

            // Optionally log the logout action using model.UserId

            string url = "https://localhost:7289/api/Auth/logout";
            if (ModelState.IsValid) {  
                using (var client = new HttpClient())
                {
                    var response = client.PostAsync(url, null).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        // Logout successful
                    }
                    else
                    {
                        // Handle unsuccessful logout if necessary
                    }
                }
            }
            return RedirectToPage("/JobSeeker/Login");
        }

    }
}
