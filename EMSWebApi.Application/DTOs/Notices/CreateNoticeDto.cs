using EMS.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace EMSWebApi.Application.DTOs.Notices
{
    public class CreateNoticeDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public NoticeAudience Audience { get; set; } = NoticeAudience.All;
        public bool IsActive { get; set; } = true;
    }
}
