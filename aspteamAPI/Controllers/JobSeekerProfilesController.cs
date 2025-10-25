using aspteamAPI.DTOs;
using aspteamAPI.IRepository;
using aspteamAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace aspteamAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobSeekerProfilesController : ControllerBase
    {
        private readonly IJobSeekerProfileRepo _repo;
        private readonly IJobSeekerRepository _jobSeekerRepo;

        public JobSeekerProfilesController(IJobSeekerProfileRepo repo, IJobSeekerRepository jobSeekerRepo)
        {
            _repo = repo;
            _jobSeekerRepo= jobSeekerRepo;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID claim not found in JWT");
            return int.Parse(userIdClaim);
        }

        [HttpGet]
        public async Task<IActionResult> GetJobSeekerProfiles()
        {
            int UserId = GetCurrentUserId();
            var jobSeeker = await _jobSeekerRepo.GetJobSeekerByUserIdAsync(UserId);

            if (jobSeeker == null) return NotFound();

            var profile = await _repo.GetJobSeekerProfile(jobSeeker.Id);

            return Ok(profile);


        }

        // PATCH: api/jobseekerprofiles/{id}
        [HttpPatch]
        public async Task<IActionResult> EditJobSeekerProfile( [FromBody] UpdateJobSeekerProfileDTO dto)
        {
            int UserId = GetCurrentUserId();
            var jobSeeker= await _jobSeekerRepo.GetJobSeekerByUserIdAsync(UserId);

            if (dto == null)
                return BadRequest("Invalid data.");

            var updatedProfile = await _repo.UpdateJobSeekerProfile(jobSeeker.Id, dto);

            if (updatedProfile == null)
                return NotFound($"Job seeker with Id={jobSeeker.Id} not found.");

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
