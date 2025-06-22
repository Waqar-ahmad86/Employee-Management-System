using EMS.Common.Enums;
using EMSWebApi.Application.DTOs.Leave;
using EMSWebApi.Application.DTOs.Notifications;
using EMSWebApi.Application.Helpers;
using EMSWebApi.Application.Interfaces.Infrastructure;
using EMSWebApi.Application.Interfaces.Persistence;
using EMSWebApi.Application.Mappings;
using EMSWebApi.Domain.Entities;
using EMSWebApi.Domain.Identity;
using EMSWebApi.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMSWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveApplicationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LeaveApplicationsController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly LeaveAttendanceHelper _leaveAttendanceHelper;

        public LeaveApplicationsController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ILogger<LeaveApplicationsController> logger,
            ApplicationDbContext context,
            INotificationService notificationService,
            LeaveAttendanceHelper leaveAttendanceHelper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
            _context = context;
            _notificationService = notificationService;
            _leaveAttendanceHelper = leaveAttendanceHelper;
        }


        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<LeaveApplicationDto>> ApplyForLeave([FromBody] CreateLeaveApplicationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var applicantId = UserHelper.GetCurrentUserId(User);
            if (string.IsNullOrEmpty(applicantId)) return Unauthorized();

            var applicant = await _userManager.FindByIdAsync(applicantId);
            if (applicant == null) return NotFound(new { Message = "Applicant not found." });

            var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(dto.LeaveTypeId);
            if (leaveType == null || !leaveType.IsActive)
            {
                return BadRequest(new { Message = "Invalid or inactive leave type specified." });
            }

            if (dto.StartDate.Date < DateTime.UtcNow.Date)
            {
                return BadRequest(new { Message = "Start date cannot be in the past." });
            }
            if (dto.EndDate.Date < dto.StartDate.Date)
            {
                return BadRequest(new { Message = "End date cannot be before start date." });
            }

            var overlappingLeaves = await _unitOfWork.LeaveApplications.GetForDateRangeAndUserAsync(
                                    applicantId,
                                    dto.StartDate,
                                    dto.EndDate);

            if (overlappingLeaves.Any())
            {
                return Conflict(new { Message = "You already have a pending or approved leave application that overlaps with these dates." });
            }


            var leaveApplication = new LeaveApplication
            {
                ApplicantId = applicantId,
                LeaveTypeId = dto.LeaveTypeId,
                StartDate = dto.StartDate.Date,
                EndDate = dto.EndDate.Date,
                Reason = dto.Reason,
                Status = LeaveApplicationStatus.Pending,
                AppliedDate = DateTime.UtcNow
            };

            await _unitOfWork.LeaveApplications.AddAsync(leaveApplication);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Leave application ID {LeaveAppId} created by user {UserId}", leaveApplication.Id, applicantId);

            try
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Any())
                {
                    string applicantName = applicant.FullName ?? applicant.UserName ?? "A user";
                    string adminMessage = $"{applicantName} has applied for {leaveType.Name} from {leaveApplication.StartDate:dd MMM} to {leaveApplication.EndDate:dd MMM}.";
                    string adminLink = $"/Leave/Manage";

                    foreach (var adminUser in admins)
                    {
                        await _notificationService.CreateAndSendNotificationAsync(new CreateNotificationDto
                        {
                            UserId = adminUser.Id,
                            Message = adminMessage,
                            Link = adminLink
                        });
                        _logger.LogInformation("Sent new leave application notification to Admin {AdminUserId}", adminUser.Id);
                    }
                }
                else
                {
                    _logger.LogWarning("No admin users found to notify about new leave application {LeaveAppId}", leaveApplication.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying admins about new leave application {LeaveAppId}", leaveApplication.Id);
            }

            var createdAppWithIncludes = await _context.LeaveApplications
                                        .Include(la => la.Applicant)
                                        .Include(la => la.LeaveType)
                                        .FirstOrDefaultAsync(la => la.Id == leaveApplication.Id);

            if (createdAppWithIncludes == null)
            {
                _logger.LogError("Failed to retrieve created leave application {LeaveAppId} with includes.", leaveApplication.Id);
                return StatusCode(500, "Error retrieving created application details.");
            }

            return CreatedAtAction(nameof(GetLeaveApplicationById), new { id = createdAppWithIncludes.Id }, createdAppWithIncludes.ToDto());
        }

        [HttpGet("my-applications")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<IEnumerable<LeaveApplicationDto>>> GetMyLeaveApplications(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var applicantId = UserHelper.GetCurrentUserId(User);
            if (string.IsNullOrEmpty(applicantId)) return Unauthorized();

            var applications = await _unitOfWork.LeaveApplications.GetLeaveApplicationsForUserAsync(applicantId, startDate, endDate);

            var dtos = applications.Select(la => la.ToDto()).ToList();
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveApplicationDto>> GetLeaveApplicationById(int id)
        {
            var leaveApplication = await _context.LeaveApplications
                                        .Include(la => la.Applicant)
                                        .Include(la => la.LeaveType)
                                        .Include(la => la.ApprovedByUser)
                                        .FirstOrDefaultAsync(la => la.Id == id);

            if (leaveApplication == null) return NotFound(new { Message = "Leave application not found." });

            var currentUserId = UserHelper.GetCurrentUserId(User);
            var isCurrentUserAdmin = User.IsInRole("Admin");

            if (!isCurrentUserAdmin && leaveApplication.ApplicantId != currentUserId)
            {
                return Forbid();
            }

            return Ok(leaveApplication.ToDto());
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<LeaveApplicationDto>>> GetPendingLeaveApplications()
        {
            var applications = await _unitOfWork.LeaveApplications.GetAllPendingAsync();
            var dtos = applications.Select(la => la.ToDto()).ToList();
            return Ok(dtos);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<LeaveApplicationDto>>> GetAllLeaveApplications(
            [FromQuery] string? applicantName = null,
            [FromQuery] int? leaveTypeId = null,
            [FromQuery] LeaveApplicationStatus? status = null,
            [FromQuery] DateTime? SDate = null,
            [FromQuery] DateTime? EDate = null)
        {
            IQueryable<LeaveApplication> query = _context.LeaveApplications
                .Include(la => la.Applicant)
                .Include(la => la.LeaveType)
                .Include(la => la.ApprovedByUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(applicantName))
            {
                string searchTerm = applicantName.ToLower();
                query = query.Where(la => (la.Applicant.FullName != null && la.Applicant.FullName.ToLower().Contains(searchTerm)) ||
                                           (la.Applicant.UserName != null && la.Applicant.UserName.ToLower().Contains(searchTerm)));
            }
            if (leaveTypeId.HasValue)
            {
                query = query.Where(la => la.LeaveTypeId == leaveTypeId.Value);
            }
            if (status.HasValue)
            {
                query = query.Where(la => la.Status == status.Value);
            }
            if (SDate.HasValue)
            {
                query = query.Where(la => la.EndDate >= SDate.Value.Date);
            }
            if (EDate.HasValue)
            {
                query = query.Where(la => la.StartDate <= EDate.Value.Date);
            }

            var applications = await query.OrderByDescending(la => la.AppliedDate).ToListAsync();
            var dtos = applications.Select(la => la.ToDto()).ToList();

            return Ok(dtos);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLeaveApplicationStatus(int id, [FromBody] UpdateLeaveApplicationStatusDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var leaveApplication = await _context.LeaveApplications
                                .Include(la => la.LeaveType)
                                .Include(la => la.Applicant)
                                .FirstOrDefaultAsync(la => la.Id == id);

            if (leaveApplication == null) return NotFound(new { Message = "Leave application not found." });

            if (leaveApplication.Status != LeaveApplicationStatus.Pending)
            {
                return BadRequest(new { Message = $"Leave application is already '{leaveApplication.Status}'. Cannot change status." });
            }

            if (dto.NewStatus != LeaveApplicationStatus.Approved && dto.NewStatus != LeaveApplicationStatus.Rejected)
            {
                return BadRequest(new { Message = "Invalid new status. Must be 'Approved' or 'Rejected'." });
            }

            var adminUserId = UserHelper.GetCurrentUserId(User);

            leaveApplication.Status = dto.NewStatus;
            leaveApplication.AdminRemarks = dto.AdminRemarks;
            leaveApplication.ApprovedByUserId = adminUserId;
            leaveApplication.ActionDate = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Leave application ID {LeaveAppId} status updated to {Status} by Admin {AdminId}", id, dto.NewStatus, adminUserId);

            string notificationMessage;
            if (leaveApplication.Status == LeaveApplicationStatus.Approved)
            {
                notificationMessage = $"Your leave application from {leaveApplication.StartDate:dd MMM yyyy} to {leaveApplication.EndDate:dd MMM yyyy} has been approved.";
                await _leaveAttendanceHelper.HandleAttendanceUpdateForApprovedLeave(leaveApplication);
            }
            else
            {
                notificationMessage = $"Your leave application from {leaveApplication.StartDate:dd MMM yyyy} to {leaveApplication.EndDate:dd MMM yyyy} has been rejected. Remarks: {leaveApplication.AdminRemarks ?? "N/A"}";
            }

            await _notificationService.CreateAndSendNotificationAsync(new CreateNotificationDto
            {
                UserId = leaveApplication.ApplicantId,
                Message = notificationMessage,
                Link = $"/Leave/MyApplications"
            });

            return NoContent();
        }
    }
}
