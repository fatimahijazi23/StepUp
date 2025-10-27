using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;

namespace aspteamWeb.Pages.Company
{
    public class RegisterModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly string connectionString = "Server=DESKTOP-81J6GVU\\SQLEXPRESS;Database=SetUp;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

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

        public class AuthResponse
        {
            public int UserId { get; set; }
            public string Role { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var apiUrl = "https://localhost:7289/api/Auth/register-company";

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
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

                    if (result != null)
                    {
                        // ✅ FETCH CompanyId FROM DATABASE (Same as Login does)
                        int companyId = GetCompanyIdFromDatabase(result.UserId);

                        if (companyId > 0)
                        {
                            // Store ALL session data
                            HttpContext.Session.SetInt32("UserId", result.UserId);
                            HttpContext.Session.SetInt32("CompanyId", companyId); // ✅ THIS IS THE FIX!
                            HttpContext.Session.SetString("Token", result.Token);
                            HttpContext.Session.SetString("Role", result.Role);
                            HttpContext.Session.SetString("UserType", "Company");
                            HttpContext.Session.SetString("UserEmail", Input.Email);

                            TempData["SuccessMessage"] = "Account created successfully! Complete your profile.";
                            return RedirectToPage("/Company/ProfileCompany");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Account created but company profile not found. Please login.");
                            return Page();
                        }
                    }
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

        // ✅ Get CompanyId from database using UserId (Same logic as Login)
        private int GetCompanyIdFromDatabase(int userId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT Id FROM CompanyAccounts WHERE UserId = @UserId";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);

                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            return Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                Console.WriteLine($"Error fetching CompanyId: {ex.Message}");
            }

            return 0;
        }

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
                _ => 8
            };
        }

        private int ConvertCompanySizeToInt(string companySize)
        {
            return companySize switch
            {
                "1-10" => 10,
                "11-50" => 50,
                "51-200" => 200,
                "201-500" => 500,
                "500+" => 1000,
                _ => 10
            };
        }
    }
}