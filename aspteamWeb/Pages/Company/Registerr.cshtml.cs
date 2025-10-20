using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace aspteamWeb.Pages.Company
{
    public class RegisterModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public RegisterModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty]
        public RegisterCompanyInput Input { get; set; } = new();

        public class RegisterCompanyInput
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Passwords do not match")]
            [Display(Name = "Confirm Password")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Company Name")]
            public string CompanyName { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Industry")]
            public string Industry { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Company Size")]
            public string CompanySize { get; set; } = string.Empty;
        }

        // Add this class to deserialize the API response
        public class AuthResponse
        {
            public int UserId { get; set; }
            public string Role { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
        }

        public void OnGet()
        {
            // Initialize if needed
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var apiUrl = "https://localhost:7289/api/Auth/register-company";

                // FIXED: Remove the 'dto' wrapper and send data directly
                // Also convert CompanySize to int
                var payload = new
                {
                    CompanyName = Input.CompanyName?.Trim(),
                    Email = Input.Email?.Trim(),
                    Password = Input.Password,
                    ConfirmPassword = Input.ConfirmPassword,
                    Industry = ConvertIndustryToEnum(Input.Industry),
                    CompanySize = ConvertCompanySizeToInt(Input.CompanySize)
                };

                var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);

                if (response.IsSuccessStatusCode)
                {
                    // Get the response data containing UserId and Token
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

                    if (result != null)
                    {
                        // Store authentication data in session or cookies
                        HttpContext.Session.SetString("UserId", result.UserId.ToString());
                        HttpContext.Session.SetString("Token", result.Token);
                        HttpContext.Session.SetString("Role", result.Role);

                        TempData["SuccessMessage"] = "Account created successfully!";
                        return RedirectToPage("/Company/CompanyDashboard");
                    }

                    return RedirectToPage("/Login");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Registration failed: {errorContent}");
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

        // Convert industry string to enum value (integer)
        private int ConvertIndustryToEnum(string industry)
        {
            return industry switch
            {
                "Technology" => 0,
                "Healthcare" => 1,
                "Finance" => 2,
                "Education" => 3,
                "Manufacturing" => 4,
                "Retail" => 5,
                "Consulting" => 6,
                "RealEstate" => 7,
                "Other" => 8,
                _ => 8 // Default to Other
            };
        }

        // Convert company size string to integer
        private int ConvertCompanySizeToInt(string companySize)
        {
            return companySize switch
            {
                "1-10" => 10,
                "11-50" => 50,
                "51-200" => 200,
                "201-500" => 500,
                "500+" => 1000,
                _ => 0
            };
        }
    }
}