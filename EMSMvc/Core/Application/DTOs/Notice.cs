using EMS.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace EMSMvc.Core.Application.DTOs
{
    public class Notice
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Display(Name = "Published On")]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy, HH:mm}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Expires On")]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy, HH:mm}", NullDisplayText = "Never")]
        public DateTime? ExpiresAt { get; set; }

        public NoticeAudience Audience { get; set; }

        [Display(Name = "Audience")]
        public string AudienceString => Audience.ToString();


        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}
