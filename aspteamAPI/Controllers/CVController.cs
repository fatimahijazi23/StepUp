using aspteamAPI.context;
using aspteamAPI.DTOs;
using aspteamAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using aspteamAPI.Migrations;
using UglyToad.PdfPig;

namespace aspteamAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CvController : ControllerBase
    {
        private readonly ICvRepository _cvRepository;

        public CvController(ICvRepository cvRepository)
        {
            _cvRepository = cvRepository;
        }

        // Existing method for analyzing by CV ID
        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeCv([FromForm] int cvId, [FromForm] string jobDescription)
        {
            var cv = await _cvRepository.GetCvByIdAsync(cvId);
            if (cv == null)
                return NotFound($"CV with ID = {cvId} not found");

            var filePath = Path.Combine("wwwroot/files", cv.FileUrl);
            if (!System.IO.File.Exists(filePath))
                return NotFound($"CV file '{cv.FileUrl}' not found on server");

            using var client = new HttpClient();
            using var form = new MultipartFormDataContent();
            using var fileStream = System.IO.File.OpenRead(filePath);

            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
            form.Add(fileContent, "cvFile", cv.FileUrl);
            form.Add(new StringContent(jobDescription), "jobDescription");

            var n8nWebhookUrl = "https://n8nfatima.ddns.net/webhook/resume-evaluator";
            var response = await client.PostAsync(n8nWebhookUrl, form);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Failed to process CV through n8n");

            var analysisJson = await response.Content.ReadAsStringAsync();
            return Ok(new { cvId = cv.Id, analysis = analysisJson });
        }

        // NEW method for analyzing uploaded file directly
        [HttpPost("analyze-upload")]
        public async Task<IActionResult> AnalyzeUploadedCv([FromForm] IFormFile cvFile, [FromForm] string jobDescription)
        {
            if (cvFile == null || cvFile.Length == 0)
                return BadRequest("No file uploaded");

            if (string.IsNullOrWhiteSpace(jobDescription))
                return BadRequest("Job description is required");

            try
            {
                using var client = new HttpClient();
                using var form = new MultipartFormDataContent();

                // Read file content
                using var fileStream = cvFile.OpenReadStream();
                var fileContent = new StreamContent(fileStream);

                // Set content type based on file extension
                var contentType = cvFile.ContentType;
                if (string.IsNullOrEmpty(contentType))
                {
                    contentType = cvFile.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                        ? "application/pdf"
                        : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                }

                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                form.Add(fileContent, "cvFile", cvFile.FileName);
                form.Add(new StringContent(jobDescription), "jobDescription");

                // Send to n8n webhook
                var n8nWebhookUrl = "https://n8nfatima.ddns.net/webhook/resume-evaluator";
                var response = await client.PostAsync(n8nWebhookUrl, form);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode,
                        new { message = "Failed to process CV through n8n", details = errorContent });
                }

                var analysisJson = await response.Content.ReadAsStringAsync();

                // Try to parse

                try
                {
                    var analysisResult = JsonConvert.DeserializeObject<dynamic>(analysisJson);
                    return Ok(new { success = true, analysis = analysisResult });
                }
                catch
                {
                    // If parsing fails, return raw JSON
                    return Ok(new { success = true, analysis = analysisJson });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing CV", error = ex.Message });
            }
        }
    }
}