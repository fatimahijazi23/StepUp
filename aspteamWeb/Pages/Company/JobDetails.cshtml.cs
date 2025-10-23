using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace aspteamWeb.Pages.Company
{
    public class JobDetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JobDetailsModel> _logger;

        public JobDetailsModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<JobDetailsModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public JobDetails? Job { get; set; }
        public int ApplicantCount { get; set; }
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                // Get CompanyId from session to verify ownership
                var companyId = HttpContext.Session.GetInt32("CompanyId");

                if (companyId == null)
                {
                    _logger.LogWarning("User not logged in, redirecting to login");
                    return RedirectToPage("/Company/Login");
                }

                // Call API to get job details
                var client = _httpClientFactory.CreateClient();
                var apiUrl = _configuration["ApiUrl"] ?? "https://localhost:7289/api";

                var response = await client.GetAsync($"{apiUrl}/Jobs/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("Job {JobId} not found", id);
                        return NotFound();
                    }

                    ErrorMessage = "Failed to load job details.";
                    return Page();
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var apiJob = JsonSerializer.Deserialize<JobDto>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiJob == null)
                {
                    _logger.LogError("Failed to deserialize job data for job {JobId}", id);
                    return NotFound();
                }

                // Verify the job belongs to this company
                if (apiJob.PostedBy != companyId.Value)
                {
                    _logger.LogWarning("Company {CompanyId} tried to access job {JobId} posted by {PostedBy}",
                        companyId.Value, id, apiJob.PostedBy);
                    ErrorMessage = "You don't have permission to view this job.";
                    return Page();
                }

                // Map API data to display model
                Job = new JobDetails
                {
                    Id = apiJob.Id,
                    JobTitle = apiJob.Title,
                    CompanyName = "Your Company", // TODO: Fetch company name from API if available
                    Location = apiJob.Location,
                    JobType = GetEmploymentTypeText(apiJob.EmploymentType),
                    Industry = GetIndustryText(apiJob.Industry),
                    SalaryRange = FormatSalaryRange(apiJob.MinSalaryRange, apiJob.MaxSalaryRange),
                    JobDescription = apiJob.Description,
                    Requirements = apiJob.Requirements,
                    Benefits = apiJob.Description, // Using Description as Benefits
                    PostedDaysAgo = (DateTime.Now - apiJob.CreatedAt).Days,
                    ExperienceLevel = GetExperienceLevelText(apiJob.ExperienceLevel),
                    WorkArrangement = GetWorkArrangementText(apiJob.WorkArrangement),
                    IsActive = apiJob.IsActive
                };

                // Fetch actual applicant count from Applications API
                try
                {
                    var countResponse = await client.GetAsync($"{apiUrl}/Applications/job/{id}/count");
                    if (countResponse.IsSuccessStatusCode)
                    {
                        var countJson = await countResponse.Content.ReadAsStringAsync();
                        var countData = JsonSerializer.Deserialize<aspteamAPI.DTOs.ApplicantCountResponse>(countJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        ApplicantCount = countData?.ApplicantCount ?? 0;
                    }
                    else
                    {
                        ApplicantCount = 0;
                    }
                }
                catch
                {
                    ApplicantCount = 0; // Fallback if count endpoint fails
                }

                _logger.LogInformation("Loaded job details for job {JobId}", id);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading job details for job {JobId}", id);
                ErrorMessage = $"An error occurred while loading job details: {ex.Message}";
                return Page();
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

        private string GetIndustryText(int industry)
        {
            return industry switch
            {
                1 => "Technology",
                2 => "Healthcare",
                3 => "Finance",
                4 => "Education",
                5 => "Retail",
                6 => "Manufacturing",
                7 => "Hospitality",
                8 => "Construction",
                _ => "Other"
            };
        }

        private string GetExperienceLevelText(int experienceLevel)
        {
            return experienceLevel switch
            {
                1 => "Entry Level",
                2 => "Mid Level",
                3 => "Senior Level",
                4 => "Executive",
                _ => "Not Specified"
            };
        }

        private string GetWorkArrangementText(int workArrangement)
        {
            return workArrangement switch
            {
                1 => "On-site",
                2 => "Remote",
                3 => "Hybrid",
                _ => "Not Specified"
            };
        }

        private string FormatSalaryRange(decimal? minSalary, decimal? maxSalary)
        {
            if (minSalary.HasValue && maxSalary.HasValue)
            {
                return $"${minSalary.Value:N0} - ${maxSalary.Value:N0}";
            }
            else if (minSalary.HasValue)
            {
                return $"From ${minSalary.Value:N0}";
            }
            else if (maxSalary.HasValue)
            {
                return $"Up to ${maxSalary.Value:N0}";
            }
            else
            {
                return "Salary not disclosed";
            }
        }

        // DTO class matching your API
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

        public class JobDetails
        {
            public int Id { get; set; }
            public string JobTitle { get; set; } = string.Empty;
            public string CompanyName { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public string JobType { get; set; } = string.Empty;
            public string Industry { get; set; } = string.Empty;
            public string SalaryRange { get; set; } = string.Empty;
            public string JobDescription { get; set; } = string.Empty;
            public string Requirements { get; set; } = string.Empty;
            public string Benefits { get; set; } = string.Empty;
            public int PostedDaysAgo { get; set; }
            public string ExperienceLevel { get; set; } = string.Empty;
            public string WorkArrangement { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }
    }
}