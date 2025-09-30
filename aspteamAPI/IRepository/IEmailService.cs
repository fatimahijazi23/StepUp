namespace aspteamAPI.IRepository
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string name, string resetToken);
        Task SendWelcomeEmailAsync(string email, string name);
        Task SendJobNotificationEmailAsync(string email, string jobTitle, string companyName);
    }
}
