using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

namespace aspteamWeb.Pages.Company
{
    public class ProfileCompanyModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ProfileCompanyModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty]
        public CompanyViewModel Company { get; set; } = new();

        public int FollowersCount { get; set; } = 0;

        [TempData]
        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }

        // GET request - Fetch company data from API using UserId
        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var token = HttpContext.Session.GetString("Token");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                // Set authorization header if using JWT
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                // Fetch company profile using UserId
                var apiUrl = $"https://localhost:7289/api/CompanyProfiles/{userId}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var companyData = await response.Content.ReadFromJsonAsync<CompanyProfileDTO>();

                    if (companyData != null)
                    {
                        // Store the actual CompanyId from the response
                        HttpContext.Session.SetString("CompanyId", companyData.Id.ToString());

                        Company = new CompanyViewModel
                        {
                            Id = companyData.Id,
                            Name = companyData.Name ?? string.Empty,
                            Email = companyData.Email ?? string.Empty,
                            Industry = ConvertIndustryEnumToString(companyData.Industry),
                            CompanySize = ConvertCompanySizeIntToString(companyData.CompanySize),
                            About = companyData.About ?? string.Empty
                        };

                        FollowersCount = 0;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Failed to load company profile: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading profile: {ex.Message}";
            }

            return Page();
        }

        // POST request - Update company profile via API using CompanyId
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var token = HttpContext.Session.GetString("Token");

            // Get CompanyId from session (set during OnGet or use Company.Id from form)
            var companyIdString = HttpContext.Session.GetString("CompanyId");

            // If CompanyId not in session, use the Id from the form
            int companyId = Company.Id;
            if (!string.IsNullOrEmpty(companyIdString) && int.TryParse(companyIdString, out int sessionCompanyId))
            {
                companyId = sessionCompanyId;
            }

            if (companyId == 0)
            {
                ErrorMessage = "Company ID not found. Please refresh the page.";
                return Page();
            }

            try
            {
                // Set authorization header if using JWT
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                // Use CompanyId for the API endpoint
                var apiUrl = $"https://localhost:7289/api/CompanyProfiles/{companyId}";

                var updateDto = new UpdateCompanyProfileDTO
                {
                    Id = companyId, // Use the actual CompanyId
                    Name = Company.Name?.Trim(),
                    Email = Company.Email?.Trim(),
                    Industry = ConvertIndustryToEnum(Company.Industry),
                    CompanySize = ConvertCompanySizeToInt(Company.CompanySize),
                    About = Company.About?.Trim()
                };

                var response = await _httpClient.PatchAsJsonAsync(apiUrl, updateDto);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Company profile updated successfully!";
                    return RedirectToPage();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Failed to update profile: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating profile: {ex.Message}";
            }

            return Page();
        }

        // Convert industry enum (int) to string for display
        private string ConvertIndustryEnumToString(int industry)
        {
            return industry switch
            {
                0 => "Technology",
                1 => "Healthcare",
                2 => "Finance",
                3 => "Education",
                4 => "Manufacturing",
                5 => "Retail",
                6 => "Consulting",
                7 => "RealEstate",
                8 => "Other",
                _ => "Other"
            };
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
                _ => 8
            };
        }

        // Convert company size integer to display string
        private string ConvertCompanySizeIntToString(int companySize)
        {
            return companySize switch
            {
                10 => "1-10",
                50 => "11-50",
                200 => "51-200",
                500 => "201-500",
                1000 => "500+",
                _ => "1-10"
            };
        }

        // Convert company size string to integer (matches registration)
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

        // DTOs matching your API
        public class CompanyProfileDTO
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Email { get; set; }
            public int Industry { get; set; }
            public int CompanySize { get; set; }
            public string? About { get; set; }
        }

        public class UpdateCompanyProfileDTO
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Email { get; set; }
            public int Industry { get; set; }
            public int CompanySize { get; set; }
            public string? About { get; set; }
        }

        public class CompanyViewModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Company name is required.")]
            [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters.")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Industry is required.")]
            public string Industry { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Invalid email address.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Company size is required.")]
            public string CompanySize { get; set; } = string.Empty;

            [StringLength(1000, ErrorMessage = "About section cannot exceed 1000 characters.")]
            public string About { get; set; } = string.Empty;
        }
    }
}