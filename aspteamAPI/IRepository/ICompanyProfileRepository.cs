using aspteamAPI.DTOs;

namespace aspteamAPI
{
    public interface ICompanyProfileRepository
    {
        // GET /api/CompanyProfiles/{id}
        Task<UpdateCompanyProfileDTO> GetCompanyProfile(int companyId);

        // GET /api/CompanyProfiles/by-user/{userId} ✅ NEW
        Task<UpdateCompanyProfileDTO> GetCompanyProfileByUserId(int userId);

        // PATCH /api/CompanyProfiles/{id}
        Task<UpdateCompanyProfileDTO> UpdateCompanyProfile(UpdateCompanyProfileDTO dto);

        // DELETE /api/CompanyProfiles/{id}
        Task<CompanyAccount> DeleteCompanyAccount(int companyId);
    }
}