
using aspteamAPI.context;
using aspteamAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace aspteamAPI.Repositories
{
    public class CvRepository : ICvRepository
    {
        private  AppDbContext _context ;

        public CvRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CV>> GetCvsByUserIdAsync(int userId)
        {
            return await _context.CVs.Where(cv=>cv.JobSeekerId == userId)
                .ToListAsync();


        }
        public async Task<CV?> GetCvByIdAsync(int cvId)
        {
            return await _context.CVs.SingleOrDefaultAsync(cv => cv.Id == cvId);

        }
        public async Task<CV?> AddCvAsync(CV cv)
        {
            if (cv == null) return null;

            // Check if the JobSeeker exists
            var jobSeeker = await _context.JobSeekerAccounts
                .SingleOrDefaultAsync(js => js.Id == cv.JobSeekerId);

            if (jobSeeker == null) return null;

            await _context.CVs.AddAsync(cv);
            await _context.SaveChangesAsync();

            return cv;
        }


        public async Task<bool> DeleteCvAsync(int cvId)
        {
            var cv = await _context.CVs.SingleOrDefaultAsync(cv => cv.Id == cvId);

            if(cv == null) return false;

            _context.CVs.Remove(cv);

            await _context.SaveChangesAsync() ;
            return true;

        }

      

       
    }
}
