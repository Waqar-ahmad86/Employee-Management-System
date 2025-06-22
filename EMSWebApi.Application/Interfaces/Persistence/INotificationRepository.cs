using EMSWebApi.Domain.Entities;

namespace EMSWebApi.Application.Interfaces.Persistence
{
    public interface INotificationRepository : IBaseGuidRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false, int count = 5);
        Task<int> GetUnreadNotificationCountAsync(string userId);
        Task MarkAsReadAsync(Guid notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
    }
}
