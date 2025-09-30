using Microsoft.AspNetCore.Mvc;
using aspteamAPI.Repositories;

namespace aspteamAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IJobRepository _repo;

        public JobsController(IJobRepository repo)
        {
            _repo = repo;
        }

        // GET: api/jobs (with filters)
        [HttpGet]
        public async Task<IActionResult> GetJobs(
            [FromQuery] string? keyword,
            [FromQuery] Industry? industry,
            [FromQuery] ExperienceLevel? experienceLevel,
            [FromQuery] EmploymentType? employmentType,
            [FromQuery] WorkArrangement? workArrangement,
            [FromQuery] decimal? minSalary,
            [FromQuery] decimal? maxSalary,
            [FromQuery] string? location)
        {
            var jobs = await _repo.GetJobsAsync(keyword, industry, experienceLevel, employmentType, workArrangement, minSalary, maxSalary, location);
            return Ok(jobs);
        }

        // GET: api/jobs/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobDetails(int id)
        {
            var job = await _repo.GetJobDetailsAsync(id);
            if (job == null) return NotFound(new { message = "Job not found" });
            return Ok(job);
        }
    }
}