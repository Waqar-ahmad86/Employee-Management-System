using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace EMSMvc.ViewModels
{
    public class ApplyLeaveVM
    {
        [Required]
        [Display(Name = "Leave Type")]
        public int SelectedLeaveTypeId { get; set; }
        public List<SelectListItem> AvailableLeaveTypes { get; set; } = new List<SelectListItem>();

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [Required]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Reason must be between 10 and 500 characters.")]
        [DataType(DataType.MultilineText)]
        public string Reason { get; set; }

        [Display(Name = "Calculated Leave Days")]
        public int CalculatedNumberOfDays { get; set; }

    }
}
