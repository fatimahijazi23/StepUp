using aspteamAPI.context;
using aspteamAPI.DTOs;
using aspteamAPI.IRepository;
using aspteamAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace aspteamAPI.Repositories
{
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly AppDbContext _context;

        public JobApplicationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponseDto<JobApplicationResponseDto>> CreateApplicationAsync(CreateJobApplicationDto dto, int userId)
        {
            try
            {
                // Get the job seeker account for the current user
                var jobSeeker = await _context.JobSeekerAccounts
                    .Include(js => js.User)
                    .FirstOrDefaultAsync(js => js.UserId == userId);

                if (jobSeeker == null)
                {
                    return new ApiResponseDto<JobApplicationResponseDto>
                    {
                        Success = false,
                        Message = "Job seeker account not found"
                    };
                }

                // Check if job exists and is active
                var job = await _context.Jobs
                    .Include(j => j.Company)
                    .FirstOrDefaultAsync(j => j.Id == dto.JobId && j.IsActive);

                if (job == null)
                {
                    return new ApiResponseDto<JobApplicationResponseDto>
                    {
                        Success = false,
                        Message = "Job not found or not active"
                    };
                }

                // Check if CV belongs to the user
                var cv = await _context.CVs
                    .FirstOrDefaultAsync(c => c.Id == dto.CvId && c.JobSeekerId == jobSeeker.Id);

                if (cv == null)
                {
                    return new ApiResponseDto<JobApplicationResponseDto>
                    {
                        Success = false,
                        Message = "CV not found or does not belong to user"
                    };
                }

                // Check if user already applied for this job
                var existingApplication = await _context.JobApplications
                    .FirstOrDefaultAsync(ja => ja.ApplicantId == jobSeeker.Id && ja.JobId == dto.JobId);

                if (existingApplication != null)
                {
                    return new ApiResponseDto<JobApplicationResponseDto>
                    {
                        Success = false,
                        Message = "You have already applied for this job"
                    };
                }

                // Create new application
                var application = new JobApplication
                {
                    ApplicantId = jobSeeker.Id,
                    JobId = dto.JobId,
                    CvId = dto.CvId,
                    CreatedAt = DateTime.UtcNow,
                    Status = ApplicationStatusBadge.Applied
                };

                _context.JobApplications.Add(application);

                // CREATE NOTIFICATION FOR COMPANY OWNER
                var notification = new Notification
                {
                    UserId = job.Company.UserId,
                    Title = "New Job Application",
                    Message = $"{jobSeeker.User.Name} applied for {job.Title}",
                    Type = "application",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();

                // Load related data for response
                var createdApplication = await _context.JobApplications
                    .Include(ja => ja.Applicant)
                        .ThenInclude(a => a.User)
                    .Include(ja => ja.Job)
                        .ThenInclude(j => j.Company)
                    .Include(ja => ja.Cv)
                    .Include(ja => ja.Evaluation)
                    .FirstOrDefaultAsync(ja => ja.Id == application.Id);

                var responseDto = MapToResponseDto(createdApplication!);

                return new ApiResponseDto<JobApplicationResponseDto>
                {
                    Success = true,
                    Message = "Application submitted successfully",
                    Data = responseDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<JobApplicationResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the application"
                };
            }
        }

        public async Task<PaginatedResponseDto<JobApplicationResponseDto>> GetUserApplicationsAsync(int userId, int page, int pageSize)
        {
            try
            {
                var jobSeeker = await _context.JobSeekerAccounts
                    .FirstOrDefaultAsync(js => js.UserId == userId);

                if (jobSeeker == null)
                {
                    return new PaginatedResponseDto<JobApplicationResponseDto>
                    {
                        Success = false,
                        Message = "Job seeker account not found"
                    };
                }

                var query = _context.JobApplications
                    .Include(ja => ja.Applicant)
                        .ThenInclude(a => a.User)
                    .Include(ja => ja.Job)
                        .ThenInclude(j => j.Company)
                    .Include(ja => ja.Cv)
                    .Include(ja => ja.Evaluation)
                    .Where(ja => ja.ApplicantId == jobSeeker.Id)
                    .OrderByDescending(ja => ja.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var applications = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var responseData = applications.Select(MapToResponseDto).ToList();

                return new PaginatedResponseDto<JobApplicationResponseDto>
                {
                    Success = true,
                    Message = "Applications retrieved successfully",
                    Data = responseData,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                return new PaginatedResponseDto<JobApplicationResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving applications"
                };
            }
        }

        public async Task<ApiResponseDto<JobApplicationResponseDto>> GetApplicationByIdAsync(int applicationId, int userId)
        {
            try
            {
                var application = await _context.JobApplications
                    .Include(ja => ja.Applicant)
                        .ThenInclude(a => a.User)
                    .Include(ja => ja.Job)
                        .ThenInclude(j => j.Company)
                            .ThenInclude(c => c.User)
                    .Include(ja => ja.Cv)
                    .Include(ja => ja.Evaluation)
                    .FirstOrDefaultAsync(ja => ja.Id == applicationId);

                if (application == null)
                {
                    return new ApiResponseDto<JobApplicationResponseDto>
                    {
                        Success = false,
                        Message = "Application not found"
                    };
                }

                // Check if user has permission to view this application
                var isJobSeeker = application.Applicant.UserId == userId;
                var isCompanyOwner = application.Job.Company.UserId == userId;

                if (!isJobSeeker && !isCompanyOwner)
                {
                    return new ApiResponseDto<JobApplicationResponseDto>
                    {
                        Success = false,
                        Message = "You don't have permission to view this application"
                    };
                }

                var responseDto = MapToResponseDto(application);

                return new ApiResponseDto<JobApplicationResponseDto>
                {
                    Success = true,
                    Message = "Application retrieved successfully",
                    Data = responseDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<JobApplicationResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the application"
                };
            }
        }

        public async Task<ApiResponseDto<object>> DeleteApplicationAsync(int applicationId, int userId)
        {
            try
            {
                var application = await _context.JobApplications
                    .Include(ja => ja.Applicant)
                    .Include(ja => ja.Job)
                        .ThenInclude(j => j.Company)
                    .FirstOrDefaultAsync(ja => ja.Id == applicationId);

                if (application == null)
                {
                    return new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Application not found"
                    };
                }

                // Only allow job seekers to delete their own applications
                if (application.Applicant.UserId != userId)
                {
                    return new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "You don't have permission to delete this application"
                    };
                }

                // Only allow deletion if application is in Applied or UnderReview status
                if (application.Status == ApplicationStatusBadge.Hired || application.Status == ApplicationStatusBadge.Interview)
                {
                    return new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Cannot delete application in current status"
                    };
                }

                // CREATE NOTIFICATION FOR COMPANY OWNER ABOUT WITHDRAWAL
                var notification = new Notification
                {
                    UserId = application.Job.Company.UserId,
                    Title = "Application Withdrawn",
                    Message = $"A candidate withdrew their application for {application.Job.Title}",
                    Type = "application_withdrawn",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                _context.Notifications.Add(notification);

                _context.JobApplications.Remove(application);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Application deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the application"
                };
            }
        }

        public async Task<PaginatedResponseDto<JobApplicationResponseDto>> GetJobApplicationsAsync(int jobId, int userId, int page, int pageSize)
        {
            try
            {
                // Check if user owns the job (company check)
                var job = await _context.Jobs
                    .Include(j => j.Company)
                    .FirstOrDefaultAsync(j => j.Id == jobId);

                if (job == null)
                {
                    return new PaginatedResponseDto<JobApplicationResponseDto>
                    {
                        Success = false,
                        Message = "Job not found"
                    };
                }

                if (job.Company.UserId != userId)
                {
                    return new PaginatedResponseDto<JobApplicationResponseDto>
                    {
                        Success = false,
                        Message = "You don't have permission to view applications for this job"
                    };
                }

                var query = _context.JobApplications
                    .Include(ja => ja.Applicant)
                        .ThenInclude(a => a.User)
                    .Include(ja => ja.Job)
                        .ThenInclude(j => j.Company)
                    .Include(ja => ja.Cv)
                    .Include(ja => ja.Evaluation)
                    .Where(ja => ja.JobId == jobId)
                    .OrderByDescending(ja => ja.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var applications = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var responseData = applications.Select(MapToResponseDto).ToList();

                return new PaginatedResponseDto<JobApplicationResponseDto>
                {
                    Success = true,
                    Message = "Job applications retrieved successfully",
                    Data = responseData,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                return new PaginatedResponseDto<JobApplicationResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving job applications"
                };
            }
        }

        public async Task<ApiResponseDto<JobApplicationResponseDto>> UpdateApplicationStatusAsync(int applicationId, UpdateApplicationStatusDto dto, int userId)
        {
            try
            {
                var application = await _context.JobApplications
                    .Include(ja => ja.Applicant)
                        .ThenInclude(a => a.User)
                    .Include(ja => ja.Job)
                        .ThenInclude(j => j.Company)
                    .Include(ja => ja.Cv)
                    .Include(ja => ja.Evaluation)
                    .FirstOrDefaultAsync(ja => ja.Id == applicationId);

                if (application == null)
                {
                    return new ApiResponseDto<JobApplicationResponseDto>
                    {
                        Success = false,
                        Message = "Application not found"
                    };
                }

                // Only company owners can update application status
                if (application.Job.Company.UserId != userId)
                {
                    return new ApiResponseDto<JobApplicationResponseDto>
                    {
                        Success = false,
                        Message = "You don't have permission to update this application status"
                    };
                }

                var oldStatus = application.Status;
                application.Status = dto.Status;

                // Add evaluation note if provided
                if (!string.IsNullOrEmpty(dto.Notes))
                {
                    if (application.Evaluation == null)
                    {
                        var evaluation = new Evaluation
                        {
                            JobApplicationId = applicationId,
                            CvId = application.CvId,
                            Notes = dto.Notes
                        };
                        _context.Evaluations.Add(evaluation);
                    }
                    else
                    {
                        application.Evaluation.Notes = dto.Notes;
                    }
                }

                // CREATE NOTIFICATION FOR JOB SEEKER ABOUT STATUS CHANGE
                if (oldStatus != dto.Status) // Only notify if status actually changed
                {
                    string statusMessage = dto.Status switch
                    {
                        ApplicationStatusBadge.Applied => $"Your application for {application.Job.Title} has been received",
                        ApplicationStatusBadge.UnderReview => $"Your application for {application.Job.Title} is now under review",
                        ApplicationStatusBadge.Interview => $"Great news! You've been invited for an interview for {application.Job.Title}",
                        ApplicationStatusBadge.Hired => $"Congratulations! You've been hired for {application.Job.Title}",
                        ApplicationStatusBadge.Rejected => $"Thank you for your interest. Your application for {application.Job.Title} was not selected",
                        _ => $"Your application status for {application.Job.Title} has been updated"
                    };

                    string notificationTitle = dto.Status switch
                    {
                        ApplicationStatusBadge.Interview => "Interview Invitation",
                        ApplicationStatusBadge.Hired => "Job Offer",
                        ApplicationStatusBadge.Rejected => "Application Update",
                        _ => "Application Status Update"
                    };

                    var notification = new Notification
                    {
                        UserId = application.Applicant.UserId,
                        Title = notificationTitle,
                        Message = statusMessage,
                        Type = "application_status",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };
                    _context.Notifications.Add(notification);
                }

                await _context.SaveChangesAsync();

                // Reload with updated data
                application = await _context.JobApplications
                    .Include(ja => ja.Applicant)
                        .ThenInclude(a => a.User)
                    .Include(ja => ja.Job)
                        .ThenInclude(j => j.Company)
                    .Include(ja => ja.Cv)
                    .Include(ja => ja.Evaluation)
                    .FirstOrDefaultAsync(ja => ja.Id == applicationId);

                var responseDto = MapToResponseDto(application!);

                return new ApiResponseDto<JobApplicationResponseDto>
                {
                    Success = true,
                    Message = "Application status updated successfully",
                    Data = responseDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<JobApplicationResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while updating application status"
                };
            }
        }

        public async Task<PaginatedResponseDto<JobApplicationResponseDto>> GetCompanyApplicationsAsync(int userId, int page, int pageSize, ApplicationStatusBadge? status = null)
        {
            try
            {
                var company = await _context.CompanyAccounts
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (company == null)
                {
                    return new PaginatedResponseDto<JobApplicationResponseDto>
                    {
                        Success = false,
                        Message = "Company account not found"
                    };
                }

                var query = _context.JobApplications
                    .Include(ja => ja.Applicant)
                        .ThenInclude(a => a.User)
                    .Include(ja => ja.Job)
                        .ThenInclude(j => j.Company)
                    .Include(ja => ja.Cv)
                    .Include(ja => ja.Evaluation)
                    .Where(ja => ja.Job.Company.UserId == userId);

                if (status.HasValue)
                {
                    query = query.Where(ja => ja.Status == status.Value);
                }

                query = query.OrderByDescending(ja => ja.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var applications = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var responseData = applications.Select(MapToResponseDto).ToList();

                return new PaginatedResponseDto<JobApplicationResponseDto>
                {
                    Success = true,
                    Message = "Company applications retrieved successfully",
                    Data = responseData,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                return new PaginatedResponseDto<JobApplicationResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving company applications"
                };
            }
        }

        private static JobApplicationResponseDto MapToResponseDto(JobApplication application)
        {
            return new JobApplicationResponseDto
            {
                Id = application.Id,
                ApplicantId = application.ApplicantId,
                JobId = application.JobId,
                CvId = application.CvId,
                CreatedAt = application.CreatedAt,
                Status = application.Status,
                Applicant = application.Applicant != null ? new JobSeekerBasicInfoDto
                {
                    Id = application.Applicant.Id,
                    Bio = application.Applicant.Bio,
                    ProfilePictureUrl = application.Applicant.ProfilePictureUrl,
                    User = application.Applicant.User != null ? new UserBasicInfoDto
                    {
                        Id = application.Applicant.User.Id,
                        Name = application.Applicant.User.Name,
                        Email = application.Applicant.User.Email,
                        Role = application.Applicant.User.Role
                    } : null
                } : null,
                Job = application.Job != null ? new JobBasicInfoDto
                {
                    Id = application.Job.Id,
                    Description = application.Job.Description,
                    Requirements = application.Job.Requirements,
                    Location = application.Job.Location,
                    Industry = application.Job.Industry,
                    ExperienceLevel = application.Job.ExperienceLevel,
                    EmploymentType = application.Job.EmploymentType,
                    WorkArrangement = application.Job.WorkArrangement,
                    MaxSalaryRange = application.Job.MaxSalaryRange,
                    MinSalaryRange = application.Job.MinSalaryRange,
                    CreatedAt = application.Job.CreatedAt,
                    IsActive = application.Job.IsActive,
                    Company = application.Job.Company != null ? new CompanyBasicInfoDto
                    {
                        Id = application.Job.Company.Id,
                        CompanyName = application.Job.Company.CompanyName,
                        About = application.Job.Company.About,
                        CompanySize = application.Job.Company.CompanySize,
                        Industry = application.Job.Company.Industry,
                        ProfilePictureUrl = application.Job.Company.ProfilePictureUrl
                    } : null
                } : null,
                Cv = application.Cv != null ? new CvBasicInfoDto
                {
                    Id = application.Cv.Id,
                    FileUrl = application.Cv.FileUrl,
                    UploadedAt = application.Cv.UploadedAt
                } : null,
                Evaluation = application.Evaluation != null ? new EvaluationBasicInfoDto
                {
                    Id = application.Evaluation.Id,
                    Score = application.Evaluation.Score,
                    Notes = application.Evaluation.Notes,
                    OverallFitRating = application.Evaluation.OverallFitRating
                } : null
            };
        }
    }
}