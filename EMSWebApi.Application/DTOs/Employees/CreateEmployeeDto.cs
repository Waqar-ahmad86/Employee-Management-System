using System.ComponentModel.DataAnnotations;

namespace EMSWebApi.Application.DTOs.Employees
{
    public class CreateEmployeeDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Department { get; set; }

        [Range(0, (double)decimal.MaxValue)]
        public decimal Salary { get; set; }
        public DateTime? DateOfJoining { get; set; }

        [StringLength(15)]
        public string? ContactNumber { get; set; }

        [StringLength(100)]
        public string? JobTitle { get; set; }
    }
}
