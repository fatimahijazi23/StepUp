using aspteamAPI.DTOs;
using aspteamAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace aspteamAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobSeekerProfilesController : ControllerBase
    {
        private readonly IJobSeekerProfileRepo _repo;

        public JobSeekerProfilesController(IJobSeekerProfileRepo repo)
        {
            _repo = repo;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobSeekerProfiles(int id)
        {
            var jobSeeker = await _repo.GetJobSeekerProfile(id);

            if (jobSeeker == null) return NotFound();
            return Ok(jobSeeker);


        }

        // PATCH: api/jobseekerprofiles/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> EditJobSeekerProfile(int id, [FromBody] UpdateJobSeekerProfileDTO dto)
        {
            if (dto == null || id != dto.Id)
                return BadRequest("Invalid data.");

            var updatedProfile = await _repo.UpdateJobSeekerProfile(dto);

            if (updatedProfile == null)
                return NotFound($"Job seeker with Id={id} not found.");

            return Ok(updatedProfile); 
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJobSeekerProfile(int id)
        {
            var joobSeeker = await _repo.DeleteJobSeekerProfile(id);

            if(joobSeeker == null) return NotFound();
            return Ok(joobSeeker);
        }
    }
}
