using EMSWebApi.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace EMSWebApi.Infrastructure.Services.Identity
{
    public class RoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleService(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _roleManager.RoleExistsAsync(roleName);
        }

        public async Task<(bool succeeded, string message)> AssignRoleToUserAsync(
            UserManager<ApplicationUser> userManager,
            string userId,
            string roleName)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return (false, $"User with ID '{userId}' not found.");

            if (!await _roleManager.RoleExistsAsync(roleName))
                return (false, $"Role '{roleName}' does not exist.");

            if (await userManager.IsInRoleAsync(user, roleName))
                return (false, $"User '{user.UserName}' is already in role '{roleName}'.");

            var result = await userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
                return (true, $"Role '{roleName}' assigned to user '{user.UserName}' successfully.");

            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return (false, errors);
        }

        public async Task<(bool succeeded, string message)> RemoveRoleFromUserAsync(
            UserManager<ApplicationUser> userManager,
            string userId,
            string roleName)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return (false, $"User with ID '{userId}' not found.");

            if (!await _roleManager.RoleExistsAsync(roleName))
                return (false, $"Role '{roleName}' does not exist.");

            if (!await userManager.IsInRoleAsync(user, roleName))
                return (false, $"User '{user.UserName}' is not in role '{roleName}'.");

            var result = await userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded)
                return (true, $"Role '{roleName}' removed from user '{user.UserName}' successfully.");

            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return (false, errors);
        }

        public async Task<(bool succeeded, string message)> UpdateUserRolesAsync(
            UserManager<ApplicationUser> userManager,
            string userId,
            IEnumerable<string> roles)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return (false, $"User with ID '{userId}' not found.");

            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                    return (false, $"Role '{roleName}' does not exist.");
            }

            var currentRoles = await userManager.GetRolesAsync(user);

            var rolesToAdd = roles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(roles).ToList();

            if (rolesToRemove.Any())
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    var errors = string.Join("; ", removeResult.Errors.Select(e => e.Description));
                    return (false, errors);
                }
            }

            if (rolesToAdd.Any())
            {
                var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    var errors = string.Join("; ", addResult.Errors.Select(e => e.Description));
                    return (false, errors);
                }
            }

            return (true, "User roles updated successfully.");
        }
    }
}
