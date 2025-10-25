using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Headers;
using static aspteamWeb.Pages.JobSeeker.JobDetailsModel;

namespace aspteamWeb.Pages.JobSeeker
{ 
    public class ResumeTestModel : PageModel
    {
        private readonly HttpClient _httpClient;
        public ResumeTestModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }   
        [BindProperty]
        public IFormFile? ResumeFile { get; set; }

        [BindProperty]
        public string JobDescription { get; set; } = string.Empty;

        public string? ResultMessage { get; set; }

        public async  Task<IActionResult> OnGetAsync(int jobId)
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

                var apiUrl = $"https://localhost:7289/api/Jobs/{jobId}";
                var responseJob = await _httpClient.GetFromJsonAsync<JobResponse>(apiUrl);

                if (responseJob != null)
                    JobDescription = responseJob.Description;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching job Details: {ex.Message}");
            }

            return Page();
        }
        

        public async Task<IActionResult> OnPostAsync()
        {
            if (ResumeFile == null || ResumeFile.Length == 0)
            {
                ResultMessage = "⚠️ Please upload a resume file.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(JobDescription))
            {
                ResultMessage = "⚠️ Please enter a job description.";
                return Page();
            }

            // 🔹 Here you can implement your API call / AI analysis logic
            // For now, just mock the response
            await Task.Delay(500); // simulate processing
            ResultMessage = $"✅ Your resume '{ResumeFile.FileName}' was analyzed against the job description.";

            return Page();
        }
    }
}
