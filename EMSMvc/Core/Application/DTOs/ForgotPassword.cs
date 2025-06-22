using System.ComponentModel.DataAnnotations;

namespace EMSMvc.Core.Application.DTOs
{
    public class ForgotPassword
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
