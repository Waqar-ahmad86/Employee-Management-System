using EMSWebApi.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace EMSWebApi.Infrastructure.Services.Identity
{
    public class UserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public UserManager<ApplicationUser> UserManager => _userManager;

        public async Task<ApplicationUser?> FindUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<(bool succeeded, string message)> UpdateUserAsync(ApplicationUser user)
        {
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return (true, "User details updated successfully.");

            var errors = string.Join("; ", result.Errors);
            return (false, errors);
        }

        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<(bool succeeded, string message)> LockUserAsync(ApplicationUser user)
        {
            if (await _userManager.IsLockedOutAsync(user))
                return (false, $"User '{user.UserName}' is already locked.");

            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            if (result.Succeeded)
                return (true, $"User '{user.UserName}' locked successfully.");

            var errors = string.Join("; ", result.Errors);
            return (false, errors);
        }

        public async Task<(bool succeeded, string message)> UnlockUserAsync(ApplicationUser user)
        {
            var result = await _userManager.SetLockoutEndDateAsync(user, null);
            if (result.Succeeded)
                return (true, $"User '{user.UserName}' unlocked successfully.");

            var errors = string.Join("; ", result.Errors);
            return (false, errors);
        }

        public async Task<(bool succeeded, string message)> CreateUserAsync(ApplicationUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
                return (true, "User created successfully.");

            var errors = string.Join("; ", result.Errors);
            return (false, errors);
        }
    }
}
