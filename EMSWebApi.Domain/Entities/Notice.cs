using EMS.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace EMSWebApi.Domain.Entities
{
    public class Notice
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }

        [Required]
        public string CreatedByUserId { get; set; }
        public NoticeAudience Audience { get; set; } = NoticeAudience.All;
        public bool IsActive { get; set; } = true;
    }
}
