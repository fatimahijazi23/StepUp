using aspteamAPI.context;
using aspteamAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace aspteamAPI.Repositories
{
    public class CompanyProfileRepository : ICompanyProfileRepository
    {
        private AppDbContext _context;

        public CompanyProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        // GET by CompanyId
        public async Task<UpdateCompanyProfileDTO> GetCompanyProfile(int companyId)
        {
            var company = await _context.CompanyAccounts
                .Include(c => c.User)
                .SingleOrDefaultAsync(c => c.Id == companyId);

            if (company == null) { return null; }

            return new UpdateCompanyProfileDTO
            {
                Id = company.Id,
                ProfilePictureUrl = company.ProfilePictureUrl,
                CompanySize = company.CompanySize,
                CompanyName = company.CompanyName,
                Email = company.User.Email,
                About = company.About,
                Industry = company.Industry
            };
        }

        // GET by UserId
        public async Task<UpdateCompanyProfileDTO> GetCompanyProfileByUserId(int userId)
        {
            var company = await _context.CompanyAccounts
                .Include(c => c.User)
                .SingleOrDefaultAsync(c => c.UserId == userId);

            if (company == null) { return null; }

            return new UpdateCompanyProfileDTO
            {
                Id = company.Id,
                ProfilePictureUrl = company.ProfilePictureUrl,
                CompanySize = company.CompanySize,
                CompanyName = company.CompanyName,
                Email = company.User.Email,
                About = company.About,
                Industry = company.Industry
            };
        }

        // UPDATE Company Profile
        public async Task<UpdateCompanyProfileDTO> UpdateCompanyProfile(UpdateCompanyProfileDTO dto)
        {
            var company = await _context.CompanyAccounts
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == dto.Id);

            if (company == null) { return null; }

            // Update Industry
            if (dto.Industry != null)
            {
                company.Industry = dto.Industry;
            }

            // ✅ FIX: Update ProfilePictureUrl (was missing proper update)
            if (!string.IsNullOrEmpty(dto.ProfilePictureUrl))
            {
                company.ProfilePictureUrl = dto.ProfilePictureUrl;
            }

            // ✅ FIX: Update CompanyName in BOTH places
            if (!string.IsNullOrEmpty(dto.CompanyName))
            {
                company.CompanyName = dto.CompanyName;
                company.User.Name = dto.CompanyName; // Keep User.Name in sync
            }

            // ✅ FIX: Update Email in User table
            if (!string.IsNullOrEmpty(dto.Email))
            {
                company.User.Email = dto.Email;
            }

            // Update About
            if (!string.IsNullOrEmpty(dto.About))
            {
                company.About = dto.About;
            }

            // Update CompanySize
            if (dto.CompanySize != null)
            {
                company.CompanySize = dto.CompanySize;
            }

            // ✅ CRITICAL: Save changes to database
            await _context.SaveChangesAsync();

            // ✅ FIX: Return the correct CompanyName from company.CompanyName (not User.Name)
            return new UpdateCompanyProfileDTO
            {
                Id = company.Id,
                ProfilePictureUrl = company.ProfilePictureUrl,
                CompanySize = company.CompanySize,
                CompanyName = company.CompanyName, // ✅ WAS: company.User.Name
                Email = company.User.Email,
                About = company.About,
                Industry = company.Industry
            };
        }

        // DELETE Company Account
        public async Task<CompanyAccount?> DeleteCompanyAccount(int companyId)
        {
            var company = await _context.CompanyAccounts
                .Include(c => c.User)
                .SingleOrDefaultAsync(c => c.Id == companyId);

            if (company == null)
                return null;

            _context.Users.Remove(company.User);
            await _context.SaveChangesAsync();

            return company;
        }
    }
}