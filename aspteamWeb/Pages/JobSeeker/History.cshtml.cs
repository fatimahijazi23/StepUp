using aspteamAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static aspteamWeb.Pages.JobSeeker.JobDetailsModel;

namespace aspteamWeb.Pages.JobSeeker
{
    public class HistoryModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public HistoryModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<Application> Applications { get; set; } = new();

        // Wrapper class matching your API response structure

        public enum ApplicationStatusBadge
        {
            Applied = 0,
            UnderReview = 1,
            Interview = 2,
            Rejected = 3,
            Hired = 4
        }
        public class ApplicationResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public List<Application>? Data { get; set; }
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
        }

        public class Application
        {
            public int Id { get; set; }
            public int ApplicantId { get; set; }
            public int JobId { get; set; }
            public int CvId { get; set; }
            public DateTime CreatedAt { get; set; }
            public ApplicationStatusBadge Status { get; set; }
            public JobSeekerBasicInfoDto? Applicant { get; set; }
            public JobBasicInfoDto? Job { get; set; }
            public CvBasicInfoDto? Cv { get; set; }
            public EvaluationBasicInfoDto? Evaluation { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                if (string.IsNullOrEmpty(token))
                    return RedirectToPage("/JobSeeker/Login");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var apiUrl = "https://localhost:7289/api/Applications/my-applications?page=1&pageSize=10&testUserId=2015";
                var response = await _httpClient.GetFromJsonAsync<ApplicationResponse>(apiUrl);

                Applications = response?.Data ?? new List<Application>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching job applications: {ex.Message}");
            }

            return Page();
        }
    }
}
