using aspteamAPI.context;
using aspteamAPI.IRepository;

namespace aspteamAPI.Repositories
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string email, string name, string resetToken)
        {
            try
            {
                // Get the reset URL from configuration
                var baseUrl = _config["App:BaseUrl"] ?? "https://localhost:7289";
                var resetUrl = $"{baseUrl}/reset-password?token={resetToken}&email={Uri.EscapeDataString(email)}";

                var subject = "Password Reset Request - Job Portal";
                var body = GeneratePasswordResetEmailBody(name, resetUrl);

                await SendEmailAsync(email, subject, body);

                _logger.LogInformation($"Password reset email sent to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send password reset email to {email}: {ex.Message}");
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string email, string name)
        {
            try
            {
                var subject = "Welcome to Job Portal!";
                var body = GenerateWelcomeEmailBody(name);

                await SendEmailAsync(email, subject, body);

                _logger.LogInformation($"Welcome email sent to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send welcome email to {email}: {ex.Message}");
                // Don't throw - welcome emails are not critical
            }
        }

        public async Task SendJobNotificationEmailAsync(string email, string jobTitle, string companyName)
        {
            try
            {
                var subject = $"New Job Alert: {jobTitle} at {companyName}";
                var body = GenerateJobNotificationEmailBody(jobTitle, companyName);

                await SendEmailAsync(email, subject, body);

                _logger.LogInformation($"Job notification email sent to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send job notification email to {email}: {ex.Message}");
                // Don't throw - notifications are not critical
            }
        }

        private async Task SendEmailAsync(string email, string subject, string body)
        {
            // OPTION 1: Using System.Net.Mail (SMTP)
            await SendEmailViaSMTP(email, subject, body);

            // OPTION 2: Using SendGrid (uncomment if you prefer)
            // await SendEmailViaSendGrid(email, subject, body);
        }

        #region SMTP Implementation
        private async Task SendEmailViaSMTP(string email, string subject, string body)
        {
            using var client = new System.Net.Mail.SmtpClient();

            // Configure SMTP settings from appsettings.json
            client.Host = _config["Email:Smtp:Host"] ?? "smtp.gmail.com";
            client.Port = int.Parse(_config["Email:Smtp:Port"] ?? "587");
            client.EnableSsl = bool.Parse(_config["Email:Smtp:EnableSsl"] ?? "true");
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(
                _config["Email:Smtp:Username"],
                _config["Email:Smtp:Password"]
            );

            var mailMessage = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(
                    _config["Email:From:Address"] ?? "noreply@jobportal.com",
                    _config["Email:From:Name"] ?? "Job Portal"
                ),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }
        #endregion

        #region SendGrid Implementation (Alternative)
        /*
        private async Task SendEmailViaSendGrid(string email, string subject, string body)
        {
            var apiKey = _config["Email:SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);

            var from = new EmailAddress(_config["Email:From:Address"], _config["Email:From:Name"]);
            var to = new EmailAddress(email);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, body);

            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode != HttpStatusCode.Accepted)
            {
                throw new Exception($"SendGrid failed with status: {response.StatusCode}");
            }
        }
        */
        #endregion

        #region Email Templates
        private string GeneratePasswordResetEmailBody(string name, string resetUrl)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>Password Reset</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f4f4f4; }}
                    .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; }}
                    .header {{ text-align: center; color: #333; border-bottom: 2px solid #007bff; padding-bottom: 20px; }}
                    .content {{ padding: 20px 0; line-height: 1.6; color: #555; }}
                    .button {{ display: inline-block; padding: 12px 30px; background: #007bff; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                    .footer {{ text-align: center; padding-top: 20px; border-top: 1px solid #eee; color: #888; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Password Reset Request</h1>
                    </div>
                    <div class='content'>
                        <p>Hello {name},</p>
                        <p>We received a request to reset your password for your Job Portal account.</p>
                        <p>Click the button below to reset your password:</p>
                        <p style='text-align: center;'>
                            <a href='{resetUrl}' class='button'>Reset Password</a>
                        </p>
                        <p>If the button doesn't work, copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #007bff;'>{resetUrl}</p>
                        <p><strong>This link will expire in 2 hours for security reasons.</strong></p>
                        <p>If you didn't request this password reset, please ignore this email.</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; 2024 Job Portal. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string GenerateWelcomeEmailBody(string name)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>Welcome to Job Portal</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f4f4f4; }}
                    .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; }}
                    .header {{ text-align: center; color: #333; border-bottom: 2px solid #28a745; padding-bottom: 20px; }}
                    .content {{ padding: 20px 0; line-height: 1.6; color: #555; }}
                    .footer {{ text-align: center; padding-top: 20px; border-top: 1px solid #eee; color: #888; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Welcome to Job Portal!</h1>
                    </div>
                    <div class='content'>
                        <p>Hello {name},</p>
                        <p>Welcome to Job Portal! Your account has been successfully created.</p>
                        <p>You can now:</p>
                        <ul>
                            <li>Browse and apply for jobs</li>
                            <li>Follow companies you're interested in</li>
                            <li>Get notified about new job postings</li>
                            <li>Track your applications</li>
                        </ul>
                        <p>Start exploring opportunities today!</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; 2024 Job Portal. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string GenerateJobNotificationEmailBody(string jobTitle, string companyName)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>New Job Alert</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f4f4f4; }}
                    .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; }}
                    .header {{ text-align: center; color: #333; border-bottom: 2px solid #ffc107; padding-bottom: 20px; }}
                    .content {{ padding: 20px 0; line-height: 1.6; color: #555; }}
                    .footer {{ text-align: center; padding-top: 20px; border-top: 1px solid #eee; color: #888; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>🔔 New Job Alert!</h1>
                    </div>
                    <div class='content'>
                        <p>Great news!</p>
                        <p>A company you're following has posted a new job:</p>
                        <h3>{jobTitle}</h3>
                        <p><strong>Company:</strong> {companyName}</p>
                        <p>Don't wait - great opportunities don't last long!</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; 2024 Job Portal. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
        }
        #endregion
    }
}

