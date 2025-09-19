using Microsoft.AspNetCore.Mvc;
using aspteamAPI.Repositories;

namespace aspteamAPI.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IJobRepository _jobRepository;

        public JobsController(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        // GET /api/jobs
        [HttpGet]
        public async Task<IActionResult> GetJobs()
        {
            var jobs = await _jobRepository.GetAllAsync();
            return Ok(jobs);
        }

        // GET /api/jobs/{jobId}
        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetJob(int jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null) return NotFound();
            return Ok(job);
        }

        // GET /api/jobs/search?keyword=developer
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var jobs = await _jobRepository.SearchAsync(keyword);
            return Ok(jobs);
        }

        // GET /api/jobs/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _jobRepository.GetCategoriesAsync();
            return Ok(categories);
        }

        // GET /api/jobs/locations
        [HttpGet("locations")]
        public async Task<IActionResult> GetLocations()
        {
            var locations = await _jobRepository.GetLocationsAsync();
            return Ok(locations);
        }

        // POST /api/jobs
        [HttpPost]
        public async Task<IActionResult> CreateJob(Job job)
        {
            await _jobRepository.AddAsync(job);
            return CreatedAtAction(nameof(GetJob), new { jobId = job.Id }, job);
        }

        // PUT /api/jobs/{jobId}
        [HttpPut("{jobId}")]
        public async Task<IActionResult> UpdateJob(int jobId, Job job)
        {
            if (jobId != job.Id) return BadRequest();
            await _jobRepository.UpdateAsync(job);
            return NoContent();
        }

        // DELETE /api/jobs/{jobId}
        [HttpDelete("{jobId}")]
        public async Task<IActionResult> DeleteJob(int jobId)
        {
            await _jobRepository.DeleteAsync(jobId);
            return NoContent();
        }

        // GET /api/jobs/company/{companyId}
        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetJobsByCompany(int companyId)
        {
            var jobs = await _jobRepository.GetByCompanyIdAsync(companyId);
            return Ok(jobs);
        }
    }

}