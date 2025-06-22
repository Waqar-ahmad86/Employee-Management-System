using System.ComponentModel.DataAnnotations;

namespace EMSWebApi.Application.DTOs.Users
{
    public class RoleAssignDto
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string RoleName { get; set; }
    }
}
