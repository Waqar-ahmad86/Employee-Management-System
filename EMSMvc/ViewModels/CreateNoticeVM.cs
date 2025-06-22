using EMS.Common.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace EMSMvc.ViewModels
{
    public class CreateNoticeVM
    {
        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Title { get; set; }

        [Required]
        [MinLength(10)]
        public string Content { get; set; }

        [Display(Name = "Expires On (Optional)")]
        [DataType(DataType.DateTime)]
        public DateTime? ExpiresAt { get; set; }

        [Required]
        public NoticeAudience Audience { get; set; } = NoticeAudience.All;

        [Display(Name = "Publish Immediately")]
        public bool IsActive { get; set; } = true;

        public List<SelectListItem> AudienceOptions { get; set; }

        public CreateNoticeVM()
        {
            AudienceOptions = System.Enum.GetValues(typeof(NoticeAudience))
                                  .Cast<NoticeAudience>()
                                  .Select(v => new SelectListItem
                                  {
                                      Text = v.ToString(),
                                      Value = ((int)v).ToString()
                                  }).ToList();
        }
    }
}
