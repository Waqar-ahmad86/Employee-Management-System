using EMSMvc.Core.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace EMSMvc.ViewModels
{
    public class MonthlyReportRequestVM
    {
        [Required]
        [Display(Name = "Year")]
        public int Year { get; set; } = DateTime.Today.Year;

        [Required]
        [Range(1, 12)]
        [Display(Name = "Month")]
        public int Month { get; set; } = DateTime.Today.Month;

        [Display(Name = "Name")]
        public string? Name { get; set; }

        [Display(Name = "Role")]
        public string? RoleName { get; set; }

        public List<SelectListItem> Years { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Months { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
        public List<MonthlyAttendanceReportItem>? ReportData { get; set; }
    }
}
