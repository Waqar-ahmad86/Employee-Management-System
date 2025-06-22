using EMSMvc.Core.Application.DTOs;
using EMSMvc.Core.Application.Interfaces;
using EMSMvc.Core.Application.Services;
using EMSMvc.Helpers;
using EMSMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EMSMvc.Controllers
{
    public class UserManagementController : Controller
    {
        private readonly UserManagementService _userManagementService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApiService _apiService;
        private readonly AuthService _authService;

        public UserManagementController(
            UserManagementService userManagementService,
            IHttpContextAccessor httpContextAccessor,
            IApiService apiService,
            AuthService authService)
        {
            _userManagementService = userManagementService;
            _httpContextAccessor = httpContextAccessor;
            _apiService = apiService;
            _authService = authService;
        }


        public async Task<IActionResult> Index(string? searchTerm, string? roleFilter)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();

            var users = await _userManagementService.GetUsersAsync(searchTerm, roleFilter);

            var allRoles = await _userManagementService.GetAllRolesAsync() ?? new List<string>();
            ViewBag.RoleFilterList = allRoles.Select(r => new SelectListItem
            {
                Value = r,
                Text = r,
                Selected = r == roleFilter
            }).ToList();

            ViewBag.CurrentSearchTerm = searchTerm;
            ViewBag.CurrentRoleFilter = roleFilter;

            return View(users ?? new List<User>());
        }

        public async Task<IActionResult> Details(string id)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();

            var user = await _userManagementService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        public async Task<(bool Success, string Message)> UpdateUserDetailsAsync(EditUserDetails model)
        {
            var payload = new { model.Id, model.FullName, model.IsActive };
            try
            {
                var response = await _apiService.SendRequestAsync<ApiResponseWithMessage>(HttpMethod.Put, "UserManagement/user", payload);
                return (response != null && _apiService.LastResponseWasSuccessful, 
                        response?.Message ?? "Failed to update user details.");
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex));
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();

            var user = await _userManagementService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new EditUserDetails
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                IsActive = user.IsActive
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserDetails model)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, message) = await _userManagementService.UpdateUserDetailsAsync(model);

            if (success)
            {
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            else
            {
                TempData["ErrorMessage"] = message;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUserRoles(string id)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();

            var user = await _userManagementService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var allRoles = await _userManagementService.GetAllRolesAsync() ?? new List<string>();

            var viewModel = new EditUserRolesVM
            {
                UserId = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                UserRoles = user.Roles?.ToList() ?? new List<string>(),
                AllRoles = allRoles
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserRoles(EditUserRolesVM model)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();

            if (!ModelState.IsValid)
            {
                model.AllRoles = await _userManagementService.GetAllRolesAsync() ?? new List<string>();
                return View(model);
            }

            var rolesToSet = model.UserRoles ?? new List<string>();
            var (success, message) = await _userManagementService.UpdateUserRolesAsync(model.UserId, rolesToSet);

            if (success)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = message;
                model.AllRoles = await _userManagementService.GetAllRolesAsync() ?? new List<string>();
                return View(model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserLock(string userId, bool lockUser)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext))
            {
                return Json(new { success = false, message = "Unauthorized." });
            }

            var (success, message) = lockUser
                ? await _userManagementService.LockUserAsync(userId)
                : await _userManagementService.UnlockUserAsync(userId);

            return Json(new { success, message });
        }

        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();

            var allRolesFromService = await _userManagementService.GetAllRolesAsync() ?? new List<string>();
            var viewModel = new AdminCreate
            {
                IsActive = true,
                AllRoles = allRolesFromService.Select(r => new SelectListItem { Value = r, Text = r }).ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(AdminCreate model)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();

            if (!ModelState.IsValid)
            {
                var allRolesFromService = await _userManagementService.GetAllRolesAsync() ?? new List<string>();
                model.AllRoles = allRolesFromService.Select(r => new SelectListItem { Value = r, Text = r }).ToList();
                return View(model);
            }

            var (success, message, createdUser) = await _userManagementService.CreateUserByAdminAsync(model);

            if (success)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = message;
                var allRolesFromService = await _userManagementService.GetAllRolesAsync() ?? new List<string>();
                model.AllRoles = allRolesFromService.Select(r => new SelectListItem { Value = r, Text = r }).ToList();
                return View(model);
            }
        }

        public IActionResult Profile()
        {
            if (!SessionHelper.IsUserLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            var userProfile = new UserProfileVM
            {
                FullName = HttpContext.Session.GetString("FullName"),
                Username = HttpContext.Session.GetString("UserName"),
                Role = HttpContext.Session.GetString("UserRole"),
                Email = HttpContext.Session.GetString("UserEmail")
            };

            return View(userProfile);
        }


        [HttpPost, ActionName("DeleteUserConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext))
            {
                return Json(new { success = false, message = "Unauthorized action." });
            }
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "User ID is required." });
            }

            var (success, message) = await _userManagementService.DeleteUserAsync(id);

            return Json(new 
            { 
                success, message = message ?? (success ? "User deleted successfully." : "Failed to delete user.") 
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelectedUsersConfirmed(List<string> ids)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext))
            {
                return Json(new { success = false, message = "Unauthorized action." });
            }

            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "No users selected for deletion." });
            }

            var (success, message, details) = await _userManagementService.DeleteMultipleUsersAsync(ids);

            if (success)
            {
                bool allIntendedDeletionsSuccessful = (details?.Errors == null || !details.Errors.Any()) &&
                                                     (details?.NotFoundIds == null || !details.NotFoundIds.Any());

                return Json(new
                {
                    success = allIntendedDeletionsSuccessful,
                    message = message ?? "Processed delete request."
                });
            }

            return Json(new { success = false, message = message ?? "Failed to delete selected users." });
        }

    }
}
