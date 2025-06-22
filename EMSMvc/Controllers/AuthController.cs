using Microsoft.AspNetCore.Mvc;
using EMSMvc.Helpers;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using EMSMvc.Core.Application.Services;
using EMSMvc.Core.Application.DTOs;
using EMSMvc.Core.Application.Interfaces;

namespace EMSMvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly IApiService _apiService;


        public AuthController(AuthService authService, IApiService apiService)
        {
            _authService = authService;
            _apiService = apiService;
        }

        public IActionResult Login()
        {
            if (SessionHelper.IsUserLoggedIn(HttpContext))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (token, errorMessage, isAdminLocked) = await _authService.LoginAsync(model);

            if (string.IsNullOrEmpty(token))
            {
                ViewData["Error"] = errorMessage ?? "Invalid username or password. Please try again.";
                return View(model);
            }

            var claims = JwtHelper.DecodeToken(token);

            if (claims == null || !claims.ContainsKey("UserId") || string.IsNullOrEmpty(claims["UserId"]) ||
                !claims.ContainsKey("UserRole") || string.IsNullOrEmpty(claims["UserRole"]))
            {
                ViewData["Error"] = "Failed to process login information. Essential details missing from token.";
                return View(model);
            }

            HttpContext.Session.SetString("JwtToken", token);
            HttpContext.Session.SetString("UserId", claims["UserId"]);
            HttpContext.Session.SetString("UserRole", claims["UserRole"]);
            HttpContext.Session.SetString("UserEmail", claims.GetValueOrDefault("UserEmail", ""));
            HttpContext.Session.SetString("UserName", claims.GetValueOrDefault("UserName", ""));
            HttpContext.Session.SetString("FullName", claims.GetValueOrDefault("FullName", ""));
            HttpContext.Session.SetString("UserRolesAll", claims.GetValueOrDefault("UserRoles", ""));

            TempData["ShowLoginLoader"] = true;
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(provider)) provider = "Google";

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/Dashboard/Index");
            if (remoteError != null)
            {
                TempData["ErrorMessage"] = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }

            var info = await HttpContext.AuthenticateAsync("EMSMvcCookieAuth");

            if (info?.Principal == null)
            {
                TempData["ErrorMessage"] = "Error loading external login information.";
                return RedirectToAction(nameof(Login));
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var providerKey = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
            var fullName = info.Principal.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerKey))
            {
                TempData["ErrorMessage"] = "Could not retrieve required information (email or user ID) from the external provider.";
                return RedirectToAction(nameof(Login));
            }

            var externalLoginRequest = new ExternalLoginRequest
            {
                Provider = info.Principal.Identity.AuthenticationType,
                ProviderKey = providerKey,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                FullName = (!string.IsNullOrEmpty(fullName) && fullName != email) ? fullName : $"{firstName} {lastName}".Trim()
            };
            if (string.IsNullOrWhiteSpace(externalLoginRequest.FullName)) externalLoginRequest.FullName = email;

            var (apiSuccess, apiMessage, apiLoginResponse, isAdminLockedExt) = await _authService.ExternalLoginApiAsync(externalLoginRequest);

            if (apiSuccess && apiLoginResponse != null && !string.IsNullOrEmpty(apiLoginResponse.Token))
            {
                var internalClaims = JwtHelper.DecodeToken(apiLoginResponse.Token);
                if (internalClaims == null || !internalClaims.ContainsKey("UserId") || string.IsNullOrEmpty(internalClaims["UserId"]) ||
                    !internalClaims.ContainsKey("UserRole") || string.IsNullOrEmpty(internalClaims["UserRole"]))
                {
                    TempData["ErrorMessage"] = "Failed to process internal login information after external authentication.";
                    return RedirectToAction(nameof(Login));
                }

                HttpContext.Session.SetString("JwtToken", apiLoginResponse.Token);
                HttpContext.Session.SetString("UserId", internalClaims["UserId"]);
                HttpContext.Session.SetString("UserRole", internalClaims["UserRole"]);
                HttpContext.Session.SetString("UserEmail", internalClaims.GetValueOrDefault("UserEmail", ""));
                HttpContext.Session.SetString("UserName", internalClaims.GetValueOrDefault("UserName", ""));
                HttpContext.Session.SetString("FullName", internalClaims.GetValueOrDefault("FullName", ""));
                HttpContext.Session.SetString("UserRolesAll", internalClaims.GetValueOrDefault("UserRoles", ""));

                await HttpContext.SignOutAsync("EMSMvcCookieAuth");

                TempData["ShowLoginLoader"] = true;
                return LocalRedirect(returnUrl);
            }
            else
            {
                TempData["ErrorMessage"] = $"Login with {externalLoginRequest.Provider} failed. {(string.IsNullOrEmpty(apiMessage) ? "Please try again." : apiMessage)}";

                await HttpContext.SignOutAsync("EMSMvcCookieAuth");
                return RedirectToAction(nameof(Login));
            }
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Register model)
        {
            if (ModelState.IsValid)
            {
                var errorMessage = await _authService.RegisterAsync(model);
                if (errorMessage == null)
                {
                    TempData["SuccessMessage"] = "Registration Successful! Please log in.";
                    return RedirectToAction("Login");
                }
                ViewData["Error"] = errorMessage;
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPasswordAjax([FromBody] ForgotPassword model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

                return BadRequest(new { message = "Validation failed.", errors = errors });
            }

            var (success, message) = await _authService.ForgotPasswordAsync(model);
            return Ok(new { message = message });
        }

        [HttpGet]
        public IActionResult ResetPasswordPage(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Invalid password reset link.";
                return RedirectToAction("Login");
            }
            var model = new ResetPassword { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordAjax([FromBody] ResetPassword model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(new { message = "Validation failed.", errors = errors });
            }

            var (success, message) = await _authService.ResetPasswordAsync(model);

            if (success)
            {
                return Ok(new { message = message });
            }
            else
            {
                return BadRequest(new { message = message });
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _apiService.LogoutAsync();
            await HttpContext.SignOutAsync("EMSMvcCookieAuth");
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
    }
}