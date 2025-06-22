using EMSWebApi.Application.Interfaces.Persistence;
using EMSWebApi.Domain.Entities;
using EMSWebApi.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace EMSWebApi.Infrastructure.Persistence.Repositories
{
    public class NotificationRepository : BaseGuidRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(
            string userId,
            bool unreadOnly = false,
            int count = 5)
        {
            var query = _dbSet.Where(n => n.UserId == userId);
            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }
            return await query.OrderByDescending(n => n.CreatedAt).Take(count).ToListAsync();
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            return await _dbSet.CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(Guid notificationId, string userId)
        {
            var notification = await _dbSet.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = await _dbSet.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                base.Delete(entity);
            }
        }
    }
}
