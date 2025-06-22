using EMSMvc.Core.Application.DTOs;
using EMSMvc.Core.Application.Interfaces;

namespace EMSMvc.Core.Application.Services
{
    public class AuthService
    {
        private readonly IApiService _apiService;

        public AuthService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<(string? Token, string? ErrorMessage, bool IsAdminLocked)> LoginAsync(Login model)
        {
            try
            {
                var result = await _apiService.SendRequestAsync<LoginResponse>(HttpMethod.Post, "Auth/login", model);

                if (result != null && !string.IsNullOrEmpty(result.Token) && _apiService.LastResponseWasSuccessful)
                {
                    return (result.Token, null, false);
                }
                return (null, "Login failed: Invalid response from server.", false);
            }
            catch (HttpRequestException ex)
            {
                string rawErrorBody = _apiService.GetRawResponseBodyFromHttpException(ex);
                ApiErrorResponse? apiError = _apiService.TryParseApiError(rawErrorBody);

                if (apiError != null)
                {
                    return (null, apiError.Message, apiError.IsAdminLocked);
                }
                else
                {
                    string fallbackErrorMessage = _apiService.ExtractErrorMessageFromHttpException(ex);
                    return (null, fallbackErrorMessage, false);
                }
            }
            catch (Exception)
            {
                return (null, "An unexpected error occurred during login.", false);
            }
        }

        public async Task<(bool Success, string? Message, LoginResponse? LoginResponse, bool IsAdminLocked)> ExternalLoginApiAsync(ExternalLoginRequest model)
        {
            try
            {
                var response = await _apiService.SendRequestAsync<LoginResponse>(HttpMethod.Post, "Auth/external-login", model);
                if (_apiService.LastResponseWasSuccessful && response != null && !string.IsNullOrEmpty(response.Token))
                {
                    return (true, "Successfully processed external login.", response, false);
                }

                return (false, "External login processing failed at API: Unexpected successful response format.", null, false);
            }
            catch (HttpRequestException ex)
            {
                string rawErrorBody = _apiService.GetRawResponseBodyFromHttpException(ex);
                ApiErrorResponse? apiError = _apiService.TryParseApiError(rawErrorBody);
                if (apiError != null)
                {
                    return (false, apiError.Message, null, apiError.IsAdminLocked);
                }
                else
                {
                    return (false, _apiService.ExtractErrorMessageFromHttpException(ex), null, false);
                }
            }
            catch (Exception)
            {
                return (false, "An unexpected error occurred during external API login.", null, false);
            }
        }

        public async Task<string?> RegisterAsync(Register model)
        {
            try
            {
                await _apiService.SendRequestAsync(HttpMethod.Post, "Auth/register", model);
                return null;
            }
            catch (HttpRequestException ex)
            {
                return _apiService.ExtractErrorMessageFromHttpException(ex);
            }
            catch (Exception)
            {
                return "An unexpected registration error occurred.";
            }
        }

        public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPassword model)
        {
            try
            {
                var response = await _apiService.SendRequestAsync<ApiResponseWithMessage>(HttpMethod.Post, "Auth/forgot-password", model);
                if (response != null && _apiService.LastResponseWasSuccessful)
                {
                    return (true, response.Message);
                }
                return (false, response?.Message ?? "Failed to send password reset email.");
            }
            catch (HttpRequestException ex)
            {
                string apiErrorMessage = _apiService.ExtractErrorMessageFromHttpException(ex);
                return (false, string.IsNullOrEmpty(apiErrorMessage) ? "Error requesting password reset." : apiErrorMessage);
            }
            catch (Exception)
            {
                return (false, "An unexpected error occurred during forgot password request.");
            }
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPassword model)
        {
            try
            {
                var apiPayload = new
                {
                    model.Email,
                    model.Password,
                    model.ConfirmPassword,
                    model.Token
                };

                var response = await _apiService.SendRequestAsync<ApiResponseWithMessage>(HttpMethod.Post, "Auth/reset-password", apiPayload);

                if (response != null && _apiService.LastResponseWasSuccessful)
                {
                    return (true, response.Message ?? "Password reset successful.");
                }

                string errorMessage = response?.Message ?? "Password reset failed. The link may be invalid or expired.";
                return (false, errorMessage);
            }
            catch (HttpRequestException ex)
            {
                string apiErrorMessage = _apiService.ExtractErrorMessageFromHttpException(ex);
                return (false, string.IsNullOrWhiteSpace(apiErrorMessage)
                    ? "Password reset failed due to a communication error."
                    : apiErrorMessage);
            }
            catch
            {
                return (false, "An unexpected error occurred during password reset.");
            }
        }
    }
}