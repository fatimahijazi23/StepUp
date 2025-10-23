using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace aspteamWeb.Pages.Company
{
    public class HistoryModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HistoryModel> _logger;

        public HistoryModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<HistoryModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        // Bindable query properties for search and filter
        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Page { get; set; } = 1;

        public int TotalPages { get; set; }
        public int CurrentPage => Page;
        public List<JobDisplayDto> Jobs { get; set; } = new List<JobDisplayDto>();
        public string ErrorMessage { get; set; }

        private const int PageSize = 10;

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Get CompanyId from session
                var companyId = HttpContext.Session.GetInt32("CompanyId");

                if (companyId == null)
                {
                    ErrorMessage = "Please log in to view your job postings.";
                    _logger.LogWarning("CompanyId not found in session");
                    return RedirectToPage("/Account/Login");
                }

                // Build API URL
                var apiUrl = _configuration["ApiUrl"] ?? "https://localhost:7289/api";
                var fullUrl = $"{apiUrl}/Jobs/company/{companyId.Value}";

                _logger.LogInformation("Fetching jobs for CompanyId: {CompanyId} from {Url}", companyId, fullUrl);

                // Call API to get company's jobs
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiJobs = JsonSerializer.Deserialize<List<JobApiDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<JobApiDto>();

                    _logger.LogInformation("Retrieved {Count} jobs from API", apiJobs.Count);

                    // Get application counts for each job
                    var allJobs = new List<JobDisplayDto>();
                    foreach (var job in apiJobs)
                    {
                        var applicantsCount = await GetApplicationCountAsync(job.Id);

                        allJobs.Add(new JobDisplayDto
                        {
                            Id = job.Id,
                            Title = job.Title,
                            Location = job.Location,
                            JobType = GetEmploymentTypeName(job.EmploymentType),
                            Status = job.IsActive ? "Active" : "Closed",
                            PostedDaysAgo = job.CreatedAt.HasValue
                                ? (int)(DateTime.Now - job.CreatedAt.Value).TotalDays
                                : 0,
                            ApplicantsCount = applicantsCount,
                            Industry = GetIndustryName(job.Industry),
                            ExperienceLevel = GetExperienceLevelName(job.ExperienceLevel),
                            WorkArrangement = GetWorkArrangementName(job.WorkArrangement),
                            MinSalaryRange = job.MinSalaryRange,
                            MaxSalaryRange = job.MaxSalaryRange
                        });
                    }

                    // Apply search filter
                    if (!string.IsNullOrEmpty(SearchQuery))
                    {
                        allJobs = allJobs.Where(j =>
                            (j.Title?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (j.Location?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (j.Industry?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false)
                        ).ToList();
                    }

                    // Apply status filter
                    if (!string.IsNullOrEmpty(StatusFilter))
                    {
                        allJobs = allJobs.Where(j =>
                            j.Status?.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase) ?? false
                        ).ToList();
                    }

                    // Calculate pagination
                    var totalItems = allJobs.Count;
                    TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

                    // Ensure current page is valid
                    if (Page < 1) Page = 1;
                    if (Page > TotalPages && TotalPages > 0) Page = TotalPages;

                    // Apply pagination
                    Jobs = allJobs
                        .OrderByDescending(j => j.PostedDaysAgo == 0 ? int.MaxValue : j.PostedDaysAgo) // Most recent first
                        .Skip((Page - 1) * PageSize)
                        .Take(PageSize)
                        .ToList();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Failed to load jobs. Status: {response.StatusCode}";
                    _logger.LogError("API call failed - Status: {Status}, Error: {Error}",
                        response.StatusCode, errorContent);
                }

                return Page();
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = "Cannot connect to API. Please check if the API server is running.";
                _logger.LogError(ex, "HTTP request failed when fetching job history");
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred while loading jobs.";
                _logger.LogError(ex, "Unexpected error in OnGetAsync");
                return Page();
            }
        }

        // Get application count for a specific job
        private async Task<int> GetApplicationCountAsync(int jobId)
        {
            try
            {
                var apiUrl = _configuration["ApiUrl"] ?? "https://localhost:7289/api";
                var fullUrl = $"{apiUrl}/Applications/job/{jobId}?pageSize=1";

                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApplicationPaginatedResponse>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return result?.TotalCount ?? 0;
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get application count for job {JobId}", jobId);
                return 0;
            }
        }

        // Handler for deleting a job
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var companyId = HttpContext.Session.GetInt32("CompanyId");

                if (companyId == null)
                {
                    TempData["ErrorMessage"] = "Session expired. Please log in again.";
                    return RedirectToPage("/Account/Login");
                }

                var apiUrl = _configuration["ApiUrl"] ?? "https://localhost:7289/api";
                var fullUrl = $"{apiUrl}/Jobs/{id}";

                _logger.LogInformation("Deleting job {JobId} for CompanyId: {CompanyId}", id, companyId);

                var client = _httpClientFactory.CreateClient();
                var response = await client.DeleteAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Job posting deleted successfully!";
                    _logger.LogInformation("Job {JobId} deleted successfully", id);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Failed to delete job. Please try again.";
                    _logger.LogError("Failed to delete job {JobId} - Status: {Status}, Error: {Error}",
                        id, response.StatusCode, errorContent);
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the job.";
                _logger.LogError(ex, "Error deleting job {JobId}", id);
                return RedirectToPage();
            }
        }

        // Helper methods to convert enum IDs to display names
        private string GetIndustryName(int? industryId)
        {
            return industryId switch
            {
                1 => "Technology",
                2 => "Healthcare",
                3 => "Finance",
                4 => "Education",
                5 => "Manufacturing",
                6 => "Retail",
                7 => "Construction",
                8 => "Hospitality",
                9 => "Marketing",
                10 => "Other",
                _ => "Unknown"
            };
        }

        private string GetExperienceLevelName(int? levelId)
        {
            return levelId switch
            {
                1 => "Entry Level",
                2 => "Mid Level",
                3 => "Senior Level",
                4 => "Executive",
                _ => "Unknown"
            };
        }

        private string GetEmploymentTypeName(int? typeId)
        {
            return typeId switch
            {
                1 => "Full-Time",
                2 => "Part-Time",
                3 => "Contract",
                4 => "Internship",
                _ => "Unknown"
            };
        }

        private string GetWorkArrangementName(int? arrangementId)
        {
            return arrangementId switch
            {
                1 => "On-Site",
                2 => "Remote",
                3 => "Hybrid",
                _ => "Unknown"
            };
        }
    }

    // DTOs matching your API structure
    public class JobApiDto
    {
        public int Id { get; set; }
        public int PostedBy { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public string Location { get; set; }
        public int? Industry { get; set; }
        public int? ExperienceLevel { get; set; }
        public int? EmploymentType { get; set; }
        public int? WorkArrangement { get; set; }
        public decimal? MinSalaryRange { get; set; }
        public decimal? MaxSalaryRange { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    // DTO for display in the UI
    public class JobDisplayDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public string JobType { get; set; }
        public string Status { get; set; }
        public int PostedDaysAgo { get; set; }
        public int ApplicantsCount { get; set; }
        public string Industry { get; set; }
        public string ExperienceLevel { get; set; }
        public string WorkArrangement { get; set; }
        public decimal? MinSalaryRange { get; set; }
        public decimal? MaxSalaryRange { get; set; }
    }

    // Response DTO for application count
    public class ApplicationPaginatedResponse
    {
        public bool Success { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}