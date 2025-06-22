using System.ComponentModel.DataAnnotations;

namespace EMSWebApi.Application.DTOs.Users
{
    public class UserRoleUpdateDto
    {
        [Required]
        public string UserId { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
