using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;

namespace aspteamWeb.Pages.Company
{
    public class ComanyDashboardModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ComanyDashboardModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<JobPostingDto> JobPostings { get; set; } = new();

        public class JobPostingDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Location { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public async Task OnGetAsync()
        {
            try
            {
                var apiUrl = "https://localhost:7289/api/jobs/company/1"; // example company id
                var jobs = await _httpClient.GetFromJsonAsync<List<JobPostingDto>>(apiUrl);

                if (jobs != null)
                    JobPostings = jobs;
            }
            catch (Exception ex)
            {
                // Log error or show friendly message
                Console.WriteLine($"Error fetching jobs: {ex.Message}");
            }
        }
    }
}
