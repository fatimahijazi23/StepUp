using aspteamAPI.DTOs;

namespace aspteamAPI.Repositories
{
    public interface IJobRepository
    {
        Task<IEnumerable<JobListDto>> GetJobsAsync(
            string? keyword,
            Industry? industry,
            ExperienceLevel? experienceLevel,
            EmploymentType? employmentType,
            WorkArrangement? workArrangement,
            decimal? minSalary,
            decimal? maxSalary,
            string? location);

        Task<JobDetailsDto?> GetJobDetailsAsync(int id);
    }
}
