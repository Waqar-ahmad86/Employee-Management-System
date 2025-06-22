using System.ComponentModel.DataAnnotations;

namespace EMSWebApi.Application.DTOs.Notices
{
    public class UpdateNoticeDto : CreateNoticeDto
    {
        [Required]
        public Guid Id { get; set; }
    }
}
