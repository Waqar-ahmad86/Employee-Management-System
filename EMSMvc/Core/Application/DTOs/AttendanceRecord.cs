using EMS.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace EMSMvc.Core.Application.DTOs
{
    public class AttendanceRecord
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        public DateTime Date { get; set; }

        [Display(Name = "Check-in")]
        public TimeSpan? CheckInTime { get; set; }
        public string CheckInTimeString => CheckInTime?.ToString(@"hh\:mm");

        [Display(Name = "Check-out")]
        public TimeSpan? CheckOutTime { get; set; }
        public string CheckOutTimeString => CheckOutTime?.ToString(@"hh\:mm");

        public AttendanceStatus Status { get; set; }

        [Display(Name = "Work Hours")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public double? WorkHours { get; set; }
        public string? Remarks { get; set; }
    }
}
