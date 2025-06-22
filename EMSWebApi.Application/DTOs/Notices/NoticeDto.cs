using EMS.Common.Enums;

namespace EMSWebApi.Application.DTOs.Notices
{
    public class NoticeDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public NoticeAudience Audience { get; set; }
        public bool IsActive { get; set; }
        public string CreatedAtFormatted => CreatedAt.ToString("dd MMM yyyy, HH:mm");
        public string? ExpiresAtFormatted => ExpiresAt?.ToString("dd MMM yyyy, HH:mm");
    }
}
