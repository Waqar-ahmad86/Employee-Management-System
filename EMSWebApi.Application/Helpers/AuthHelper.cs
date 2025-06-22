using EMSWebApi.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EMSWebApi.Application.Helpers
{
    public static class AuthHelper
    {
        public static async Task<(bool IsLocked, string Message, bool IsAdminLocked)> CheckUserLockoutAsync(
            ApplicationUser user,
            UserManager<ApplicationUser> userManager,
            ILogger logger)
        {
            if (!user.IsActive)
            {
                logger.LogWarning("Login attempt for deactivated user {Username}", user.UserName);
                return (true, "Your account has been deactivated.", false);
            }

            if (await userManager.IsLockedOutAsync(user))
            {
                bool isAdminLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value >= DateTimeOffset.UtcNow.AddYears(100);
                string message = isAdminLocked
                    ? "Admin blocked your account. Please contact admin."
                    : "Your account is locked. It may be due to too many failed login attempts.";

                logger.LogWarning("Login attempt for locked out user {Username}", user.UserName);
                return (true, message, isAdminLocked);
            }

            return (false, string.Empty, false);
        }

        public static string GetDecodedResetToken(string token, ILogger logger, string email)
        {
            try
            {
                return System.Text.Encoding.UTF8.GetString(Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(token));
            }
            catch (FormatException ex)
            {
                logger.LogWarning(ex, "Invalid token format during password reset for email: {Email}", email);
                return null!;
            }
        }
    }
}
