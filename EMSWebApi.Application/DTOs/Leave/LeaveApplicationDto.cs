using EMS.Common.Enums;

namespace EMSWebApi.Application.DTOs.Leave
{
    public class LeaveApplicationDto
    {
        public int Id { get; set; }
        public string ApplicantId { get; set; }
        public string ApplicantName { get; set; }
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfDays { get; set; }
        public string Reason { get; set; }
        public LeaveApplicationStatus Status { get; set; }
        public string StatusString => Status.ToString();
        public DateTime AppliedDate { get; set; }
        public string? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public DateTime? ActionDate { get; set; }
        public string? AdminRemarks { get; set; }
    }
}
