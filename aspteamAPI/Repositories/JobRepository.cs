using aspteamAPI.context;
using aspteamAPI.DTOs;
using aspteamAPI.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace aspteamAPI.Repositories
{

    public class JobRepository : IJobRepository
    {
        private readonly AppDbContext _context;

        public JobRepository(AppDbContext context)
        {
            _context = context;
        }

        

        public async Task<IEnumerable<Job>> GetAllAsync() =>
            await _context.Jobs.ToListAsync();

        public async Task<string?> GetCompanyNameByIdAsync(int companyId)
        {
            var company = await _context.CompanyAccounts.FindAsync(companyId);
            return company?.CompanyName;
        }



        public async Task<Job?> GetByIdAsync(int id) =>
            await _context.Jobs.FindAsync(id);

        public async Task AddAsync(Job entity)
        {
            await _context.Jobs.AddAsync(entity);
        }

        public void Update(Job entity)
        {
            _context.Jobs.Update(entity);
        }

        public void Delete(Job entity)
        {
            _context.Jobs.Remove(entity);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Job>> SearchAsync(string? keyword, string? location, Industry? industry)
        {
            var query = _context.Jobs.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(j => j.Description!.Contains(keyword) || j.Requirements!.Contains(keyword));

            if (!string.IsNullOrEmpty(location))
                query = query.Where(j => j.Location == location);

            if (industry.HasValue)
                query = query.Where(j => j.Industry == industry);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<string>> GetLocationsAsync()
        {
            return await _context.Jobs
                .Where(j => j.Location != null)
                .Select(j => j.Location!)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<Industry>> GetCategoriesAsync()
        {
            return await _context.Jobs
                .Where(j => j.Industry != null)
                .Select(j => j.Industry!.Value)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetByCompanyIdAsync(int companyId)
        {
            return await _context.Jobs
                .Where(j => j.PostedBy == companyId)
                .ToListAsync();
        }
   

        public async Task<bool> IsFollowingCompanyAsync(int companyId , int UserId)
        {
           var response = await  _context.Follows
                .AnyAsync(fc => fc.CompanyId == companyId && fc.JobSeekerId== UserId);
            Console.WriteLine($"IsFollowingCompanyAsync: CompanyId={companyId}, JobSeekerId={UserId}, Result={response}");
            return response;
        }
    }


}
