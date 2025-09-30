using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;

namespace aspteamWeb.Pages.JobSeeker
{


        public class JobSeekerDashboardModel : PageModel
        {
            private readonly HttpClient _httpClient;

            public JobSeekerDashboardModel(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            public List<JobPostingDto> JobListings { get; set; } = new();

            public class JobPostingDto
            {
                public int Id { get; set; }
                public string Title { get; set; }
                public string CompanyName { get; set; }
                public string Location { get; set; }
                public string Type { get; set; }
                public string Level { get; set; }
                public string SalaryRange { get; set; }
                public DateTime PostedAt { get; set; }
            }

            public async Task OnGetAsync()
            {
                try
                {
                    var apiUrl = "https://localhost:7289/api/jobs"; // adjust to your real API endpoint
                    var jobs = await _httpClient.GetFromJsonAsync<List<JobPostingDto>>(apiUrl);

                    if (jobs != null)
                        JobListings = jobs;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching job listings: {ex.Message}");
                }
            }
        }
    }



