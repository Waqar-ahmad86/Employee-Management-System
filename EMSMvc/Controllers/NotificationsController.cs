using EMSMvc.Core.Application.DTOs;
using EMSMvc.Core.Application.Services;
using EMSMvc.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace EMSMvc.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly NotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationsController(NotificationService notificationService, IHttpContextAccessor httpContextAccessor)
        {
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!SessionHelper.IsUserLoggedIn(_httpContextAccessor.HttpContext))
            {
                return RedirectToAction("Login", "Auth");
            }

            var allNotifications = await _notificationService.GetMyNotificationsAsync(unreadOnly: false, count: 50);
            ViewBag.UnreadCount = await _notificationService.GetUnreadNotificationCountAsync();
            return View(allNotifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            if (!SessionHelper.IsUserLoggedIn(_httpContextAccessor.HttpContext))
            {
                return Json(new { count = 0 });
            }
            var count = await _notificationService.GetUnreadNotificationCountAsync();
            return Json(new { count });
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentNotifications()
        {
            if (!SessionHelper.IsUserLoggedIn(_httpContextAccessor.HttpContext))
            {
                return Json(new List<Notification>());
            }

            var notifications = await _notificationService.GetMyNotificationsAsync(unreadOnly: false, count: 5);
            return Json(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            if (!SessionHelper.IsUserLoggedIn(_httpContextAccessor.HttpContext))
            {
                return Unauthorized();
            }
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid notification ID.");
            }

            var success = await _notificationService.MarkAsReadAsync(id);
            if (success)
            {
                return Ok(new { message = "Notification marked as read." });
            }
            return StatusCode(500, new { message = "Failed to mark notification as read." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            if (!SessionHelper.IsUserLoggedIn(_httpContextAccessor.HttpContext))
            {
                return Unauthorized();
            }

            var success = await _notificationService.MarkAllAsReadAsync();
            if (success)
            {
                return Ok(new { message = "All notifications marked as read." });
            }
            return StatusCode(500, new { message = "Failed to mark all notifications as read." });
        }

        
    }
}
