using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMSWebApi.Domain.Entities
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Department { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Salary { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfJoining { get; set; }

        [StringLength(15)]
        public string? ContactNumber { get; set; }

        [StringLength(100)]
        public string? JobTitle { get; set; }
    }
}
