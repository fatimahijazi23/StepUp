using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace aspteamWeb.Pages.JobSeeker
{
    public class JobSeekerDashboardModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public JobSeekerDashboardModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty]
        public JobFilterModel Filter { get; set; } = new();

        public class JobFilterModel
        {
            public string? Keyword { get; set; }
            public string? Location { get; set; }
            public int? Industry { get; set; }
        }

        public List<JobPostingDto> JobListings { get; set; } = new();

        public class JobPostingDto
        {
            public int Id { get; set; }
            public int CompanyId { get; set; }
            public string CompanyName { get; set; } = string.Empty;
            public bool IsFollowing { get; set; }
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

        public async Task OnGetAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("JWT token missing. Redirecting to login.");
                    Response.Redirect("/JobSeeker/Login");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var apiUrl = "https://localhost:7289/api/Jobs";
                var jobs = await _httpClient.GetFromJsonAsync<List<JobPostingDto>>(apiUrl);

                if (jobs != null)
                    JobListings = jobs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching job listings: {ex.Message}");
            }
        }

        public async Task OnPostFilterAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("JWT token missing. Redirecting to login.");
                    Response.Redirect("/JobSeeker/Login");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var apiUrl = $"https://localhost:7289/api/Jobs/search" +
                             $"?keyword={Filter.Keyword}" +
                             $"&location={Filter.Location}" +
                             $"&industry={Filter.Industry}";

                var filteredJobs = await _httpClient.GetFromJsonAsync<List<JobPostingDto>>(apiUrl);
                if (filteredJobs != null)
                    JobListings = filteredJobs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error filtering job listings: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostFollowAsync(int companyId)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/JobSeeker/Login");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = $"https://localhost:7289/api/JobSeeker/follow-company/{companyId}";
            var response = await _httpClient.PostAsync(apiUrl, null);

            if (!response.IsSuccessStatusCode)
                Console.WriteLine($"Follow request failed: {response.StatusCode}");

            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUnFollowAsync(int companyId)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/JobSeeker/Login");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = $"https://localhost:7289/api/JobSeeker/unfollow-company/{companyId}";
            var response = await _httpClient.DeleteAsync(apiUrl);


            if (!response.IsSuccessStatusCode)
                Console.WriteLine($"Unfollow request failed: {response.StatusCode}");

            await OnGetAsync();
            return Page();
        }
    }
}
