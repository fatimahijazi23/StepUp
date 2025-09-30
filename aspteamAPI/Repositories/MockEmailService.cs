using aspteamAPI.IRepository;

namespace aspteamAPI.Repositories
{
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string email, string name, string resetToken)
        {
            // Just log instead of actually sending email
            _logger.LogInformation($"📧 [MOCK] Password reset email to {email}");
            _logger.LogInformation($"📧 [MOCK] Reset token: {resetToken}");
            _logger.LogInformation($"📧 [MOCK] Reset URL: https://localhost:7289/reset-password?token={resetToken}&email={email}");

            await Task.CompletedTask; // Simulate async operation
        }

        public async Task SendWelcomeEmailAsync(string email, string name)
        {
            _logger.LogInformation($"📧 [MOCK] Welcome email sent to {email} for {name}");
            await Task.CompletedTask;
        }

        public async Task SendJobNotificationEmailAsync(string email, string jobTitle, string companyName)
        {
            _logger.LogInformation($"📧 [MOCK] Job notification: {jobTitle} at {companyName} sent to {email}");
            await Task.CompletedTask;
        }
    }
}
