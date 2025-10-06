using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace aspteamWeb.Pages
{
    public class LoginModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public LoginModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public class LoginInput
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }


        [BindProperty]
        public LoginInput Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var apiUrl = "https://localhost:7289/api/Auth/login-jobseeker";
                var dto = new
                {
                    Email = Input.Email,
                    Password = Input.Password
                };
                var response = await _httpClient.PostAsJsonAsync(apiUrl, dto);

                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

                    if (authResponse != null)
                    {
                        HttpContext.Session.SetString("Token", authResponse.Token);
                        HttpContext.Session.SetString("Role", authResponse.Role);
                        HttpContext.Session.SetInt32("UserId", authResponse.UserId);

                        TempData["SuccessMessage"] = "Logged in successfully!";
                        return RedirectToPage("/JobSeeker/Profile");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid response from server.");
                        return Page();
                    }
                }
                else
                {
                    // Error from backend 
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, "Login failed. Please check your email and password.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return Page();
            }
        }
    }
}
