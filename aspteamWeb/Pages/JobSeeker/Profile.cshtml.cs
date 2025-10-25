using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static aspteamWeb.Pages.JobSeeker.JobSeekerDashboardModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace aspteamWeb.Pages.JobSeeker
{
    public class ProfileModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ProfileModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public class ProfileResponse
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string? Bio { get; set; }
            public string ProfilePictureUrl { get; set; }
        }

        [BindProperty]
        public ProfileInput Input { get; set; } = new ProfileInput();

        public class ProfileInput
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? Bio { get; set; }
            public string ProfilePictureUrl { get; set; } = string.Empty;
            public int FollowCount { get; set; }
        }

        public class FollowedCompanyResponse
        {
            public int Id { get; set; }
            public string CompanyName { get; set; } = string.Empty;
            public string? About { get; set; }
            public string? Industry { get; set; }
            public DateTime FollowedAt { get; set; }
          
        }

      
        public bool ShowFollowing { get; set; } = false;

        [BindProperty(SupportsGet = true)]
        public List<FollowedCompanyResponse> FollowedCompanies { get; set; } = new List<FollowedCompanyResponse>();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("JWT token missing. Redirecting to login.");
                    return RedirectToPage("/JobSeeker/Login");
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var apiUrl = "https://localhost:7289/api/JobSeekerProfiles";
                Input = await _httpClient.GetFromJsonAsync<ProfileInput>(apiUrl);

                if (Input == null)
                {
                    ModelState.AddModelError(string.Empty, "Profile data not found.");
                }

                var followingUrl = "https://localhost:7289/api/JobSeeker/followed-companies";
                FollowedCompanies = await _httpClient.GetFromJsonAsync<List<FollowedCompanyResponse>>(followingUrl)
                                     ?? new List<FollowedCompanyResponse>();
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Connection error: {ex.Message}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Unexpected error: {ex.Message}");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var token = HttpContext.Session.GetString("Token");
                var userId = HttpContext.Session.GetString("UserId");

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("JWT token or UserId missing. Redirecting to login.");
                    return RedirectToPage("/JobSeeker/Login");
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var apiUrl = $"https://localhost:7289/api/JobSeekerProfiles";

                var response = await _httpClient.PatchAsJsonAsync(apiUrl, Input);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToPage();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Update failed: {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Connection error: {ex.Message}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Unexpected error: {ex.Message}");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUnFollowAsync(int companyId)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/JobSeeker/Login");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = $"https://localhost:7289/api/JobSeeker/unfollow-company/{companyId}";
            var response = await _httpClient.DeleteAsync(apiUrl);


            if (!response.IsSuccessStatusCode)
                Console.WriteLine($"Unfollow request failed: {response.StatusCode}");

            await OnGetAsync();
            return Page();
        }

    }
}
