using EMS.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace EMSWebApi.Application.DTOs.Leave
{
    public class UpdateLeaveApplicationStatusDto
    {
        [Required]
        public LeaveApplicationStatus NewStatus { get; set; }

        [StringLength(500)]
        public string? AdminRemarks { get; set; }
    }
}
