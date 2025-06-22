using System.ComponentModel.DataAnnotations;

namespace EMSWebApi.Application.DTOs.Auth
{
    public class AdminCreateUserDto
    {
        [Required]
        [StringLength(256)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        public List<string>? InitialRoles { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
