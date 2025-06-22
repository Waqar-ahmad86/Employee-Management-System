namespace EMSWebApi.Application.DTOs.Notifications
{
    public class CreateNotificationDto
    {
        public string UserId { get; set; }
        public string Message { get; set; }
        public string? Link { get; set; }
    }
}
