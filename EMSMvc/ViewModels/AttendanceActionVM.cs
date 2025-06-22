using EMSMvc.Core.Application.DTOs;

namespace EMSMvc.ViewModels
{
    public class AttendanceActionVM
    {
        public string? Remarks { get; set; }
        public AttendanceRecord? TodaysRecord { get; set; }
    }
}