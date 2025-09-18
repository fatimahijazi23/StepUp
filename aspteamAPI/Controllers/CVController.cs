using aspteamAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using aspteamAPI.Repositories;

namespace aspteamAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CVController : Controller
    {
        private ICvRepository _cvRepository;

        public CVController(ICvRepository cvRepository)
        {
            _cvRepository = cvRepository;
        }

        [HttpGet("userId/{userId}")]
        public async Task<IActionResult> GetCvsByUserId(int userId)
        {
            var cvs = await _cvRepository.GetCvsByUserIdAsync(userId);

            if (!cvs.Any())
                return NotFound($"No CVs found for user with ID {userId}");

            // Map entity → DTO
            var dtoList = cvs.Select(cv => new CvDTO
            {
                JobSeekerId = cv.JobSeekerId,
                FileUrl = "/files/" + cv.FileUrl,
                UploadedAt = cv.UploadedAt,
            });

            return Ok(dtoList);
        }

        [HttpGet("CvId{cvId}")]
        public async Task<IActionResult> GetCvsById(int cvId)
        {
            var cv = await _cvRepository.GetCvByIdAsync(cvId);

            if (cv == null) return NotFound($"CV with ID = {cvId} Not Found");


            return Ok(new CvDTO
            {
                JobSeekerId = cv.JobSeekerId,
                FileUrl = cv.FileUrl,
                UploadedAt = cv.UploadedAt
            });

        }

        [HttpPost]
        public async Task<IActionResult> AddCv([FromBody] CvDTO cvDto)
        {
            if (cvDto == null) return BadRequest("Invalid CV data");

            // Map DTO → Entity
            var cv = new CV
            {
                FileUrl = cvDto.FileUrl,
                UploadedAt = DateTime.UtcNow,
                JobSeekerId = cvDto.JobSeekerId
            };

            var createdCv = await _cvRepository.AddCvAsync(cv);

            if (createdCv == null) return NotFound("User Not Found");

            // Map Entity → DTO
            var resultDto = new CvDTO
            {

                FileUrl = "/files/" + createdCv.FileUrl,
                JobSeekerId = createdCv.JobSeekerId
            };

            return Ok(resultDto);
        }


        [HttpDelete("{cvId}")]
        public async Task<IActionResult> DeleteCv(int cvId)
        {
            bool isSuccessful = await _cvRepository.DeleteCvAsync(cvId);

            if (!isSuccessful)
                return NotFound($"CV with ID {cvId} not found");

            return NoContent(); // 204 is standard for successful deletion
        }



    }
}
