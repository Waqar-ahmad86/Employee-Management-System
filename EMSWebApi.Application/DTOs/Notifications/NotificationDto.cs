namespace EMSWebApi.Application.DTOs.Notifications
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public string? Link { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtFormatted => CreatedAt.ToString("g");
        public string TimeAgo => GetTimeAgo(CreatedAt);
        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;
            if (timeSpan.TotalSeconds < 60) return $"{Math.Floor(timeSpan.TotalSeconds)}s ago";
            if (timeSpan.TotalMinutes < 60) return $"{Math.Floor(timeSpan.TotalMinutes)}m ago";
            if (timeSpan.TotalHours < 24) return $"{Math.Floor(timeSpan.TotalHours)}h ago";
            if (timeSpan.TotalDays < 7) return $"{Math.Floor(timeSpan.TotalDays)}d ago";
            return dateTime.ToString("dd MMM yyyy");
        }
    }
}
