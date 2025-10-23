using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace aspteamWeb.Pages.Company
{
    public class ViewApplicantsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ViewApplicantsModel> _logger;

        public ViewApplicantsModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ViewApplicantsModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int JobId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FitStatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public string JobTitle { get; set; } = string.Empty;
        public List<ApplicantViewModel> Applicants { get; set; } = new();
        public int TotalApplicants => Applicants.Count;
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var companyId = HttpContext.Session.GetInt32("CompanyId");

                if (companyId == null)
                {
                    ErrorMessage = "Please log in to view applicants.";
                    _logger.LogWarning("CompanyId not found in session");
                    return RedirectToPage("/Account/Login");
                }

                var apiUrl = _configuration["ApiUrl"] ?? "https://localhost:7289/api";

                // Get job title
                await LoadJobTitleAsync(apiUrl);

                // Get applications for this job
                await LoadApplicationsAsync(apiUrl);

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred while loading applicants.";
                _logger.LogError(ex, "Error loading applicants for job {JobId}", JobId);
                return Page();
            }
        }

        private async Task LoadJobTitleAsync(string apiUrl)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{apiUrl}/Jobs/{JobId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var job = JsonSerializer.Deserialize<JobApiDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    JobTitle = job?.Title ?? "Job";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading job title");
                JobTitle = "Job";
            }
        }

        private async Task LoadApplicationsAsync(string apiUrl)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{apiUrl}/Applications/job/{JobId}?page=1&pageSize=100");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApplicationPaginatedResponse>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResponse?.Data != null)
                    {
                        var allApplicants = apiResponse.Data.Select(app => new ApplicantViewModel
                        {
                            Id = app.Id,
                            Name = app.ApplicantName ?? "Unknown",
                            FitScore = CalculateFitScore(app),
                            FitScoreColor = GetFitScoreColor(CalculateFitScore(app)),
                            Status = app.Status?.ToString() ?? "In-progress",
                            CvUrl = app.CvUrl ?? "#",
                            AppliedDaysAgo = app.AppliedAt.HasValue
                                ? (int)(DateTime.Now - app.AppliedAt.Value).TotalDays
                                : 0
                        }).ToList();

                        // Apply filters
                        Applicants = allApplicants;

                        if (!string.IsNullOrEmpty(FitStatusFilter))
                        {
                            Applicants = FitStatusFilter.ToLower() switch
                            {
                                "high" => Applicants.Where(a => a.FitScore >= 7).ToList(),
                                "medium" => Applicants.Where(a => a.FitScore >= 4 && a.FitScore < 7).ToList(),
                                "low" => Applicants.Where(a => a.FitScore < 4).ToList(),
                                _ => Applicants
                            };
                        }

                        if (!string.IsNullOrEmpty(StatusFilter))
                        {
                            Applicants = Applicants.Where(a =>
                                a.Status.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to load applications. Status: {Status}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading applications");
            }
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int applicantId, int jobId, string status)
        {
            try
            {
                var apiUrl = _configuration["ApiUrl"] ?? "https://localhost:7289/api";
                var client = _httpClientFactory.CreateClient();

                // Map status string to enum value
                var statusDto = new
                {
                    Status = MapStatusToEnum(status),
                    Notes = $"Status updated to {status}"
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(statusDto),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await client.PutAsync(
                    $"{apiUrl}/Applications/{applicantId}/status",
                    jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Application status updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update application status.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating application status");
                TempData["ErrorMessage"] = "An error occurred while updating status.";
            }

            return RedirectToPage(new { jobId, FitStatusFilter, StatusFilter });
        }

        private int CalculateFitScore(ApplicationDto app)
        {
            // Simple fit score calculation (you can enhance this)
            var random = new Random(app.Id);
            return random.Next(1, 11);
        }

        private string GetFitScoreColor(int score)
        {
            return score switch
            {
                >= 7 => "#16a34a", // Green for high fit
                >= 4 => "#facc15", // Yellow for medium fit
                _ => "#ef4444"     // Red for low fit
            };
        }

        private int MapStatusToEnum(string status)
        {
            return status switch
            {
                "In-progress" => 0,
                "Reviewed" => 1,
                "Accepted" => 2,
                "Rejected" => 3,
                _ => 0
            };
        }

        public class ApplicantViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int FitScore { get; set; }
            public string FitScoreColor { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string CvUrl { get; set; } = string.Empty;
            public int AppliedDaysAgo { get; set; }
        }

        public class JobApiDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
        }

        public class ApplicationPaginatedResponse
        {
            public bool Success { get; set; }
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public List<ApplicationDto> Data { get; set; }
        }

        public class ApplicationDto
        {
            public int Id { get; set; }
            public string ApplicantName { get; set; }
            public string CvUrl { get; set; }
            public int? Status { get; set; }
            public DateTime? AppliedAt { get; set; }
        }
    }
}