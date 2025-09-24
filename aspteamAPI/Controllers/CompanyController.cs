using aspteamAPI.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace aspteamAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : Controller
    {
        private readonly ICompanyRepository _repo;
        public CompanyController(ICompanyRepository repo)
        {
            _repo = repo;
        }


        //GET /api/company
        //GET /api/company/{companyId}
        //GET /api/company/{companyId}/ followers
        [HttpGet("/api/company")]
        public async Task<IActionResult> GetAllCompanies()
        {
            var companies =await  _repo.GetAllCompanies();
            if (companies == null || companies.Count==0)
            {
                return NotFound("No companies found.");
            }

            return Ok(companies);
        }
        [HttpGet("/api/company/{id}")]
        public async Task<IActionResult> GetCompany(int id)
        {

            var company = await _repo.GetCompanyById(id);
            if (company == null)
            {
                return NotFound($"No company found with id {id}.");
            }

            return Ok(company);
        }

        [HttpGet("/api/company/{companyId}/followers")]
        public async Task<IActionResult> GetCompanyFollowers(int companyId)
        {
            List<Follow> followers= await _repo.GetFollowers(companyId);
            if(followers == null || followers.Count==0 )
            {
                return NotFound($"No followers found for company with id {companyId}.");
            }
            return Ok(followers);
        }
    }
}
