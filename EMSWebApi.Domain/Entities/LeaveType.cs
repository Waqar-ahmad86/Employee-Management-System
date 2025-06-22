using System.ComponentModel.DataAnnotations;

namespace EMSWebApi.Domain.Entities
{
    public class LeaveType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }
        public int? DefaultDaysAllowed { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual ICollection<LeaveApplication> LeaveApplications { get; set; } = new List<LeaveApplication>();
    }
}
