using EMSMvc.Core.Application.Services;
using EMSMvc.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace EMSMvc.ViewComponents
{
    public class NotificationBellViewComponent : ViewComponent
    {
        private readonly NotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationBellViewComponent(NotificationService notificationService, IHttpContextAccessor httpContextAccessor)
        {
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!SessionHelper.IsUserLoggedIn(_httpContextAccessor.HttpContext))
            {
                return Content(string.Empty);
            }

            var unreadCount = await _notificationService.GetUnreadNotificationCountAsync();
            var recentNotifications = await _notificationService.GetMyNotificationsAsync(unreadOnly: false, count: 5);

            ViewBag.UnreadNotificationCount = unreadCount;
            return View(recentNotifications);
        }
    }
}
