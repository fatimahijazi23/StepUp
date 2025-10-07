using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
  
namespace aspteamWeb.Pages.JobSeeker
{
    public class ViewJobModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ViewJobModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public JobDetailsDto? Job { get; set; }

        // DTO for job details
        public class JobDetailsDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string CompanyName { get; set; }
            public string Location { get; set; }
            public string Type { get; set; }
            public string Level { get; set; }
            public string SalaryRange { get; set; }
            public DateTime PostedAt { get; set; }
            public string Description { get; set; }
            public string Requirements { get; set; }
            public string Benefits { get; set; }
        }

        // OnGet with job id
        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var apiUrl = $"https://localhost:7289/api/jobs/{id}"; // update API endpoint if needed
                var job = await _httpClient.GetFromJsonAsync<JobDetailsDto>(apiUrl);

                if (job == null)
                {
                    return NotFound();
                }

                Job = job;
                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching job details: {ex.Message}");
                return StatusCode(500, "Error loading job details.");
            }
        }
    }
}
