using aspteamAPI.context;
using aspteamAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace aspteamAPI.Repositories
{
    public class CompanyProfileRepository : ICompanyProfileRepository
    {

        private AppDbContext _context;

        public  CompanyProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UpdateCompanyProfileDTO> GetCompanyProfile(int companyId)
        {
            
            var company = await _context.CompanyAccounts
                .Include(c => c.User)
                .SingleOrDefaultAsync(c => c.Id == companyId);

            if(company == null) {return null;}


            return new UpdateCompanyProfileDTO
            {
                Id = company.Id,
                ProfilePictureUrl = company.ProfilePictureUrl,
                CompanySize = company.CompanySize,
                CompanyName = company.User.Name,
                Email = company.User.Email,
                About = company.About
            };

        }

        public async Task<UpdateCompanyProfileDTO> UpdateCompanyProfile(UpdateCompanyProfileDTO dto)
        {
            var company = await _context.CompanyAccounts
                           .Include(c => c.User)
                           .FirstOrDefaultAsync(c => c.Id == dto.Id);

            if (company == null) {return null;}

            if(dto.Industry != null){ company.Industry = dto.Industry;}

            if (!string.IsNullOrEmpty(dto.ProfilePictureUrl)) { company.ProfilePictureUrl = dto.ProfilePictureUrl; }
            
            if (!string.IsNullOrEmpty(dto.CompanyName)) {  
                company.CompanyName = dto.CompanyName;
                company.User.Name = dto.CompanyName;
            }

            if (!string.IsNullOrEmpty(dto.Email)) { company.User.Email = dto.Email; }

            if (!string.IsNullOrEmpty(dto.About)) { company.About = dto.About; }   
            
            if(dto.CompanySize != null) {  company.CompanySize = dto.CompanySize;}


            // save changes to db
           await  _context.SaveChangesAsync();

            return new UpdateCompanyProfileDTO
            {
                Id = company.Id,
                ProfilePictureUrl = company.ProfilePictureUrl,
                CompanySize = company.CompanySize,
                CompanyName = company.User.Name,
                Email = company .User.Email,
                About = company.About
            };
        }


        public async Task<CompanyAccount?> DeleteCompanyAccount(int companyId)
        {
            var company = await _context.CompanyAccounts
                .Include(c => c.User) // load the parent UserAccount
                .SingleOrDefaultAsync(c => c.Id == companyId);

            if (company == null)
                return null;

            // Remove the UserAccount → cascades to CompanyAccount
            _context.Users.Remove(company.User);

            await _context.SaveChangesAsync();

            return company;
        }


    }
}
