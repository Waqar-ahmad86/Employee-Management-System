using EMS.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace EMSMvc.Core.Application.DTOs
{
    public class LeaveApplication
    {
        public int Id { get; set; }

        public string ApplicantId { get; set; }
        [Display(Name = "Applicant")]
        public string ApplicantName { get; set; }

        public int LeaveTypeId { get; set; }
        [Display(Name = "Leave Type")]
        public string LeaveTypeName { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        public DateTime EndDate { get; set; }

        [Display(Name = "No. of Days")]
        public int NumberOfDays { get; set; }

        [Display(Name = "Reason")]
        public string Reason { get; set; }

        [Display(Name = "Status")]
        public LeaveApplicationStatus Status { get; set; }
        public string StatusString => Status.ToString();


        [Display(Name = "Applied On")]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:mm}")]
        public DateTime AppliedDate { get; set; }

        public string? ApprovedByUserId { get; set; }
        [Display(Name = "Action By")]
        public string? ApprovedByUserName { get; set; }


        [Display(Name = "Action Date")]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:mm}", NullDisplayText = "N/A")]
        public DateTime? ActionDate { get; set; }

        [Display(Name = "Admin Remarks")]
        public string? AdminRemarks { get; set; }
    }
}
