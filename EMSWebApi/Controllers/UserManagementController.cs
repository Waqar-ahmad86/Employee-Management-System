using EMSWebApi.Application.DTOs.Auth;
using EMSWebApi.Application.DTOs.Users;
using EMSWebApi.Application.Mappings;
using EMSWebApi.Domain.Identity;
using EMSWebApi.Infrastructure.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMSWebApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly RoleService _roleService;
        private readonly ILogger<UserManagementController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(
            UserService userService,
            RoleService roleService,
            ILogger<UserManagementController> logger,
            RoleManager<IdentityRole> roleManager)
        {
            _userService = userService;
            _roleService = roleService;
            _logger = logger;
            _roleManager = roleManager;
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers([FromQuery] string? searchTerm, [FromQuery] string? role)
        {
            var query = _userService.UserManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u =>
                    (u.UserName != null && u.UserName.Contains(searchTerm)) ||
                    (u.Email != null && u.Email.Contains(searchTerm)) ||
                    (u.FullName != null && u.FullName.Contains(searchTerm))
                );
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                var usersInRole = await _userService.UserManager.GetUsersInRoleAsync(role);
                var userIdsInRole = usersInRole.Select(u => u.Id).ToList();
                query = query.Where(u => userIdsInRole.Contains(u.Id));
            }

            var users = await query.OrderBy(u => u.UserName).ToListAsync();

            var userDto = new List<UserDto>();

            foreach (var user in users)
            {
                userDto.Add(await user.ToUserDtoAsync(_userService.UserManager));
            }

            return Ok(userDto);
        }

        [HttpGet("user/{id}")]
        public async Task<ActionResult<UserDto>> GetUser(string id)
        {
            var user = await _userService.FindUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", id);
                return NotFound(new { Message = "User not found." });
            }

            var userDto = await user.ToUserDtoAsync(_userService.UserManager);
            return Ok(userDto);
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<string>>> GetRoles()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return Ok(roles);
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] RoleAssignDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (succeeded, message) = await _roleService.AssignRoleToUserAsync(_userService.UserManager, model.UserId, model.RoleName);
            if (succeeded)
            {
                _logger.LogInformation(message);
                return Ok(new { Message = message });
            }

            return BadRequest(new { Message = message });
        }

        [HttpPost("remove-role")]
        public async Task<IActionResult> RemoveRoleFromUser([FromBody] RoleAssignDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (succeeded, message) = await _roleService.RemoveRoleFromUserAsync(_userService.UserManager, model.UserId, model.RoleName);
            if (succeeded)
            {
                _logger.LogInformation(message);
                return Ok(new { Message = message });
            }

            return BadRequest(new { Message = message });
        }

        [HttpPut("user")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userService.FindUserByIdAsync(model.Id);
            if (user == null)
                return NotFound(new { Message = "User not found." });

            user.FullName = model.FullName;
            user.IsActive = model.IsActive;

            var (succeeded, message) = await _userService.UpdateUserAsync(user);
            if (succeeded)
            {
                _logger.LogInformation(message);
                return Ok(new { Message = message });
            }

            return BadRequest(new { Message = message });
        }

        [HttpPut("update-user-roles")]
        public async Task<IActionResult> UpdateUserRoles([FromBody] UserRoleUpdateDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (succeeded, message) = await _roleService.UpdateUserRolesAsync(_userService.UserManager, model.UserId, model.Roles);
            if (succeeded)
            {
                _logger.LogInformation(message);
                return Ok(new { Message = message });
            }

            return BadRequest(new { Message = message });
        }

        [HttpPost("lock-user/{id}")]
        public async Task<IActionResult> LockUser(string id)
        {
            var user = await _userService.FindUserByIdAsync(id);
            if (user == null) return NotFound(new { Message = "User not found." });

            var (succeeded, message) = await _userService.LockUserAsync(user);
            if (succeeded)
            {
                _logger.LogInformation(message);
                return Ok(new { Message = message });
            }
            return BadRequest(new { Message = message });
        }

        [HttpPost("unlock-user/{id}")]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var user = await _userService.FindUserByIdAsync(id);
            if (user == null) return NotFound(new { Message = "User not found." });

            var (succeeded, message) = await _userService.UnlockUserAsync(user);
            if (succeeded)
            {
                _logger.LogInformation(message);
                return Ok(new { Message = message });
            }
            return BadRequest(new { Message = message });
        }

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUserByAdmin([FromBody] AdminCreateUserDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                IsActive = model.IsActive,
                EmailConfirmed = true
            };

            var (succeeded, message) = await _userService.CreateUserAsync(user, model.Password);
            if (!succeeded) return BadRequest(new { Message = message });

            _logger.LogInformation("User {UserName} created successfully by admin.", user.UserName);

            if (model.InitialRoles != null && model.InitialRoles.Any())
            {
                foreach (var roleName in model.InitialRoles)
                {
                    if (await _roleService.RoleExistsAsync(roleName))
                    {
                        await _userService.UserManager.AddToRoleAsync(user, roleName);
                    }
                    else
                    {
                        _logger.LogWarning("Role {RoleName} specified during user creation does not exist.", roleName);
                    }
                }
            }
            else
            {
                if (await _roleService.RoleExistsAsync("User"))
                {
                    await _userService.UserManager.AddToRoleAsync(user, "User");
                }
            }

            return Ok(new { Message = message, User = await user.ToUserDtoAsync(_userService.UserManager) });
        }



        [HttpDelete("user/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { Message = "User ID cannot be null or empty." });
            }

            var user = await _userService.UserManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("DeleteUser: User with ID {UserId} not found.", id);
                return NotFound(new { Message = "User not found." });
            }

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (user.Id == currentUserId)
            {
                _logger.LogWarning("DeleteUser: Admin user {UserId} attempted to delete themselves.", user.Id);
                return BadRequest(new { Message = "You cannot delete your own account." });
            }

            var isAdmin = await _userService.UserManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                var admins = await _userService.UserManager.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    _logger.LogWarning("DeleteUser: Attempt to delete the last admin user {UserId}.", user.Id);
                    return BadRequest(new { Message = "Cannot delete the last administrator account." });
                }
            }


            var result = await _userService.UserManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID: {UserId} (Username: {Username}) deleted successfully by admin.", id, user.UserName);
                return Ok(new { Message = "User deleted successfully." });
            }

            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            _logger.LogError("Error deleting user ID {UserId}: {Errors}", id, errors);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"Error deleting user: {errors}" });
        }

        [HttpPost("delete-multiple-users")]
        public async Task<IActionResult> DeleteMultipleUsers([FromBody] DeleteMultipleUsersDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.Ids == null || !request.Ids.Any())
            {
                _logger.LogWarning("DeleteMultipleUsers: No IDs provided for deletion.");
                return BadRequest(new { Message = "No user IDs provided for deletion." });
            }

            var deletedUsernames = new List<string>();
            var notFoundIds = new List<string>();
            var errorMessages = new List<string>();
            var currentAdminUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            foreach (var id in request.Ids.Distinct())
            {
                if (string.IsNullOrEmpty(id))
                {
                    errorMessages.Add("A null or empty user ID was provided.");
                    continue;
                }

                if (id == currentAdminUserId)
                {
                    errorMessages.Add($"Skipped deleting current admin user (ID: {id}). You cannot delete yourself.");
                    _logger.LogWarning("DeleteMultipleUsers: Admin attempted to include themselves (ID: {UserId}) in bulk delete.", id);
                    continue;
                }

                var user = await _userService.UserManager.FindByIdAsync(id);
                if (user != null)
                {
                    var isAdmin = await _userService.UserManager.IsInRoleAsync(user, "Admin");
                    if (isAdmin)
                    {
                        var admins = await _userService.UserManager.GetUsersInRoleAsync("Admin");
                        if (admins.Count(a => !request.Ids.Contains(a.Id) || 
                              a.Id == id) <= 0 && admins.Count <= request.Ids.Count(rid => admins.Any(adm => adm.Id == rid)))
                        {
                            errorMessages.Add($"Cannot delete user '{user.UserName}' (ID: {id}) as it would remove the last administrator(s).");
                            _logger.LogWarning("DeleteMultipleUsers: Attempt to delete last admin user '{Username}' (ID: {UserId}) in bulk.", user.UserName, id);
                            continue;
                        }
                    }

                    var result = await _userService.UserManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        deletedUsernames.Add(user.UserName ?? id);
                        _logger.LogInformation("User '{Username}' (ID: {UserId}) deleted successfully in bulk by admin.", user.UserName, id);
                    }
                    else
                    {
                        var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                        errorMessages.Add($"Failed to delete user '{user.UserName}' (ID: {id}): {errors}");
                        _logger.LogError("Error deleting user '{Username}' (ID: {UserId}) in bulk: {Errors}", user.UserName, id, errors);
                    }
                }
                else
                {
                    notFoundIds.Add(id);
                    _logger.LogWarning("DeleteMultipleUsers: User with ID {UserId} not found during bulk delete.", id);
                }
            }

            var responseMessage = $"Processed delete request. Successfully deleted users: {string.Join(", ", deletedUsernames)}.";
            if (notFoundIds.Any())
            {
                responseMessage += $" Users not found for IDs: {string.Join(", ", notFoundIds)}.";
            }
            if (errorMessages.Any())
            {
                responseMessage += $" Errors encountered: {string.Join("; ", errorMessages)}.";
            }

            if (deletedUsernames.Any() && !notFoundIds.Any() && !errorMessages.Any())
            {
                return Ok(new { Message = responseMessage, DeletedCount = deletedUsernames.Count });
            }
            else if (deletedUsernames.Any() || notFoundIds.Any() || errorMessages.Any())
            {
                return Ok(new
                {
                    Message = responseMessage,
                    DeletedCount = deletedUsernames.Count,
                    NotFoundIds = notFoundIds,
                    Errors = errorMessages
                });
            }
            else
            {
                return BadRequest(new { Message = responseMessage.Replace("Processed delete request. ", "") });
            }
        }
    }
}