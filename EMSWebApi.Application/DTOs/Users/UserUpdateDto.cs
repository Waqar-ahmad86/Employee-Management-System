using System.ComponentModel.DataAnnotations;

namespace EMSWebApi.Application.DTOs.Users
{
    public class UserUpdateDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string FullName { get; set; }
        public bool IsActive { get; set; }
    }
}
