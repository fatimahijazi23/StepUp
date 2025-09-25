using aspteamAPI.DTOs;
using aspteamAPI.IRepository;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace aspteamAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Ensure JWT authentication is required
    public class JobSeekerController : ControllerBase
    {
        private readonly IJobSeekerRepository _jobSeekerRepository;

        public JobSeekerController(IJobSeekerRepository jobSeekerRepository)
        {
            _jobSeekerRepository = jobSeekerRepository;
        }

        // Helper method to get the current user's ID from JWT
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID claim not found in JWT");
            return int.Parse(userIdClaim);
        }

        [HttpPost("follow-company/{companyId}")]
        public async Task<IActionResult> FollowCompany(int companyId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var jobSeeker = await _jobSeekerRepository.GetJobSeekerByUserIdAsync(userId);

                if (jobSeeker == null)
                    return BadRequest($"Job seeker account not found for user ID: {userId}");

                var result = await _jobSeekerRepository.FollowCompanyAsync(jobSeeker.Id, companyId);

                if (!result)
                    return BadRequest("Unable to follow company. Company may not exist or already followed.");

                return Ok(new { message = "Successfully followed company" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpDelete("unfollow-company/{companyId}")]
        public async Task<IActionResult> UnfollowCompany(int companyId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var jobSeeker = await _jobSeekerRepository.GetJobSeekerByUserIdAsync(userId);

                if (jobSeeker == null)
                    return BadRequest("Job seeker account not found");

                var result = await _jobSeekerRepository.UnfollowCompanyAsync(jobSeeker.Id, companyId);

                if (!result)
                    return BadRequest("Unable to unfollow company. You may not be following this company.");

                return Ok(new { message = "Successfully unfollowed company" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpGet("followed-companies")]
        public async Task<IActionResult> GetFollowedCompanies()
        {
            try
            {
                var userId = GetCurrentUserId();
                var jobSeeker = await _jobSeekerRepository.GetJobSeekerByUserIdAsync(userId);

                if (jobSeeker == null)
                    return BadRequest("Job seeker account not found");

                var companies = await _jobSeekerRepository.GetFollowedCompaniesAsync(jobSeeker.Id);
                return Ok(companies);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpPost("evaluate-cv")]
        public async Task<IActionResult> EvaluateCV([FromBody] CVEvaluationRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var jobSeeker = await _jobSeekerRepository.GetJobSeekerByUserIdAsync(userId);

                if (jobSeeker == null)
                    return BadRequest("Job seeker account not found");

                var evaluation = await _jobSeekerRepository.EvaluateCVAsync(jobSeeker.Id, request);
                return Ok(evaluation);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var userId = GetCurrentUserId();
                var notifications = await _jobSeekerRepository.GetNotificationsAsync(userId);
                return Ok(notifications);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpPut("notifications/{notificationId}/mark-read")]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _jobSeekerRepository.MarkNotificationAsReadAsync(userId, notificationId);

                if (!result)
                    return BadRequest("Notification not found or doesn't belong to current user");

                return Ok(new { message = "Notification marked as read" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpGet("debug/current-user")]
        public async Task<IActionResult> GetCurrentUserDebug()
        {
            var userId = GetCurrentUserId();
            var jobSeeker = await _jobSeekerRepository.GetJobSeekerByUserIdAsync(userId);

            return Ok(new
            {
                UserId = userId,
                HasJobSeekerAccount = jobSeeker != null,
                JobSeekerAccountId = jobSeeker?.Id
            });
        }
    }
}
