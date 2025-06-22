using System.ComponentModel.DataAnnotations;

namespace EMSWebApi.Application.DTOs.Leave
{
    public class CreateLeaveTypeDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }
        public int? DefaultDaysAllowed { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
