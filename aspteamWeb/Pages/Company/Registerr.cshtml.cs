using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace teamProject.WEB.Pages.Company
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
            public string Industry { get; set; } = string.Empty;

            // REMOVE CompanySize since your API doesn't expect it
            // If you need it, add it to your RegisterCompanyDto first
        }

        public void OnGet()
        {
            // Initialize if needed
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ Razor Page ModelState is invalid:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"   {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return Page();
            }

            try
            {
                var apiUrl = "https://localhost:7289/api/Auth/register-company";

                // CRITICAL FIX: Make sure Industry matches your enum exactly
                Console.WriteLine($"🔍 Input Industry string: '{Input.Industry}'");

                // List your exact Industry enum values here:
                var validIndustries = Enum.GetNames(typeof(Industry));
                Console.WriteLine($"🔍 Valid industries: {string.Join(", ", validIndustries)}");

                if (!Enum.TryParse<Industry>(Input.Industry, true, out var industryEnum))
                {
                    Console.WriteLine($"❌ Failed to parse industry: '{Input.Industry}'");
                    ModelState.AddModelError(string.Empty, $"Invalid industry: {Input.Industry}");
                    return Page();
                }

                Console.WriteLine($"✅ Parsed industry enum: {industryEnum}");

                // Create the exact DTO that matches your RegisterCompanyDto
                var dto = new
                {
                    CompanyName = Input.CompanyName?.Trim(),
                    Email = Input.Email?.Trim(),
                    Password = Input.Password,
                    ConfirmPassword = Input.ConfirmPassword,
                    Industry = industryEnum
                    // NOTE: Removed CompanySize since your API doesn't expect it
                };

                Console.WriteLine($"🌐 Sending DTO: CompanyName='{dto.CompanyName}', Email='{dto.Email}', Industry='{dto.Industry}'");

                var jsonContent = System.Text.Json.JsonSerializer.Serialize(dto);
                Console.WriteLine($"🌐 JSON being sent: {jsonContent}");

                var response = await _httpClient.PostAsJsonAsync(apiUrl, dto);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"🌐 Response Status: {response.StatusCode}");
                Console.WriteLine($"🌐 Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Company account created successfully! Please log in.";
                    return RedirectToPage("/Login");
                }
                else
                {
                    Console.WriteLine($"❌ API call failed with status: {response.StatusCode}");
                    ModelState.AddModelError(string.Empty, $"Registration failed: {responseContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ HTTP Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, $"Connection error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected error: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            }

            return Page();
        }
    }
}
