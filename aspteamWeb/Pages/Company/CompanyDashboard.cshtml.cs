using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        private string GetEmploymentTypeText(int employmentType)
        {
            return employmentType switch
            {
                0 => "Full-Time",
                1 => "Part-Time",
                2 => "Contract",
                3 => "Internship",
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
            public decimal MaxSalaryRange { get; set; }
            public decimal MinSalaryRange { get; set; }
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