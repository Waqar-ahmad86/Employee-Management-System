using System.ComponentModel.DataAnnotations;

namespace EMSWebApi.Application.DTOs.Leave
{
    public class CreateLeaveApplicationDto
    {
        [Required]
        public int LeaveTypeId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 5)]
        public string Reason { get; set; }
    }
}
