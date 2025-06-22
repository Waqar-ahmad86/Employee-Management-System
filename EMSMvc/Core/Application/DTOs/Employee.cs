using System.ComponentModel.DataAnnotations;

namespace EMSMvc.Core.Application.DTOs
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Display(Name = "Department")]
        public string Department { get; set; }

        [Required(ErrorMessage = "Salary is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Salary must be greater than 0")]
        [DataType(DataType.Currency)]
        public decimal Salary { get; set; }

        [Display(Name = "Date of Joining")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Date is required")]
        public DateTime? DateOfJoining { get; set; }

        [Display(Name = "Contact Number")]
        [Phone]
        public string? ContactNumber { get; set; }

        [Display(Name = "Job Title")]
        public string? JobTitle { get; set; }
    }
}
