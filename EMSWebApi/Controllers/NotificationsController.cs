using EMSWebApi.Application.DTOs.Notifications;
using EMSWebApi.Application.Interfaces.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMSWebApi.Application.Mappings;
using EMSWebApi.Application.Helpers;

namespace EMSWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(IUnitOfWork unitOfWork, ILogger<NotificationsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetMyNotifications(
            [FromQuery] bool unreadOnly = false, 
            [FromQuery] int count = 5)
        {
            var userId = UserHelper.GetCurrentUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var notifications = await _unitOfWork.Notifications.GetUserNotificationsAsync(userId, unreadOnly, count);
            return Ok(notifications.ToDtoList());
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var userId = UserHelper.GetCurrentUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var count = await _unitOfWork.Notifications.GetUnreadNotificationCountAsync(userId);
            return Ok(count);
        }

        [HttpPost("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = UserHelper.GetCurrentUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _unitOfWork.Notifications.MarkAsReadAsync(id, userId);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Notification {NotificationId} marked as read for user {UserId}", id, userId);
            return NoContent();
        }

        [HttpPost("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = UserHelper.GetCurrentUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("All unread notifications marked as read for user {UserId}", userId);
            return NoContent();
        }
    }
}
