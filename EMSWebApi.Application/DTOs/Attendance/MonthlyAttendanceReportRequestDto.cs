using System.ComponentModel.DataAnnotations;

namespace EMSWebApi.Application.DTOs.Attendance
{
    public class MonthlyAttendanceReportRequestDto
    {
        [Required]
        public int Year { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }
        public string? UserName { get; set; }
        public string? RoleName { get; set; }
    }
}
