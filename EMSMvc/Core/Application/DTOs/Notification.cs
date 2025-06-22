using System.ComponentModel.DataAnnotations;

namespace EMSMvc.Core.Application.DTOs
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public string? Link { get; set; }
        public bool IsRead { get; set; }

        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; }
    }
}
