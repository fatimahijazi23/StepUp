using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace aspteamWeb.Pages.Company
{
    public class EditJobModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EditJobModel> _logger;

        public EditJobModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<EditJobModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        [BindProperty]
        public JobUpdateDto Job { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
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
                var response = await client.GetAsync($"{apiUrl}/Jobs/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Job = JsonSerializer.Deserialize<JobUpdateDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Verify this job belongs to the logged-in company
                    if (Job.PostedBy != companyId.Value)
                    {
                        TempData["ErrorMessage"] = "You don't have permission to edit this job.";
                        return RedirectToPage("/Company/History");
                    }

                    return Page();
                }
                else
                {
                    ErrorMessage = "Failed to load job details.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading job for edit");
                ErrorMessage = "An error occurred while loading the job.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var companyId = HttpContext.Session.GetInt32("CompanyId");
                if (companyId == null)
                {
                    return RedirectToPage("/Account/Login");
                }

                var apiUrl = _configuration["ApiUrl"] ?? "https://localhost:7289/api";
                var client = _httpClientFactory.CreateClient();

                var jsonContent = JsonSerializer.Serialize(Job);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"{apiUrl}/Jobs/{Job.Id}", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Job updated successfully!";
                    return RedirectToPage("/Company/History");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Failed to update job: {error}";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job");
                ErrorMessage = "An error occurred while updating the job.";
                return Page();
            }
        }
    }

    public class JobUpdateDto
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
        public bool IsActive { get; set; }
    }
}