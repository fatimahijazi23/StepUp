using aspteamAPI.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace aspteamAPI
{
    public interface ICompanyProfileRepository
    {
        //          GET /api/user/profile ✅
        //          PUT /api/user/profile ✅
        //          DELETE /api/user/account✅
        //          POST /api/user/upload-avatar✅
        //         
        public Task<UpdateCompanyProfileDTO> UpdateCompanyProfile(UpdateCompanyProfileDTO dto);

        public Task<UpdateCompanyProfileDTO> GetCompanyProfile(int companyId);

        public Task<CompanyAccount> DeleteCompanyAccount(int companyId);


    }
}
