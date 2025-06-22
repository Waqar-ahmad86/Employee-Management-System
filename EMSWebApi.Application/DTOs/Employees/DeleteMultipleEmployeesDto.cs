using System.ComponentModel.DataAnnotations;

namespace EMSWebApi.Application.DTOs.Employees
{
    public class DeleteMultipleEmployeesDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one employee ID must be provided.")]
        public List<int> Ids { get; set; } = new List<int>();
    }
}
