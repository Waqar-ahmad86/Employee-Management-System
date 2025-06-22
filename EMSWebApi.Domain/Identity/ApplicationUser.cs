using Microsoft.AspNetCore.Identity;

namespace EMSWebApi.Domain.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
