using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net.Http;
using static aspteamWeb.Pages.JobSeeker.RegisterModel;

namespace aspteamWeb.Pages.JobSeeker
{
    public class LoginModel : PageModel
    {
        private readonly HttpClient _httpClient;
        public LoginModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty]
        public LoginJobSeekerInput Input { get; set; } = new();

        public class LoginJobSeekerInput
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }
        public class LoginResponse
        {
            public int UserId { get; set; }
            public string Role { get; set; }
            public string Token { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var apiUrl = "https://localhost:7289/api/Auth/login-jobseeker";

                var response = await _httpClient.PostAsJsonAsync(apiUrl, Input);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "login succesful";

                    var loginData = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    if (loginData == null)
                    {
                        ModelState.AddModelError(string.Empty, "Unexpected server response.");
                        return Page();
                    }

                    HttpContext.Session.SetString("Token", loginData.Token);
                    HttpContext.Session.SetString("UserId", loginData.UserId.ToString());
                    HttpContext.Session.SetString("Role", loginData.Role);

                    Console.WriteLine($"Token: {loginData.Token}, UserId: {loginData.UserId.ToString()}, Role: {loginData.Role}");


                    return RedirectToPage("JobSeekerDashboard");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Login failed: {errorContent}");
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Connection error: {ex.Message}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            }

            return Page();
        }
    }
}