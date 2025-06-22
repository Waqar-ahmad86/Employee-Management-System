using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using EMSWebApi.Domain.Identity;
using System.Text;
using EMSWebApi.Infrastructure.Services.Authentication;
using EMSWebApi.Application.DTOs.Auth;
using EMSWebApi.Application.Interfaces.Infrastructure;
using EMSWebApi.Application.Helpers;

namespace EMSWebApi.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtTokenService jwtTokenService,
            IEmailService emailService, 
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FullName = model.FullName
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    return Ok(new { Message = "User registered successfully." });
                }
                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Registration failed for user {Username}: {ErrorDescription}", 
                        model.Username, 
                        error.Description);
                }
                return BadRequest(result.Errors);
            }
            return BadRequest("Invalid registration data.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user == null)
            {
                _logger.LogWarning("Login attempt failed for non-existent user {Username}", model.Username);
                return Unauthorized(new { Message = "Invalid username or password." });
            }

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                _logger.LogWarning("Login attempt failed for user {Username} due to incorrect password.", model.Username);
                return Unauthorized(new { Message = "Invalid username or password." });
            }

            var lockoutCheck = await AuthHelper.CheckUserLockoutAsync(user, _userManager, _logger);

            if (lockoutCheck.IsLocked)
            {
                return Unauthorized(new
                {
                    Message = lockoutCheck.Message,
                    IsAdminLocked = lockoutCheck.IsAdminLocked
                });
            }

            var token = await _jwtTokenService.GenerateTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new { Token = token, Roles = roles });
        }


        [HttpPost("external-login")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginRequestDto model)
        {
            _logger.LogInformation("External login attempt for provider {Provider} with email {Email}", model.Provider, model.Email);

            var existingUser = await _userManager.FindByLoginAsync(model.Provider, model.ProviderKey);

            if (existingUser == null)
            {
                existingUser = await _userManager.FindByEmailAsync(model.Email);

                if (existingUser == null)
                {
                    var newUser = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FullName = model.FullName ?? $"{model.FirstName} {model.LastName}".Trim(),
                        EmailConfirmed = true,
                        IsActive = true
                    };

                    if (string.IsNullOrWhiteSpace(newUser.FullName)) newUser.FullName = model.Email;

                    var createUserResult = await _userManager.CreateAsync(newUser);
                    if (!createUserResult.Succeeded)
                    {
                        _logger.LogError("Failed to create new user via external login for email {Email}. Errors: {Errors}", 
                            model.Email, string.Join(", ", createUserResult.Errors.Select(e => e.Description)));

                        return BadRequest(new { Message = "User creation failed.", Errors = createUserResult.Errors });
                    }

                    var addLoginResult = await _userManager.AddLoginAsync(newUser,
                        new UserLoginInfo
                        (
                            model.Provider, 
                            model.ProviderKey, 
                            model.Provider)
                        );

                    if (!addLoginResult.Succeeded)
                    {
                        _logger.LogError("Failed to add external login for new user {Email}. Errors: {Errors}", 
                            model.Email, string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                    }

                    await _userManager.AddToRoleAsync(newUser, "User");

                    existingUser = newUser;
                    _logger.LogInformation("New user {Email} registered via external provider {Provider}.", model.Email, model.Provider);
                }
                else
                {
                    var addLoginResult = await _userManager.AddLoginAsync(existingUser, 
                        new UserLoginInfo
                        (
                            model.Provider, 
                            model.ProviderKey, 
                            model.Provider)
                        );

                    if (!addLoginResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to add external login for existing user {Email}. Errors: {Errors}", 
                            model.Email, string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                    }
                    _logger.LogInformation("External login {Provider} linked to existing user {Email}.", model.Provider, model.Email);
                }
            }


            if (!existingUser.IsActive)
            {
                _logger.LogWarning("External login attempt for deactivated user {Username}", existingUser.UserName);
                return Unauthorized(new { Message = "Your account has been deactivated." });
            }

            if (await _userManager.IsLockedOutAsync(existingUser))
            {
                _logger.LogWarning("External login attempt for locked out user {Username}", existingUser.UserName);

                bool isAdminLocked = existingUser.LockoutEnd.HasValue &&
                                     existingUser.LockoutEnd.Value >= DateTimeOffset.UtcNow.AddYears(100);

                string message = isAdminLocked
                    ? "Admin blocked your account. Please contact admin."
                    : "Your account is locked.";

                return Unauthorized(new
                {
                    Message = message,
                    IsAdminLocked = isAdminLocked
                });
            }

            var token = await _jwtTokenService.GenerateTokenAsync(existingUser);
            var roles = await _userManager.GetRolesAsync(existingUser);

            _logger.LogInformation("User {Username} logged in successfully via external provider {Provider}.", existingUser.UserName, model.Provider);
            return Ok(new { Token = token, Roles = roles });
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogInformation("Forgot password request for non-existent email: {Email}", model.Email);
                return Ok(new { Message = "If your email address is registered, you will receive a password reset link." });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var clientAppUrl = _configuration["ClientAppSettings:ResetPasswordUrl"];
            if (string.IsNullOrEmpty(clientAppUrl))
            {
                _logger.LogError("ClientAppSettings:ResetPasswordUrl is not configured in appsettings.json");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Server configuration error for password reset." });
            }

            var callbackUrl = $"{clientAppUrl}?token={encodedToken}&email={System.Net.WebUtility.UrlEncode(model.Email)}";

            var emailSubject = "Reset Your Password - EMS Portal";
            var emailMessage = $"Please reset your password by <a href='{
                                                                System.Text.Encodings.Web.HtmlEncoder.Default.Encode(callbackUrl)
                                                                }'>clicking here</a>. This link will expire shortly.";

            try
            {
                await _emailService.SendEmailAsync(model.Email, emailSubject, emailMessage);
                _logger.LogInformation("Password reset email sent to {Email}", model.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", model.Email);
            }

            return Ok(new { Message = "If your email address is registered, you will receive a password reset link." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning("Reset password attempt for non-existent email: {Email}", model.Email);
                return BadRequest(new { Message = "Password reset failed. Invalid request." });
            }

            var decodedToken = AuthHelper.GetDecodedResetToken(model.Token, _logger, model.Email);

            if (string.IsNullOrEmpty(decodedToken))
            {
                return BadRequest(new { Message = "Invalid token format." });
            }

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("Password reset successfully for user {Username}", user.UserName);
                return Ok(new { Message = "Your password has been reset successfully. Please login with your new password." });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            _logger.LogWarning("Password reset failed for user {Username}: {Errors}", 
                user.UserName, 
                string.Join(", ", result.Errors.Select(e => e.Description)));

            return BadRequest(ModelState);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _logger.LogInformation("User logout request received.");
            return Ok(new { Message = "User logged out successfully." });
        }
    }
}
