using System.ComponentModel.DataAnnotations;

namespace EMSWebApi.Application.DTOs.Auth
{
    public class ExternalLoginRequestDto
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        public string ProviderKey { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
    }
}
