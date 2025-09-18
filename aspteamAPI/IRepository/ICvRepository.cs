using aspteamAPI.DTOs;

namespace aspteamAPI.Repositories
{
    public interface ICvRepository
    {
        // Get all CVs for a user
        Task<IEnumerable<CV>> GetCvsByUserIdAsync(int userId);

        // Get a single CV by its Id
        Task<CV?> GetCvByIdAsync(int cvId);

        // Add a new CV
        Task<CV> AddCvAsync(CV cv);

        // Delete a CV
        Task<bool> DeleteCvAsync(int cvId);
    }
}
