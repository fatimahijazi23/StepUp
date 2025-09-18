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
                .Include( u => u.User)
                .SingleOrDefaultAsync(u => u.Id == id);

            if (jobSeeker == null) return null;

            return new UpdateJobSeekerProfileDTO
            {
                Id = jobSeeker.Id,
                Name = jobSeeker.User.Name,
                Email = jobSeeker.User.Email,
                Bio = jobSeeker.Bio,
                ProfilePictureUrl = jobSeeker.ProfilePictureUrl

            };
        }

        public async Task<UpdateJobSeekerProfileDTO?> UpdateJobSeekerProfile(UpdateJobSeekerProfileDTO dto)
        {
            // Get job seeker account from db 
            var jobSeekerAccount = await _context.JobSeekerAccounts
                .Include(js => js.User)
                .FirstOrDefaultAsync(js => js.Id == dto.Id);

            if (jobSeekerAccount == null)
            {
                return null; // Not found
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(dto.ProfilePictureUrl))
                jobSeekerAccount.ProfilePictureUrl = dto.ProfilePictureUrl;

            if (!string.IsNullOrEmpty(dto.Name))
                jobSeekerAccount.User.Name = dto.Name;

            if (!string.IsNullOrEmpty(dto.Email))
                jobSeekerAccount.User.Email = dto.Email;

            if (!string.IsNullOrEmpty(dto.Bio))
                jobSeekerAccount.Bio = dto.Bio;

            // Save changes
            await _context.SaveChangesAsync();

           
            return new UpdateJobSeekerProfileDTO
            {
                Id = jobSeekerAccount.Id,
                ProfilePictureUrl = jobSeekerAccount.ProfilePictureUrl,
                Name = jobSeekerAccount.User.Name,
                Email = jobSeekerAccount.User.Email,
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
