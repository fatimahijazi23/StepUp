using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

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
        public dynamic? AnalysisResult { get; set; }
        public bool ShowResults { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(int jobId)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("JWT token missing. Redirecting to login.");
                    return Redirect("/JobSeeker/Login");
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
                ResultMessage = $"⚠️ Error loading job details: {ex.Message}";
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

            try
            {
                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                // Prepare multipart form data
                using var form = new MultipartFormDataContent();

                // Add file
                using var fileStream = ResumeFile.OpenReadStream();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(ResumeFile.ContentType);
                form.Add(fileContent, "cvFile", ResumeFile.FileName);

                // Add job description
                form.Add(new StringContent(JobDescription), "jobDescription");

                // Call API
                var apiUrl = "https://localhost:7289/api/Cv/analyze-upload";
                var response = await _httpClient.PostAsync(apiUrl, form);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                    AnalysisResult = result?.analysis;
                    ShowResults = true;
                    ResultMessage = $"✅ Your resume '{ResumeFile.FileName}' was analyzed successfully!";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ResultMessage = $"⚠️ Error analyzing resume: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                ResultMessage = $"⚠️ Error: {ex.Message}";
                Console.WriteLine($"Error analyzing CV: {ex}");
            }

            return Page();
        }

        public class JobResponse
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public string Company { get; set; } = string.Empty;
        }
    }
}