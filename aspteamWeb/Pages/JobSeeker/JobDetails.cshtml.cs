using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using static aspteamWeb.Pages.JobSeeker.JobSeekerDashboardModel;

namespace aspteamWeb.Pages.JobSeeker
{
    public class JobDetailsModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public JobDetailsModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty]
        public JobResponse Job { get; set; } = default!;

        public class JobResponse
        {
            public int Id { get; set; }
            public int CompanyId { get; set; }
            public string CompanyName { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string? Requirements { get; set; }
            public string? Location { get; set; }
            public Industry? Industry { get; set; }
            public ExperienceLevel? ExperienceLevel { get; set; }
            public EmploymentType? EmploymentType { get; set; }
            public WorkArrangement? WorkArrangement { get; set; }
            public decimal? MaxSalaryRange { get; set; }
            public decimal? MinSalaryRange { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsActive { get; set; }

        }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("JWT token missing. Redirecting to login.");
                    Response.Redirect("/JobSeeker/Login");
                    return Page();
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var apiUrl = $"https://localhost:7289/api/Jobs/{id}";
                var responseJob = await _httpClient.GetFromJsonAsync<JobResponse>(apiUrl);

                if (responseJob != null)
                    Job = responseJob;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching job Details: {ex.Message}");
            }

            return Page();
        }

    }
}
