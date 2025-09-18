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
        public CompanyProfilesController(ICompanyProfileRepository repo) { _repo = repo; }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompanyProfile(int id)
        {
            var companyProfile = await _repo.GetCompanyProfile(id);
            if (companyProfile == null) { return NotFound(); }
            return Ok(companyProfile);

        }

        // PATCH: api/companyprofiles/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> EditCompanyProfile(int id, [FromBody] UpdateCompanyProfileDTO dto)
        {
            if (dto == null || id != dto.Id)
                return BadRequest("Invalid data.");


            var updatedCompany = await _repo.UpdateCompanyProfile(dto);

            if (updatedCompany == null) { return NotFound($"Company with Id={id} not found."); }

            return Ok(updatedCompany);


        }

        [HttpDelete ("{id}")]
        public async Task<IActionResult> DeleteCompanyAccount(int id)
        {
            var company = await _repo.DeleteCompanyAccount(id);

            if (company == null) return NotFound();
            return Ok(company);

        }
       
    }
}
