using EMSWebApi.Application.DTOs.Notices;
using EMSWebApi.Application.Interfaces.Persistence;
using EMSWebApi.Domain.Identity;
using EMSWebApi.Application.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EMSWebApi.Application.Helpers;
using EMS.Common.Enums;
using EMSWebApi.Application.DTOs.Notifications;
using EMSWebApi.Application.Interfaces.Infrastructure;

namespace EMSWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoticesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<NoticesController> _logger;
        private readonly INotificationService _notificationService;

        public NoticesController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, ILogger<NoticesController> logger, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
            _notificationService = notificationService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<NoticeDto>>> GetActiveNoticesForCurrentUser()
        {
            var userId = UserHelper.GetCurrentUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var userRoles = await _userManager.GetRolesAsync(user);

            var relevantAudiences = new List<NoticeAudience> { NoticeAudience.All };
            if (userRoles.Contains("Admin"))
                relevantAudiences.Add(NoticeAudience.AdminOnly);
            if (userRoles.Contains("User"))
                relevantAudiences.Add(NoticeAudience.UserOnly);

            var notices = await _unitOfWork.Notices.GetNoticesForAudienceAsync(relevantAudiences.Distinct().ToList(), activeOnly: true);

            return Ok(notices.ToDtoList());
        }

        [HttpGet("all-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<NoticeDto>>> GetAllNoticesAdmin([FromQuery] bool activeOnly = false)
        {
            var notices = await _unitOfWork.Notices.GetAllAsync(activeOnly);
            return Ok(notices.ToDtoList());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<NoticeDto>> GetNoticeById(Guid id)
        {
            var notice = await _unitOfWork.Notices.GetByIdAsync(id);
            if (notice == null) return NotFound();

            return Ok(notice.ToDto());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<NoticeDto>> CreateNotice([FromBody] CreateNoticeDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var currentUserId = UserHelper.GetCurrentUserId(User);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var notice = dto.ToEntity(currentUserId);

            await _unitOfWork.Notices.AddAsync(notice);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Notice '{Title}' created by user {UserId}", notice.Title, currentUserId);

            try
            {
                var usersToNotify = new List<ApplicationUser>();
                switch (notice.Audience)
                {
                    case NoticeAudience.All:
                        usersToNotify = _userManager.Users.Where(u => u.IsActive).ToList();
                        break;
                    case NoticeAudience.UserOnly:
                        var usersInUserRole = await _userManager.GetUsersInRoleAsync("User");
                        usersToNotify = usersInUserRole.Where(u => u.IsActive).ToList();
                        break;
                    case NoticeAudience.AdminOnly:
                        var usersInAdminRole = await _userManager.GetUsersInRoleAsync("Admin");
                        usersToNotify = usersInAdminRole.Where(u => u.IsActive).ToList();
                        break;
                }

                if (usersToNotify.Any())
                {
                    string notificationMessage = $"New Notice: {notice.Title.Substring(0, Math.Min(notice.Title.Length, 50))}{(notice.Title.Length > 50 ? "..." : "")}";
                    string notificationLink = "/Notices/Index";

                    foreach (var user in usersToNotify)
                    {
                        await _notificationService.CreateAndSendNotificationAsync(new CreateNotificationDto
                        {
                            UserId = user.Id,
                            Message = notificationMessage,
                            Link = notificationLink
                        });
                    }
                    _logger.LogInformation("Sent new notice notifications to {Count} users for notice ID {NoticeId}", usersToNotify.Count, notice.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notifications for new notice ID {NoticeId}", notice.Id);
            }

            return CreatedAtAction(nameof(GetNoticeById), new { id = notice.Id }, notice.ToDto());
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateNotice(Guid id, [FromBody] UpdateNoticeDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var notice = await _unitOfWork.Notices.GetByIdAsync(id);
            if (notice == null) return NotFound();

            notice.UpdateEntity(dto);

            _unitOfWork.Notices.Update(notice);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Notice ID {NoticeId} updated.", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteNotice(Guid id)
        {
            var notice = await _unitOfWork.Notices.GetByIdAsync(id);
            if (notice == null) return NotFound();

            _unitOfWork.Notices.Delete(notice);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Notice ID {NoticeId} deleted.", id);
            return NoContent();
        }
    }
}

