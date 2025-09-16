using aspteamAPI.context;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using aspteamAPI.DTOs;
using aspteamAPI.Models;
using aspteamAPI.IRepository;


namespace aspteamAPI.Repositories
{
    public class AuthRepository : IAuthRepositories
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService; // You'll need to create this


        public AuthRepository(AppDbContext context, IConfiguration config , IEmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;

        }

        public async Task<bool> LogoutAsync(string tokenId, int userId)
        {
            try
            {
                // Extract expiry from the token or set a reasonable expiry
                var expiresAt = DateTime.UtcNow.AddHours(12); // Match your JWT expiry

                var blacklistedToken = new BlacklistedToken
                {
                    TokenId = tokenId,
                    ExpiresAt = expiresAt,
                    UserId = userId,
                    BlacklistedAt = DateTime.UtcNow
                };

                _context.BlacklistedTokens.Add(blacklistedToken);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsTokenBlacklistedAsync(string tokenId)
        {
            return await _context.BlacklistedTokens
                .AnyAsync(bt => bt.TokenId == tokenId && bt.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                {
                    // Don't reveal if email exists or not for security
                    return new ForgotPasswordResponseDto
                    {
                        Success = true,
                        Message = "If the email exists, a reset link has been sent."
                    };
                }

                // Generate secure reset token
                var resetToken = GenerateSecureToken();
                var expiresAt = DateTime.UtcNow.AddHours(2); // 2 hours expiry

                // Invalidate any existing reset tokens for this user
                var existingTokens = await _context.PasswordResetTokens
                    .Where(t => t.UserId == user.Id && !t.IsUsed)
                    .ToListAsync();

                foreach (var token in existingTokens)
                {
                    token.IsUsed = true;
                    token.UsedAt = DateTime.UtcNow;
                }

                // Create new reset token
                var passwordResetToken = new PasswordResetToken
                {
                    UserId = user.Id,
                    Token = resetToken,
                    ExpiresAt = expiresAt
                };

                _context.PasswordResetTokens.Add(passwordResetToken);
                await _context.SaveChangesAsync();

                // Send reset email
                await _emailService.SendPasswordResetEmailAsync(user.Email, user.Name, resetToken);

                return new ForgotPasswordResponseDto
                {
                    Success = true,
                    Message = "If the email exists, a reset link has been sent."
                };
            }
            catch (Exception ex)
            {
                return new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = "An error occurred. Please try again later."
                };
            }
        }

        public async Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null)
                {
                    return new ResetPasswordResponseDto
                    {
                        Success = false,
                        Message = "Invalid reset request."
                    };
                }

                var resetToken = await _context.PasswordResetTokens
                    .FirstOrDefaultAsync(t => t.Token == dto.Token &&
                                             t.UserId == user.Id &&
                                             !t.IsUsed &&
                                             t.ExpiresAt > DateTime.UtcNow);

                if (resetToken == null)
                {
                    return new ResetPasswordResponseDto
                    {
                        Success = false,
                        Message = "Invalid or expired reset token."
                    };
                }

                // Update password
                user.PasswordHash = HashPassword(dto.NewPassword);

                // Mark token as used
                resetToken.IsUsed = true;
                resetToken.UsedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ResetPasswordResponseDto
                {
                    Success = true,
                    Message = "Password reset successfully. Please login with your new password."
                };
            }
            catch (Exception ex)
            {
                return new ResetPasswordResponseDto
                {
                    Success = false,
                    Message = "An error occurred. Please try again later."
                };
            }
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var expiredBlacklistedTokens = await _context.BlacklistedTokens
                .Where(bt => bt.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            var expiredResetTokens = await _context.PasswordResetTokens
                .Where(rt => rt.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            _context.BlacklistedTokens.RemoveRange(expiredBlacklistedTokens);
            _context.PasswordResetTokens.RemoveRange(expiredResetTokens);

            await _context.SaveChangesAsync();
        }

        private string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        public async Task<AuthResponseDto> RegisterJobSeekerAsync(RegisterJobSeekerDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new Exception("Email already exists");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                Role = UserRole.JobSeeker,
                JobSeekerAccount = new JobSeekerAccount()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                Role = user.Role.ToString(),
                Token = GenerateJwtToken(user)
            };
        }

        public async Task<AuthResponseDto> RegisterCompanyAsync(RegisterCompanyDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new Exception("Email already exists");

            var user = new User
            {
                Name = dto.CompanyName,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                Role = UserRole.Company,
                CompanyAccount = new CompanyAccount
                {
                    CompanyName = dto.CompanyName,
                    CompanySize = dto.CompanySize,  

                    Industry = dto.Industry,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                Role = user.Role.ToString(),
                Token = GenerateJwtToken(user)
            };
        }

        public async Task<AuthResponseDto> LoginJobSeekerAsync(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.JobSeekerAccount)
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Role == UserRole.JobSeeker);

            if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            return new AuthResponseDto
            {
                UserId = user.Id,
                Role = user.Role.ToString(),
                Token = GenerateJwtToken(user)
            };
        }

        public async Task<AuthResponseDto> LoginCompanyAsync(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.CompanyAccount)
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Role == UserRole.Company);

            if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            return new AuthResponseDto
            {
                UserId = user.Id,
                Role = user.Role.ToString(),
                Token = GenerateJwtToken(user)
            };
        }

        // FIXED: Secure password hashing with salt
        private string HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt = new byte[128 / 8];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            // Hash the password with the salt using PBKDF2
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            ));

            // Store salt and hash together (salt:hash)
            return Convert.ToBase64String(salt) + ":" + hashed;
        }

        // FIXED: Proper password verification
        private bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                // Split the stored hash to get salt and hash
                var parts = storedHash.Split(':');
                if (parts.Length != 2) return false;

                var salt = Convert.FromBase64String(parts[0]);
                var hash = parts[1];

                // Hash the provided password with the same salt
                string computedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8
                ));

                // Compare the hashes
                return hash == computedHash;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}