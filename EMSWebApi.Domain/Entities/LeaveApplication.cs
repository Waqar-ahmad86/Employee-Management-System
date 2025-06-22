using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using EMSWebApi.Domain.Identity;
using EMS.Common.Enums;

namespace EMSWebApi.Domain.Entities
{
    public class LeaveApplication
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicantId { get; set; }

        [ForeignKey("ApplicantId")]
        public virtual ApplicationUser Applicant { get; set; }

        [Required]
        public int LeaveTypeId { get; set; }
        [ForeignKey("LeaveTypeId")]
        public virtual LeaveType LeaveType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public int NumberOfDays
        {
            get
            {
                int days = 0;
                for (DateTime date = StartDate.Date; date <= EndDate.Date; date = date.AddDays(1))
                {
                    if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    {
                        days++;
                    }
                }
                return days;
            }
        }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }
        public LeaveApplicationStatus Status { get; set; } = LeaveApplicationStatus.Pending;
        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
        public string? ApprovedByUserId { get; set; }

        [ForeignKey("ApprovedByUserId")]
        public virtual ApplicationUser? ApprovedByUser { get; set; }
        public DateTime? ActionDate { get; set; }

        [StringLength(500)]
        public string? AdminRemarks { get; set; }
    }
}
