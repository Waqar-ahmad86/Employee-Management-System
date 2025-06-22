using EMS.Common.Enums;

namespace EMSWebApi.Application.DTOs.Attendance
{
    public class AttendanceRecordDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? CheckInTime { get; set; }
        public string CheckInTimeString => CheckInTime?.ToString(@"hh\:mm");
        public TimeSpan? CheckOutTime { get; set; }
        public string CheckOutTimeString => CheckOutTime?.ToString(@"hh\:mm");
        public AttendanceStatus Status { get; set; }
        public double? WorkHours { get; set; }
        public string? Remarks { get; set; }
    }
}
