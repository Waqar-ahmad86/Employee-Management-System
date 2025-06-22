using EMSWebApi.Application.DTOs.Notifications;

namespace EMSWebApi.Application.Interfaces.Infrastructure
{
    public interface INotificationService
    {
        Task CreateAndSendNotificationAsync(CreateNotificationDto dto);
    }
}
