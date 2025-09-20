using aspteamAPI.context;
using aspteamAPI.DTOs;
using aspteamAPI.IRepository;
using Microsoft.EntityFrameworkCore;

namespace aspteamAPI.Repositories
{
    using Microsoft.EntityFrameworkCore;

    public class JobRepository : Repository<Job>, IJobRepository
    {
        private readonly AppDbContext _context;

        public JobRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Job>> SearchAsync(string keyword)
        {
            return await _context.Jobs
                .Include(j => j.Company)
                .Where(j => j.Description.Contains(keyword) ||
                            j.Requirements.Contains(keyword) ||
                            j.Company.CompanyName.Contains(keyword))
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _context.Jobs
                .Where(j => j.Industry != null)
                .Select(j => j.Industry.ToString()!)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetLocationsAsync()
        {
            return await _context.Jobs
                .Where(j => j.Location != null)
                .Select(j => j.Location!)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetByCompanyIdAsync(int companyId)
        {
            return await _context.Jobs
                .Where(j => j.PostedBy == companyId)
                .Include(j => j.Company)
                .ToListAsync();
        }
    }

}
