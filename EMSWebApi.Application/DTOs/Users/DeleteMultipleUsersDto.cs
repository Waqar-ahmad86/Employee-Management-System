using System.ComponentModel.DataAnnotations;

namespace EMSWebApi.Application.DTOs.Users
{
    public class DeleteMultipleUsersDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one user ID must be provided.")]
        public List<string> Ids { get; set; } = new List<string>();
    }
}
