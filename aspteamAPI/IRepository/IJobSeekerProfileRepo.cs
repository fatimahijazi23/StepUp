using aspteamAPI.DTOs;

namespace aspteamAPI
{
    public interface IJobSeekerProfileRepo
    {
        //          GET /api/user/profile ✅
        //          Patch /api/user/profile ✅
        //          DELETE /api/user/account✅
        //          POST /api/user/upload-avatar✅
        //          POST /api/user/upload-cv
        //          GET /api/user/cv

        public Task<UpdateJobSeekerProfileDTO> GetJobSeekerProfile(int id);

        public Task<UpdateJobSeekerProfileDTO> UpdateJobSeekerProfile(UpdateJobSeekerProfileDTO dto);
        public Task<JobSeekerAccount> DeleteJobSeekerProfile(int id);

    }
}
