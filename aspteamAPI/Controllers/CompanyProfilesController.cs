using aspteamAPI.DTOs;
using aspteamAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace aspteamAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyProfilesController : Controller
    {
        private ICompanyProfileRepository _repo;

        public CompanyProfilesController(ICompanyProfileRepository repo)
        {
            _repo = repo;
        }

        // GET: api/CompanyProfiles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompanyProfile(int id)
        {
            var companyProfile = await _repo.GetCompanyProfile(id);

            if (companyProfile == null)
            {
                return NotFound();
            }

            return Ok(companyProfile);
        }

        // ✅ NEW: GET: api/CompanyProfiles/by-user/{userId}
        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetCompanyProfileByUserId(int userId)
        {
            var companyProfile = await _repo.GetCompanyProfileByUserId(userId);

            if (companyProfile == null)
            {
                return NotFound($"Company profile not found for UserId={userId}");
            }

            return Ok(companyProfile);
        }

        // PATCH: api/CompanyProfiles/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> EditCompanyProfile(int id, [FromBody] UpdateCompanyProfileDTO dto)
        {
            if (dto == null || id != dto.Id)
                return BadRequest("Invalid data.");

            var updatedCompany = await _repo.UpdateCompanyProfile(dto);

            if (updatedCompany == null)
            {
                return NotFound($"Company with Id={id} not found.");
            }

            return Ok(updatedCompany);
        }

        // DELETE: api/CompanyProfiles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompanyAccount(int id)
        {
            var company = await _repo.DeleteCompanyAccount(id);

            if (company == null)
                return NotFound();

            return Ok(company);
        }
    }
}