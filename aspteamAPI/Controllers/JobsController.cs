using aspteamAPI.DTOs;
using aspteamAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace aspteamAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobRepository _jobRepository;

        public JobsController(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        // GET /api/jobs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobDto>>> GetJobs()
        {
            var jobs = await _jobRepository.GetAllAsync();
            return Ok(jobs.Select(ToDto));
        }

        // GET /api/jobs/{jobId}
        [HttpGet("{jobId}")]
        public async Task<ActionResult<JobDto>> GetJob(int jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null) return NotFound();
            return Ok(ToDto(job));
        }

        // GET /api/jobs/search?keyword=dev&location=NY&industry=IT
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<JobDto>>> Search([FromQuery] string? keyword, [FromQuery] string? location, [FromQuery] Industry? industry)
        {
            var jobs = await _jobRepository.SearchAsync(keyword, location, industry);
            return Ok(jobs.Select(ToDto));
        }

        // GET /api/jobs/categories
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<Industry>>> GetCategories()
        {
            var categories = await _jobRepository.GetCategoriesAsync();
            return Ok(categories);
        }

        // GET /api/jobs/locations
        [HttpGet("locations")]
        public async Task<ActionResult<IEnumerable<string>>> GetLocations()
        {
            var locations = await _jobRepository.GetLocationsAsync();
            return Ok(locations);
        }

        // POST /api/jobs
        [HttpPost]
        public async Task<ActionResult<JobDto>> CreateJob(CreateJobDto dto)
        {
            var job = new Job
            {
                PostedBy = dto.PostedBy,
                Description = dto.Description,
                Requirements = dto.Requirements,
                Location = dto.Location,
                Industry = dto.Industry,
                ExperienceLevel = dto.ExperienceLevel,
                EmploymentType = dto.EmploymentType,
                WorkArrangement = dto.WorkArrangement,
                MaxSalaryRange = dto.MaxSalaryRange,
                MinSalaryRange = dto.MinSalaryRange,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _jobRepository.AddAsync(job);
            await _jobRepository.SaveAsync();

            return CreatedAtAction(nameof(GetJob), new { jobId = job.Id }, ToDto(job));
        }

        // PUT /api/jobs/{jobId}
        [HttpPut("{jobId}")]
        public async Task<IActionResult> UpdateJob(int jobId, UpdateJobDto dto)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null) return NotFound();

            job.Description = dto.Description;
            job.Requirements = dto.Requirements;
            job.Location = dto.Location;
            job.Industry = dto.Industry;
            job.ExperienceLevel = dto.ExperienceLevel;
            job.EmploymentType = dto.EmploymentType;
            job.WorkArrangement = dto.WorkArrangement;
            job.MaxSalaryRange = dto.MaxSalaryRange;
            job.MinSalaryRange = dto.MinSalaryRange;
            job.IsActive = dto.IsActive;

            _jobRepository.Update(job);
            await _jobRepository.SaveAsync();

            return NoContent();
        }

        // DELETE /api/jobs/{jobId}
        [HttpDelete("{jobId}")]
        public async Task<IActionResult> DeleteJob(int jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null) return NotFound();

            _jobRepository.Delete(job);
            await _jobRepository.SaveAsync();

            return NoContent();
        }

        // GET /api/jobs/company/{companyId}
        [HttpGet("company/{companyId}")]
        public async Task<ActionResult<IEnumerable<JobDto>>> GetJobsByCompany(int companyId)
        {
            var jobs = await _jobRepository.GetByCompanyIdAsync(companyId);
            return Ok(jobs.Select(ToDto));
        }

        // Helper: map entity -> DTO
        private static JobDto ToDto(Job job) =>
            new JobDto
            {
                Id = job.Id,
                PostedBy = job.PostedBy,
                Description = job.Description,
                Requirements = job.Requirements,
                Location = job.Location,
                Industry = job.Industry,
                ExperienceLevel = job.ExperienceLevel,
                EmploymentType = job.EmploymentType,
                WorkArrangement = job.WorkArrangement,
                MaxSalaryRange = job.MaxSalaryRange,
                MinSalaryRange = job.MinSalaryRange,
                CreatedAt = job.CreatedAt,
                IsActive = job.IsActive
            };
    }


}