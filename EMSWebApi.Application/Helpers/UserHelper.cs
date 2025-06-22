using System.Security.Claims;

namespace EMSWebApi.Application.Helpers
{
    public static class UserHelper
    {
        public static string? GetCurrentUserId(ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
