using aspteamAPI.DTOs.aspteamAPI.DTOs;
using aspteamAPI.DTOs;

namespace aspteamAPI.IRepository
{
    public interface IJobSeekerRepository
    {
        Task NotifyEvaluationCompleteAsync(int jobSeekerId, int cvId);
        Task<bool> FollowCompanyAsync(int jobSeekerId, int companyId);
        Task<bool> UnfollowCompanyAsync(int jobSeekerId, int companyId);
        Task<IEnumerable<FollowedCompanyDto>> GetFollowedCompaniesAsync(int jobSeekerId);
        Task<CVEvaluationResponseDto> EvaluateCVAsync(int jobSeekerId, CVEvaluationRequestDto request);
        Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int userId);
        Task<bool> MarkNotificationAsReadAsync(int userId, int notificationId);
        Task<bool> IsCompanyFollowedAsync(int jobSeekerId, int companyId);
        Task<JobSeekerAccount?> GetJobSeekerByUserIdAsync(int userId);
        Task<CompanyAccount?> GetCompanyByIdAsync(int companyId);
    }
}
