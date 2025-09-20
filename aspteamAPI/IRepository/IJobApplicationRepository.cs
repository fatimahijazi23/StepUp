using aspteamAPI.DTOs;

namespace aspteamAPI.IRepository
{
    public interface IJobApplicationRepository
    {
        Task<ApiResponseDto<JobApplicationResponseDto>> CreateApplicationAsync(CreateJobApplicationDto dto, int userId);
        Task<PaginatedResponseDto<JobApplicationResponseDto>> GetUserApplicationsAsync(int userId, int page, int pageSize);
        Task<ApiResponseDto<JobApplicationResponseDto>> GetApplicationByIdAsync(int applicationId, int userId);
        Task<ApiResponseDto<object>> DeleteApplicationAsync(int applicationId, int userId);
        Task<PaginatedResponseDto<JobApplicationResponseDto>> GetJobApplicationsAsync(int jobId, int userId, int page, int pageSize);
        Task<ApiResponseDto<JobApplicationResponseDto>> UpdateApplicationStatusAsync(int applicationId, UpdateApplicationStatusDto dto, int userId);
        Task<PaginatedResponseDto<JobApplicationResponseDto>> GetCompanyApplicationsAsync(int userId, int page, int pageSize, ApplicationStatusBadge? status = null);
    }
}
