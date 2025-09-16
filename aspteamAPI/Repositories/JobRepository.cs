using aspteamAPI.context;
using aspteamAPI.DTOs;
using aspteamAPI.IRepository;
using Microsoft.EntityFrameworkCore;

namespace aspteamAPI.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly AppDbContext _context;

        public JobRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobListDto>> GetJobsAsync(
            string? keyword,
            Industry? industry,
            ExperienceLevel? experienceLevel,
            EmploymentType? employmentType,
            WorkArrangement? workArrangement,
            decimal? minSalary,
            decimal? maxSalary,
            string? location)
        {
            var query = _context.Jobs
                .Include(j => j.Company)
                .Where(j => j.IsActive)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(j =>
                    (j.Description != null && j.Description.Contains(keyword)) ||
                    (j.Requirements != null && j.Requirements.Contains(keyword)) ||
                    j.Company.CompanyName.Contains(keyword));

            if (industry.HasValue)
                query = query.Where(j => j.Industry == industry);

            if (experienceLevel.HasValue)
                query = query.Where(j => j.ExperienceLevel == experienceLevel);

            if (employmentType.HasValue)
                query = query.Where(j => j.EmploymentType == employmentType);

            if (workArrangement.HasValue)
                query = query.Where(j => j.WorkArrangement == workArrangement);

            if (!string.IsNullOrEmpty(location))
                query = query.Where(j => j.Location != null && j.Location.Contains(location));

            if (minSalary.HasValue)
                query = query.Where(j => j.MinSalaryRange >= minSalary);

            if (maxSalary.HasValue)
                query = query.Where(j => j.MaxSalaryRange <= maxSalary);

            return await query
                .Select(j => new JobListDto
                {
                    Id = j.Id,
                    CompanyName = j.Company.CompanyName,
                    Description = j.Description,
                    Location = j.Location,
                    MinSalaryRange = j.MinSalaryRange,
                    MaxSalaryRange = j.MaxSalaryRange
                })
                .ToListAsync();
        }

        public async Task<JobDetailsDto?> GetJobDetailsAsync(int id)
        {
            var job = await _context.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null) return null;

            return new JobDetailsDto
            {
                Id = job.Id,
                CompanyName = job.Company.CompanyName,
                Description = job.Description,
                Requirements = job.Requirements,
                Industry = job.Industry,
                ExperienceLevel = job.ExperienceLevel,
                EmploymentType = job.EmploymentType,
                WorkArrangement = job.WorkArrangement,
                Location = job.Location,
                MinSalaryRange = job.MinSalaryRange,
                MaxSalaryRange = job.MaxSalaryRange,
                CreatedAt = job.CreatedAt
            };
        }
    }
}
