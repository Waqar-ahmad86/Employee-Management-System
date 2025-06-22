using EMSWebApi.Application.DTOs;
using EMSWebApi.Application.DTOs.Notifications;
using EMSWebApi.Application.Interfaces.Infrastructure;
using EMSWebApi.Application.Interfaces.Persistence;
using EMSWebApi.Domain.Entities;
using EMSWebApi.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EMSWebApi.Infrastructure.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task CreateAndSendNotificationAsync(CreateNotificationDto dto)
        {
            if (string.IsNullOrEmpty(dto.UserId) || string.IsNullOrEmpty(dto.Message))
            {
                _logger.LogWarning("Attempted to create notification with missing UserId or Message.");
                return;
            }

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                Message = dto.Message,
                Link = dto.Link,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Notification created for user {UserId}: {Message}", dto.UserId, dto.Message);

            try
            {
                var notificationDto = new NotificationDto
                {
                    Id = notification.Id,
                    Message = notification.Message,
                    Link = notification.Link,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt
                };
                await _hubContext.Clients.User(dto.UserId).SendAsync("ReceiveNotification", notificationDto);
                _logger.LogInformation("SignalR notification sent to user {UserId}", dto.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR notification to user {UserId}", dto.UserId);
            }
        }
    }
}
