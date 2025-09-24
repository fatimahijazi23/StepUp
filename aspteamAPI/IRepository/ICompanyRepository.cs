using Microsoft.Extensions.Hosting;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace aspteamAPI.IRepository
{
    public interface ICompanyRepository
    {
        //   Company Management APIs

        //GET /api/company
        //GET /api/company/{companyId}
        //GET /api/company/{companyId}/ followers

        public Task<List<CompanyAccount>> GetAllCompanies();
        public Task<CompanyAccount?> GetCompanyById(int companyId);
        public Task<List<Follow>>? GetFollowers(int companyId);


    }
}
