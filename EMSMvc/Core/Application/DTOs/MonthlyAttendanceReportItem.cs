using System.ComponentModel.DataAnnotations;

namespace EMSMvc.Core.Application.DTOs
{
    public class MonthlyAttendanceReportItem
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int TotalDaysInMonth { get; set; }
        public int WorkingDaysInMonth { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public double TotalWorkHours { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public double AverageWorkHours { get; set; }
    }
}
