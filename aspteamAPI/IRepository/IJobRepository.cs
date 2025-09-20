using aspteamAPI.DTOs;

namespace aspteamAPI.Repositories
{
    public interface IJobRepository : IRepository<Job>
    {
        Task<IEnumerable<Job>> SearchAsync(string keyword);
        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<IEnumerable<string>> GetLocationsAsync();
        Task<IEnumerable<Job>> GetByCompanyIdAsync(int companyId);
    }

}
