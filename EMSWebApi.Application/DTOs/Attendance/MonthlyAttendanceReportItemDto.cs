namespace EMSWebApi.Application.DTOs.Attendance
{
    public class MonthlyAttendanceReportItemDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int TotalDaysInMonth { get; set; }
        public int WorkingDaysInMonth { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public double TotalWorkHours { get; set; }
        public double AverageWorkHours => PresentDays > 0 ? TotalWorkHours / PresentDays : 0;
    }
}
