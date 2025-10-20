using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace aspteamWeb.Pages.Company
{
    public class CreateJobModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CreateJobModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public JobInputModel Input { get; set; } = new();

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please fill all required fields.";
                return Page();
            }

            try
            {
                // Get UserId from session
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    ErrorMessage = "You must be logged in to create a job.";
                    return Page();
                }

                // Create job DTO to send to API
                var jobDto = new
                {
                    PostedBy = userId.Value,
                    Title = Input.JobTitle,
                    Description = Input.JobDescription,
                    Requirements = Input.Requirements,
                    Location = Input.Location,
                    Industry = MapIndustry(Input.Industry),
                    EmploymentType = MapEmploymentType(Input.JobType),
                    MinSalaryRange = ExtractMinSalary(Input.SalaryRange),
                    MaxSalaryRange = ExtractMaxSalary(Input.SalaryRange),
                    IsActive = true
                };

                // Call your API to create the job
                var client = _httpClientFactory.CreateClient();
                var apiUrl = _configuration["ApiUrl"] ?? "https://localhost:7296/api";

                var json = JsonSerializer.Serialize(jobDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{apiUrl}/Job", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = $"Job '{Input.JobTitle}' created successfully!";
                    return RedirectToPage("/Company/CompanyDashboard");
                }
                else
                {
                    ErrorMessage = $"Failed to create job. Status: {response.StatusCode}. Error: {responseContent}";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }
        }

        // Helper methods to map form values to database enums
        private int MapIndustry(string industry)
        {
            return industry switch
            {
                "Technology" => 0,
                "Healthcare" => 1,
                "Finance" => 2,
                "Education" => 3,
                "Retail" => 4,
                _ => 0
            };
        }

        private int MapEmploymentType(string jobType)
        {
            return jobType switch
            {
                "Full-Time" => 0,
                "Part-Time" => 1,
                "Contract" => 2,
                "Internship" => 3,
                _ => 0
            };
        }

        private decimal? ExtractMinSalary(string salaryRange)
        {
            try
            {
                // Extract minimum from "$50,000 - $70,000" format
                var parts = salaryRange.Split('-');
                if (parts.Length > 0)
                {
                    var minStr = parts[0].Trim().Replace("$", "").Replace(",", "");
                    return decimal.Parse(minStr);
                }
            }
            catch { }
            return null;
        }

        private decimal? ExtractMaxSalary(string salaryRange)
        {
            try
            {
                // Extract maximum from "$50,000 - $70,000" format
                var parts = salaryRange.Split('-');
                if (parts.Length > 1)
                {
                    var maxStr = parts[1].Trim().Replace("$", "").Replace(",", "");
                    return decimal.Parse(maxStr);
                }
            }
            catch { }
            return null;
        }

        public class JobInputModel
        {
            [Required, StringLength(100)]
            public string JobTitle { get; set; } = string.Empty;

            [Required, StringLength(100)]
            public string Location { get; set; } = string.Empty;

            [Required]
            public string JobType { get; set; } = string.Empty;

            [Required]
            public string Industry { get; set; } = string.Empty;

            [Required, StringLength(1000)]
            public string JobDescription { get; set; } = string.Empty;

            [Required, StringLength(1000)]
            public string Requirements { get; set; } = string.Empty;

            [StringLength(1000)]
            public string Benefits { get; set; } = string.Empty;

            [Required, StringLength(50)]
            public string SalaryRange { get; set; } = string.Empty;
        }
    }
}