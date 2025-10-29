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

        public async Task<IActionResult> OnGetAsync(int id)  // Changed from jobId to id
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

                var apiUrl = $"https://localhost:7289/api/Jobs/{id}";
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
            Console.WriteLine($"=== OnPostAsync Debug ===");
            Console.WriteLine($"ResumeFile: {ResumeFile?.FileName ?? "NULL"}");
            Console.WriteLine($"JobDescription: {JobDescription ?? "NULL"}");
            Console.WriteLine($"JobDescription Length: {JobDescription?.Length ?? 0}");

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

                using var form = new MultipartFormDataContent();

                // Add file
                using var fileStream = ResumeFile.OpenReadStream();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(ResumeFile.ContentType);
                form.Add(fileContent, "cvFile", ResumeFile.FileName);

                // Add job description - make sure it's not empty
                var jobDescContent = new StringContent(JobDescription ?? string.Empty);
                form.Add(jobDescContent, "jobDescription");

                Console.WriteLine($"Sending to API with jobDescription: {JobDescription?.Substring(0, Math.Min(50, JobDescription.Length))}...");

                var apiUrl = "https://localhost:7289/api/Cv/analyze-upload";
                var response = await _httpClient.PostAsync(apiUrl, form);

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response Status: {response.StatusCode}");
                Console.WriteLine($"API Response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    AnalysisResult = result?.analysis;
                    ShowResults = true;
                    ResultMessage = $"✅ Your resume '{ResumeFile.FileName}' was analyzed successfully!";
                }
                else
                {
                    ResultMessage = $"⚠️ Error analyzing resume: {responseContent}";
                }
            }
            catch (Exception ex)
            {
                ResultMessage = $"⚠️ Error: {ex.Message}";
                Console.WriteLine($"Exception in OnPostAsync: {ex}");
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