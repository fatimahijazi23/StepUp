using aspteamAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using aspteamAPI.IRepository;

namespace aspteamAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
        public class AuthController : ControllerBase
        {
            private readonly IAuthRepositories _repo;

            public AuthController(IAuthRepositories repo)
            {
                _repo = repo;
            }

            [HttpPost("register-jobseeker")]
            public async Task<IActionResult> RegisterJobSeeker([FromBody] RegisterJobSeekerDto dto)
            {
                var result = await _repo.RegisterJobSeekerAsync(dto);
                return Ok(result);
            }

            [HttpPost("register-company")]
            public async Task<IActionResult> RegisterCompany([FromBody] RegisterCompanyDto dto)
            {
                var result = await _repo.RegisterCompanyAsync(dto);
                return Ok(result);
            }

            [HttpPost("login-jobseeker")]
            public async Task<IActionResult> LoginJobSeeker([FromBody] LoginDto dto)
            {
                var result = await _repo.LoginJobSeekerAsync(dto);
                return Ok(result);
            }

            [HttpPost("login-company")]
            public async Task<IActionResult> LoginCompany([FromBody] LoginDto dto)
            {
                var result = await _repo.LoginCompanyAsync(dto);
                return Ok(result);
            }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromQuery] int testUserId = 1)
        {
            try
            {
                // For development - just use test user ID
                var userId = testUserId;
                var tokenId = Guid.NewGuid().ToString(); // Generate fake token ID

                var result = await _repo.LogoutAsync(tokenId, userId);

                if (result)
                {
                    return Ok(new LogoutResponseDto { Success = true, Message = "Successfully logged out" });
                }

                return Ok(new LogoutResponseDto { Success = false, Message = "Logout failed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LogoutResponseDto { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _repo.ForgotPasswordAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _repo.ResetPasswordAsync(dto);

                if (result.Success)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResetPasswordResponseDto
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }
    }

}
    

