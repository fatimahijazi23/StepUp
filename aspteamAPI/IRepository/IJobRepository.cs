using aspteamAPI.DTOs;
using aspteamAPI.IRepository;

namespace aspteamAPI.Repositories
{
    public interface IJobRepository : IRepository<Job>
    {
        Task<IEnumerable<Job>> SearchAsync(string? keyword, string? location, Industry? industry);
        Task<IEnumerable<string>> GetLocationsAsync();
        Task<IEnumerable<Industry>> GetCategoriesAsync();
        Task<IEnumerable<Job>> GetByCompanyIdAsync(int companyId);
    }


}
