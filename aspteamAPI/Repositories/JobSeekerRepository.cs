using aspteamAPI.DTOs.aspteamAPI.DTOs;
using aspteamAPI.DTOs;
using aspteamAPI.IRepository;
using aspteamAPI.Models;
using aspteamAPI.context;
using Microsoft.EntityFrameworkCore;

namespace aspteamAPI.Repositories
{
    public class JobSeekerRepository : IJobSeekerRepository
    {
        private readonly AppDbContext _context;

        public JobSeekerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> FollowCompanyAsync(int jobSeekerId, int companyId)
        {
            try
            {
                // Check if already following
                var existingFollow = await _context.Follows
                    .FirstOrDefaultAsync(f => f.JobSeekerId == jobSeekerId && f.CompanyId == companyId);

                if (existingFollow != null)
                    return false; // Already following

                // Check if company exists
                var company = await GetCompanyByIdAsync(companyId);
                if (company == null)
                    return false;

                var follow = new Follow
                {
                    JobSeekerId = jobSeekerId,
                    CompanyId = companyId,
                    FollowedAt = DateTime.UtcNow
                };

                _context.Follows.Add(follow);

                // Create notification for company
                var notification = new Notification
                {
                    UserId = company.UserId,
                    Title = "New Follower",
                    Message = "A job seeker started following your company",
                    Type = "follow"
                };
                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UnfollowCompanyAsync(int jobSeekerId, int companyId)
        {
            try
            {
                var follow = await _context.Follows
                    .FirstOrDefaultAsync(f => f.JobSeekerId == jobSeekerId && f.CompanyId == companyId);

                if (follow == null)
                    return false;

                _context.Follows.Remove(follow);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<FollowedCompanyDto>> GetFollowedCompaniesAsync(int jobSeekerId)
        {
            return await _context.Follows
                .Where(f => f.JobSeekerId == jobSeekerId)
                .Include(f => f.Company)
                .Select(f => new FollowedCompanyDto
                {
                    Id = f.Company.Id,
                    CompanyName = f.Company.CompanyName ?? "Unknown",
                    About = f.Company.About,
                    Industry = f.Company.Industry.ToString(),
                    FollowedAt = f.FollowedAt
                })
                .ToListAsync();
        }

        public async Task<CVEvaluationResponseDto> EvaluateCVAsync(int jobSeekerId, CVEvaluationRequestDto request)
        {
            try
            {
                // Verify CV belongs to job seeker
                var cv = await _context.CVs
                    .FirstOrDefaultAsync(c => c.Id == request.CVId && c.JobSeekerId == jobSeekerId);

                if (cv == null)
                    throw new UnauthorizedAccessException("CV not found or doesn't belong to job seeker");

                // Check if evaluation already exists for this CV and Job combination
                var existingEvaluation = await _context.Evaluations
                    .Include(e => e.JobApplication)
                    .FirstOrDefaultAsync(e => e.CvId == request.CVId &&
                                            e.JobApplication.JobId == request.JobId);

                if (existingEvaluation != null)
                {
                    return new CVEvaluationResponseDto
                    {
                        Id = existingEvaluation.Id,
                        CvId = existingEvaluation.CvId,
                        Score = existingEvaluation.Score,
                        Notes = existingEvaluation.Notes,
                        CandidateStrengths = existingEvaluation.CandidateStrengths,
                        CandidateWeaknesses = existingEvaluation.CandidateWeaknesses,
                        OverallFitRating = existingEvaluation.OverallFitRating,
                        JustificationForRating = existingEvaluation.JustificationForRating
                    };
                }

                // TODO: Here you would integrate with your friend's AI evaluation service
                // For now, returning a placeholder response
                var placeholderEvaluation = new CVEvaluationResponseDto
                {
                    Id = 0, // Will be set when AI service creates actual evaluation
                    CvId = request.CVId,
                    Score = null, // AI will provide this
                    Notes = "Evaluation request submitted to AI service",
                    CandidateStrengths = "To be determined by AI analysis",
                    CandidateWeaknesses = "To be determined by AI analysis",
                    OverallFitRating = null,
                    JustificationForRating = "AI evaluation in progress"
                };

                return placeholderEvaluation;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> MarkNotificationAsReadAsync(int userId, int notificationId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

                if (notification == null)
                    return false;

                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsCompanyFollowedAsync(int jobSeekerId, int companyId)
        {
            return await _context.Follows
                .AnyAsync(f => f.JobSeekerId == jobSeekerId && f.CompanyId == companyId);
        }

        public async Task<JobSeekerAccount?> GetJobSeekerByUserIdAsync(int userId)
        {
            return await _context.JobSeekerAccounts
                .Include(j => j.User)
                .FirstOrDefaultAsync(j => j.UserId == userId);
        }

        public async Task<CompanyAccount?> GetCompanyByIdAsync(int companyId)
        {
            return await _context.CompanyAccounts
                .FirstOrDefaultAsync(c => c.Id == companyId);
        }
    }
}

