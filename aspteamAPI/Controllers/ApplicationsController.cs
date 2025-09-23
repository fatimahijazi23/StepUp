using aspteamAPI.DTOs;
using aspteamAPI.IRepository;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace aspteamAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class ApplicationsController : ControllerBase
    {
        private readonly IJobApplicationRepository _jobApplicationRepo;

        public ApplicationsController(IJobApplicationRepository jobApplicationRepo)
        {
            _jobApplicationRepo = jobApplicationRepo;
        }

        // POST /api/applications - Create new job application
        [HttpPost]
        public async Task<IActionResult> CreateApplication([FromBody] CreateJobApplicationDto dto, [FromQuery] int testUserId = 1)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // For development - use testUserId, in production use JWT
                int userId;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int jwtUserId))
                {
                    userId = jwtUserId; // Use JWT if available
                }
                else
                {
                    userId = testUserId; // Fallback to test user ID
                }

                var result = await _jobApplicationRepo.CreateApplicationAsync(dto, userId);

                if (result.Success)
                    return CreatedAtAction(nameof(GetApplication), new { applicationId = result.Data?.Id }, result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<JobApplicationResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the application"
                });
            }
        }

        // GET /api/applications/my-applications - Get current user's applications
        [HttpGet("my-applications")]
        public async Task<IActionResult> GetMyApplications([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int testUserId = 1)
        {
            try
            {
                // For development - use testUserId, in production use JWT
                int userId;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int jwtUserId))
                {
                    userId = jwtUserId; // Use JWT if available
                }
                else
                {
                    userId = testUserId; // Fallback to test user ID
                }

                var result = await _jobApplicationRepo.GetUserApplicationsAsync(userId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new PaginatedResponseDto<JobApplicationResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving applications"
                });
            }
        }

        // GET /api/applications/{applicationId} - Get specific application
        [HttpGet("{applicationId}")]
        public async Task<IActionResult> GetApplication(int applicationId, [FromQuery] int testUserId = 1)
        {
            try
            {
                // For development - use testUserId, in production use JWT
                int userId;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int jwtUserId))
                {
                    userId = jwtUserId; // Use JWT if available
                }
                else
                {
                    userId = testUserId; // Fallback to test user ID
                }

                var result = await _jobApplicationRepo.GetApplicationByIdAsync(applicationId, userId);

                if (result.Success)
                    return Ok(result);

                return NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<JobApplicationResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the application"
                });
            }
        }

        // DELETE /api/applications/{applicationId} - Delete application
        [HttpDelete("{applicationId}")]
        public async Task<IActionResult> DeleteApplication(int applicationId, [FromQuery] int testUserId = 1)
        {
            try
            {
                // For development - use testUserId, in production use JWT
                int userId;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int jwtUserId))
                {
                    userId = jwtUserId; // Use JWT if available
                }
                else
                {
                    userId = testUserId; // Fallback to test user ID
                }

                var result = await _jobApplicationRepo.DeleteApplicationAsync(applicationId, userId);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the application"
                });
            }
        }

        // GET /api/applications/job/{jobId} - Get all applications for a specific job
        [HttpGet("job/{jobId}")]
        public async Task<IActionResult> GetJobApplications(int jobId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized("Invalid user token");

                var result = await _jobApplicationRepo.GetJobApplicationsAsync(jobId, userId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new PaginatedResponseDto<JobApplicationResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving job applications"
                });
            }
        }

        // PUT /api/applications/{applicationId}/status - Update application status
        [HttpPut("{applicationId}/status")]
        public async Task<IActionResult> UpdateApplicationStatus(int applicationId, [FromBody] UpdateApplicationStatusDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized("Invalid user token");

                var result = await _jobApplicationRepo.UpdateApplicationStatusAsync(applicationId, dto, userId);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<JobApplicationResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while updating application status"
                });
            }
        }

        // POST /api/applications/{applicationId}/accept - Accept application
        [HttpPost("{applicationId}/accept")]
        public async Task<IActionResult> AcceptApplication(int applicationId, [FromBody] UpdateApplicationStatusDto? dto = null)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized("Invalid user token");

                var acceptDto = new UpdateApplicationStatusDto
                {
                    Status = ApplicationStatusBadge.Hired,
                    Notes = dto?.Notes ?? "Application accepted"
                };

                var result = await _jobApplicationRepo.UpdateApplicationStatusAsync(applicationId, acceptDto, userId);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<JobApplicationResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while accepting the application"
                });
            }
        }

        // POST /api/applications/{applicationId}/reject - Reject application
        [HttpPost("{applicationId}/reject")]
        public async Task<IActionResult> RejectApplication(int applicationId, [FromBody] UpdateApplicationStatusDto? dto = null)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized("Invalid user token");

                var rejectDto = new UpdateApplicationStatusDto
                {
                    Status = ApplicationStatusBadge.Rejected,
                    Notes = dto?.Notes ?? "Application rejected"
                };

                var result = await _jobApplicationRepo.UpdateApplicationStatusAsync(applicationId, rejectDto, userId);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<JobApplicationResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while rejecting the application"
                });
            }
        }

        // GET /api/applications/company/all - Get all applications for company
        [HttpGet("company/all")]
        public async Task<IActionResult> GetAllCompanyApplications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] ApplicationStatusBadge? status = null,
            [FromQuery] int testUserId = 1)
        {
            try
            {
                // For development - use testUserId, in production use JWT
                int userId;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int jwtUserId))
                {
                    userId = jwtUserId; // Use JWT if available
                }
                else
                {
                    userId = testUserId; // Fallback to test user ID
                }

                var result = await _jobApplicationRepo.GetCompanyApplicationsAsync(userId, page, pageSize, status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new PaginatedResponseDto<JobApplicationResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving company applications"
                });
            }
        }
    }
}
