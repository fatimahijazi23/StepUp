using aspteamAPI.context;
using aspteamAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace aspteamAPI.Repositories
{
    public class JobSeekerProfileRepository : IJobSeekerProfileRepo
    {
        private readonly AppDbContext _context;

        public JobSeekerProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UpdateJobSeekerProfileDTO?> GetJobSeekerProfile(int id)
        {
            var jobSeeker = await _context.JobSeekerAccounts
                .AsNoTracking()
                .Include(u => u.User)
                .SingleOrDefaultAsync(u => u.Id == id);
            int FollowCount = _context.Follows.Count(f => f.JobSeekerId == id);

            if (jobSeeker == null || jobSeeker.User == null) return null;

            return new UpdateJobSeekerProfileDTO
            {
                Name = jobSeeker.User.Name,
                Email = jobSeeker.User.Email,
                Bio = jobSeeker.Bio,
                ProfilePictureUrl = jobSeeker.ProfilePictureUrl,
                FollowCount = FollowCount
            };
        }


        public async Task<UpdateJobSeekerProfileDTO?> UpdateJobSeekerProfile(int id, UpdateJobSeekerProfileDTO dto)
        {
            var jobSeekerAccount = await _context.JobSeekerAccounts
                .Include(js => js.User)
                .FirstOrDefaultAsync(js => js.Id == id);

            if (jobSeekerAccount == null)
                return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(dto.ProfilePictureUrl))
                jobSeekerAccount.ProfilePictureUrl = dto.ProfilePictureUrl.Trim();

            if (jobSeekerAccount.User != null)
            {
                if (!string.IsNullOrEmpty(dto.Name))
                    jobSeekerAccount.User.Name = dto.Name.Trim();

                if (!string.IsNullOrEmpty(dto.Email))
                    jobSeekerAccount.User.Email = dto.Email.Trim();
            }

            if (!string.IsNullOrEmpty(dto.Bio))
                jobSeekerAccount.Bio = dto.Bio.Trim();

            await _context.SaveChangesAsync();

            return new UpdateJobSeekerProfileDTO
            {
                ProfilePictureUrl = jobSeekerAccount.ProfilePictureUrl,
                Name = jobSeekerAccount.User?.Name ?? string.Empty,
                Email = jobSeekerAccount.User?.Email ?? string.Empty,
                Bio = jobSeekerAccount.Bio
            };
        }

        public async Task<JobSeekerAccount?> DeleteJobSeekerProfile(int id)
        {
            var jobSeekerAccount = await _context.JobSeekerAccounts
                .Include(js => js.User) // load related User
                .SingleOrDefaultAsync(a => a.Id == id);

            if (jobSeekerAccount == null)
                return null;

            // Remove the User (which cascades to JobSeeker)
            _context.Users.Remove(jobSeekerAccount.User);

            await _context.SaveChangesAsync();

            return jobSeekerAccount;
        }


    }
}
