using EMSMvc.Core.Application.DTOs;
using EMSMvc.Core.Application.Interfaces;

namespace EMSMvc.Core.Application.Services
{
    public class NotificationService
    {
        private readonly IApiService _apiService;

        public NotificationService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<Notification>> GetMyNotificationsAsync(bool unreadOnly = false, int count = 5)
        {
            var endpoint = $"Notifications?unreadOnly={unreadOnly}&count={count}";
            return await _apiService.SendRequestAsync<List<Notification>>(HttpMethod.Get, endpoint) ?? new List<Notification>();
        }

        public async Task<int> GetUnreadNotificationCountAsync()
        {
            var count = await _apiService.SendRequestAsync<int>(HttpMethod.Get, "Notifications/unread-count");
            return count;
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId)
        {
            return await _apiService.SendRequestAsync(HttpMethod.Post, $"Notifications/{notificationId}/mark-as-read");
        }

        public async Task<bool> MarkAllAsReadAsync()
        {
            return await _apiService.SendRequestAsync(HttpMethod.Post, "Notifications/mark-all-as-read");
        }
    }
}
