using EMSWebApi.Domain.Entities;
using EMSWebApi.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EMSWebApi.Application.DTOs.Attendance;
using EMSWebApi.Application.Interfaces.Persistence;
using EMSWebApi.Application.Mappings;
using EMSWebApi.Application.Helpers;
using EMS.Common.Enums;

namespace EMSWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ILogger<AttendanceController> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("check-in")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInDto? dto)
        {
            var userId = UserHelper.GetCurrentUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var today = DateTime.UtcNow.Date;
            var existingRecord = await _unitOfWork.Attendances.GetByUserIdAndDateAsync(userId, today);

            if (existingRecord?.CheckInTime != null)
            {
                return BadRequest(new { Message = "You have already checked in today." });
            }

            bool isNew = existingRecord == null;
            if (isNew)
            {
                existingRecord = new AttendanceRecord
                {
                    UserId = userId,
                    Date = today,
                    Status = AttendanceStatus.Present
                };
            }

            existingRecord.CheckInTime = DateTime.UtcNow.TimeOfDay;
            existingRecord.Remarks = dto?.Remarks ?? existingRecord.Remarks;

            if (isNew)
            {
                await _unitOfWork.Attendances.AddAsync(existingRecord);
            }

            try
            {
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("User {UserId} checked in at {Time}", userId, existingRecord.CheckInTime);

                if (existingRecord.User == null)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    return Ok(existingRecord.ToDto(user));
                }

                return Ok(existingRecord.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during check-in for user {UserId}", userId);
                return StatusCode(500, new { Message = "An error occurred during check-in." });
            }
        }

        [HttpPost("check-out")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> CheckOut([FromBody] CheckOutDto? dto)
        {
            var userId = UserHelper.GetCurrentUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var today = DateTime.UtcNow.Date;
            var record = await _unitOfWork.Attendances.GetByUserIdAndDateAsync(userId, today);

            if (record == null || !record.CheckInTime.HasValue)
            {
                return BadRequest(new { Message = "You have not checked in today or no check-in record found." });
            }

            if (record.CheckOutTime.HasValue)
            {
                return BadRequest(new { Message = "You have already checked out today." });
            }

            record.CheckOutTime = DateTime.UtcNow.TimeOfDay;
            record.Remarks = dto?.Remarks ?? record.Remarks;

            try
            {
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("User {UserId} checked out at {Time}", userId, record.CheckOutTime);
                return Ok(record.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during check-out for user {UserId}", userId);
                return StatusCode(500, new { Message = "An error occurred during check-out." });
            }
        }

        [HttpGet("my-today")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<AttendanceRecordDto>> GetMyTodaysAttendance()
        {
            var userId = UserHelper.GetCurrentUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var record = await _unitOfWork.Attendances.GetByUserIdAndDateAsync(userId, DateTime.UtcNow.Date);

            return Ok(record?.ToDto());
        }


        [HttpGet("my-history")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<IEnumerable<AttendanceRecordDto>>> GetMyAttendanceHistory(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var userId = UserHelper.GetCurrentUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (startDate == default || endDate == default || startDate > endDate)
                return BadRequest(new { Message = "Invalid date range." });

            if ((endDate - startDate).TotalDays > 90)
                return BadRequest(new { Message = "Date range cannot exceed 90 days." });

            var records = await _unitOfWork.Attendances.GetAttendanceForUserAsync(userId, startDate, endDate);
            return Ok(records.ToDtoList());
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<AttendanceRecordDto>>> GetAllAttendance(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? userName = null,
            [FromQuery] string? roleName = null)
        {
            if (startDate == default || endDate == default || startDate > endDate)
                return BadRequest(new { Message = "Invalid date range." });

            if ((endDate - startDate).TotalDays > 90)
                return BadRequest(new { Message = "Date range cannot exceed 90 days." });

            var query = _unitOfWork.Attendances.GetAllQueryable()
                .Include(a => a.User)
                .Where(a => a.Date >= startDate && a.Date <= endDate);

            if (!string.IsNullOrEmpty(userName))
            {
                var search = userName.ToLower();
                query = query.Where(a =>
                    (a.User.FullName != null && a.User.FullName.ToLower().Contains(search)) ||
                    (a.User.UserName != null && a.User.UserName.ToLower().Contains(search)));
            }

            if (!string.IsNullOrEmpty(roleName))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
                var userIds = usersInRole.Select(u => u.Id).ToList();
                if (!userIds.Any()) return Ok(new List<AttendanceRecordDto>());
                query = query.Where(a => userIds.Contains(a.UserId));
            }

            var records = await query.OrderBy(a => a.User.FullName).ThenBy(a => a.Date).ToListAsync();
            return Ok(records.ToDtoList());
        }

        [HttpPost("monthly-report")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<MonthlyAttendanceReportItemDto>>> GetMonthlyAttendanceReport(
            [FromBody] MonthlyAttendanceReportRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var baseRecords = await _unitOfWork.Attendances.GetAttendanceForMonthAsync(
                request.Year, request.Month, request.UserName);

            IEnumerable<AttendanceRecord> filtered = baseRecords;

            if (!string.IsNullOrEmpty(request.RoleName))
            {
                var users = await _userManager.GetUsersInRoleAsync(request.RoleName);
                var userIds = users.Select(u => u.Id).ToList();
                if (!userIds.Any())
                {
                    return Ok(new List<MonthlyAttendanceReportItemDto>());
                }
                filtered = baseRecords.Where(r => userIds.Contains(r.UserId));
            }

            var report = filtered
                .GroupBy(r => (r.UserId, r.User.FullName, r.User.UserName))
                .Select(g => g.ToMonthlyReportItem(request.Year, request.Month))
                .OrderBy(d => d.UserId)
                .ToList();

            _logger.LogInformation("Generated monthly report with {Count} items", report.Count);
            return Ok(report);
        }
    }
}
