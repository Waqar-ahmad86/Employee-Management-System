using System.ComponentModel.DataAnnotations;

namespace EMSWebApi.Application.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
