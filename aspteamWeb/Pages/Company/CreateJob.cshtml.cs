using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace aspteamWeb.Pages.Company
{
    public class CreateJobModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CreateJobModel> _logger;

        public CreateJobModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<CreateJobModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
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
                // Get CompanyId from session (NOT UserId!)
                var companyId = HttpContext.Session.GetInt32("CompanyId");
                var userId = HttpContext.Session.GetInt32("UserId");

                // TEMPORARY DEBUG INFO
                if (companyId == null)
                {
                    ErrorMessage = $"Session Error: CompanyId is NULL. UserId={userId}. Please log out completely and log back in. If the issue persists, your account may not have a company profile.";
                    _logger.LogWarning("CompanyId is null in session. UserId: {UserId}", userId);
                    return Page();
                }

                _logger.LogInformation("Creating job - UserId: {UserId}, CompanyId: {CompanyId}", userId, companyId);

                // Verify CompanyId exists (add validation)
                if (companyId.Value < 1 || companyId.Value > 100)
                {
                    ErrorMessage = $"Invalid CompanyId: {companyId}. Please contact support.";
                    return Page();
                }

                // Create job DTO matching your database structure exactly
                var jobDto = new
                {
                    PostedBy = companyId.Value,
                    Title = Input.JobTitle,
                    Description = Input.Description,
                    Requirements = Input.Requirements,
                    Location = Input.Location,
                    Industry = Input.Industry,
                    ExperienceLevel = Input.ExperienceLevel,
                    EmploymentType = Input.EmploymentType,
                    WorkArrangement = Input.WorkArrangement,
                    MinSalaryRange = Input.MinSalaryRange,
                    MaxSalaryRange = Input.MaxSalaryRange
                };

                // Get API URL from configuration
                var apiUrl = _configuration["ApiUrl"] ?? "https://localhost:7289/api";
                var fullUrl = $"{apiUrl}/Jobs";

                // Log the request details
                _logger.LogInformation("Attempting to create job. API URL: {Url}, CompanyId: {CompanyId}", fullUrl, companyId);

                // Call your API
                var client = _httpClientFactory.CreateClient();

                var json = System.Text.Json.JsonSerializer.Serialize(jobDto);
                _logger.LogInformation("Request payload: {Payload}", json);

                var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(fullUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("API Response - Status: {Status}, Content: {Content}",
                    response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = $"Job '{Input.JobTitle}' created successfully!";
                    return RedirectToPage("/Company/CompanyDashboard");
                }
                else
                {
                    // Enhanced error message with debugging info
                    ErrorMessage = $"Failed to create job posting.\n" +
                                 $"Status: {response.StatusCode}\n" +
                                 $"API URL: {fullUrl}\n" +
                                 $"Error: {responseContent}\n" +
                                 $"Tip: Make sure your API is running and the endpoint exists.";

                    _logger.LogError("Job creation failed - Status: {Status}, URL: {Url}, Response: {Response}",
                        response.StatusCode, fullUrl, responseContent);

                    return Page();
                }
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Cannot connect to API. Is the API server running?\nError: {ex.Message}";
                _logger.LogError(ex, "HTTP request failed when creating job");
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                _logger.LogError(ex, "Unexpected error when creating job");
                return Page();
            }
        }

        public class JobInputModel
        {
            [Required(ErrorMessage = "Job Title is required")]
            [StringLength(200, ErrorMessage = "Job Title cannot exceed 200 characters")]
            public string JobTitle { get; set; } = string.Empty;

            [Required(ErrorMessage = "Location is required")]
            [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
            public string Location { get; set; } = string.Empty;

            [Required(ErrorMessage = "Employment Type is required")]
            public int EmploymentType { get; set; }

            [Required(ErrorMessage = "Work Arrangement is required")]
            public int WorkArrangement { get; set; }

            [Required(ErrorMessage = "Industry is required")]
            public int Industry { get; set; }

            [Required(ErrorMessage = "Experience Level is required")]
            public int ExperienceLevel { get; set; }

            [Required(ErrorMessage = "Job Description is required")]
            [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
            public string Description { get; set; } = string.Empty;

            [Required(ErrorMessage = "Requirements are required")]
            [StringLength(2000, ErrorMessage = "Requirements cannot exceed 2000 characters")]
            public string Requirements { get; set; } = string.Empty;

            [Required(ErrorMessage = "Minimum Salary is required")]
            [Range(0, 10000000, ErrorMessage = "Minimum Salary must be between 0 and 10,000,000")]
            public decimal MinSalaryRange { get; set; }

            [Required(ErrorMessage = "Maximum Salary is required")]
            [Range(0, 10000000, ErrorMessage = "Maximum Salary must be between 0 and 10,000,000")]
            public decimal MaxSalaryRange { get; set; }
        }
    }
}