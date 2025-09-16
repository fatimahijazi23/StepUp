using aspteamAPI.DTOs;

namespace aspteamAPI.IRepository
{
    public interface IAuthRepositories
    {
        Task<AuthResponseDto> RegisterJobSeekerAsync(RegisterJobSeekerDto dto);
        Task<AuthResponseDto> RegisterCompanyAsync(RegisterCompanyDto dto);
        Task<AuthResponseDto> LoginJobSeekerAsync(LoginDto dto);
        Task<AuthResponseDto> LoginCompanyAsync(LoginDto dto);

        Task<bool> LogoutAsync(string tokenId, int userId);
        Task<bool> IsTokenBlacklistedAsync(string tokenId);
        Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordDto dto);
        Task CleanupExpiredTokensAsync(); // Background task to clean old tokens

    }
}
