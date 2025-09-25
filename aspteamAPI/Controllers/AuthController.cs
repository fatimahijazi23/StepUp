using aspteamAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using aspteamAPI.IRepository;
using System.IdentityModel.Tokens.Jwt;
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


        //Registration for both JobSeeker And Compnay:

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



        //Login for both JobSeeker And Compnay:

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

        //Logout for both :
        [HttpPost("logout")]
        [Authorize] // Add authorization to ensure valid token
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Get user ID from JWT claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new LogoutResponseDto { Success = false, Message = "Invalid token" });
                }

                var userId = int.Parse(userIdClaim.Value);

                // Get JTI (token ID) from JWT claims
                var jtiClaim = User.FindFirst(JwtRegisteredClaimNames.Jti);
                if (jtiClaim == null)
                {
                    return BadRequest(new LogoutResponseDto { Success = false, Message = "Token ID not found" });
                }

                var tokenId = jtiClaim.Value;

                var result = await _repo.LogoutAsync(tokenId, userId);

                if (result)
                {
                    return Ok(new LogoutResponseDto { Success = true, Message = "Successfully logged out" });
                }

                return BadRequest(new LogoutResponseDto { Success = false, Message = "Logout failed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LogoutResponseDto { Success = false, Message = "An error occurred" });
            }
        }



        //Forgot Password:

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


        //Reset Password :

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
    

