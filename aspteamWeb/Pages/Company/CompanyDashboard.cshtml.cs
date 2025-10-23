using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace aspteamWeb.Pages.Company
{
    public class CompanyDashboardModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CompanyDashboardModel> _logger;

        public CompanyDashboardModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<CompanyDashboardModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public List<JobPosting> Jobs { get; set; } = new();
        public string ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Get CompanyId from session
                var companyId = HttpContext.Session.GetInt32("CompanyId");

                if (companyId == null)
                {
                    ErrorMessage = "Please log in to view your jobs.";
                    return;
                }

                // Call API to get jobs for this company
                var client = _httpClientFactory.CreateClient();
                var apiUrl = _configuration["ApiUrl"] ?? "https://localhost:7289/api";

                var response = await client.GetAsync($"{apiUrl}/Jobs/company/{companyId}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiJobs = JsonSerializer.Deserialize<List<JobDto>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Map API jobs to display model
                    Jobs = apiJobs?.Select(job => new JobPosting
                    {
                        Id = job.Id,
                        Title = job.Title,
                        Location = job.Location,
                        Type = GetEmploymentTypeText(job.EmploymentType),
                        ApplicantCount = 0, // You'll need to add this to your API later
                        PostedAt = job.CreatedAt,
                        Status = job.IsActive ? "Active" : "Closed"
                    }).ToList() ?? new List<JobPosting>();

                    _logger.LogInformation("Loaded {Count} jobs for company {CompanyId}", Jobs.Count, companyId);
                }
                else
                {
                    _logger.LogWarning("Failed to load jobs. Status: {Status}", response.StatusCode);
                    ErrorMessage = "Failed to load jobs.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading company jobs");
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostCloseJobAsync(int id)
        {
            try
            {
                var companyId = HttpContext.Session.GetInt32("CompanyId");
                if (companyId == null)
                {
                    return RedirectToPage("/Account/Login");
                }

                var apiUrl = _configuration["ApiUrl"] ?? "https://localhost:7289/api";
                var client = _httpClientFactory.CreateClient();

                // First, get the job to verify ownership
                var getResponse = await client.GetAsync($"{apiUrl}/Jobs/{id}");
                if (!getResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Job not found.";
                    return RedirectToPage();
                }

                var jobJson = await getResponse.Content.ReadAsStringAsync();
                var job = JsonSerializer.Deserialize<JobDto>(jobJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Verify ownership
                if (job.PostedBy != companyId.Value)
                {
                    TempData["Error"] = "You don't have permission to close this job.";
                    return RedirectToPage();
                }

                // Update job to set IsActive = false
                job.IsActive = false;

                var updateJson = JsonSerializer.Serialize(job);
                var content = new StringContent(updateJson, Encoding.UTF8, "application/json");

                var updateResponse = await client.PutAsync($"{apiUrl}/Jobs/{id}", content);

                if (updateResponse.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Job closed successfully! It will still appear in your history.";
                }
                else
                {
                    TempData["Error"] = "Failed to close the job.";
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing job {JobId}", id);
                TempData["Error"] = "An error occurred while closing the job.";
                return RedirectToPage();
            }
        }

        private string GetEmploymentTypeText(int employmentType)
        {
            return employmentType switch
            {
                1 => "Full-Time",
                2 => "Part-Time",
                3 => "Contract",
                4 => "Internship",
                _ => "Unknown"
            };
        }

        // DTO classes to match your API response
        public class JobDto
        {
            public int Id { get; set; }
            public int PostedBy { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Requirements { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public int Industry { get; set; }
            public int ExperienceLevel { get; set; }
            public int EmploymentType { get; set; }
            public int WorkArrangement { get; set; }
            public decimal? MaxSalaryRange { get; set; }
            public decimal? MinSalaryRange { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsActive { get; set; }
        }

        public class JobPosting
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public int ApplicantCount { get; set; }
            public DateTime PostedAt { get; set; }
            public string Status { get; set; } = "Active";
        }
    }
}