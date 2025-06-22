using EMS.Common.Enums;
using EMSMvc.Core.Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace EMSMvc.ViewModels
{
    public class LeaveApprovalVM
    {
        public int LeaveApplicationId { get; set; }
        public LeaveApplication? ApplicationDetails { get; set; }

        [Required]
        [Display(Name = "Action")]
        public LeaveApplicationStatus NewStatus { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(500)]
        [Display(Name = "Admin Remarks (Optional for Approve, Required for Reject)")]
        public string? AdminRemarks { get; set; }
    }
}
