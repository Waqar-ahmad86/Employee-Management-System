using EMSMvc.Core.Application.DTOs;
using EMSMvc.Core.Application.Interfaces;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace EMSMvc.Core.Application.Services
{
    public class UserManagementService
    {
        private readonly IApiService _apiService;

        public UserManagementService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<User>> GetUsersAsync(string? searchTerm = null, string? role = null)
        {
            var endpoint = "UserManagement/users";
            var queryParams = new Dictionary<string, string?>();

            if (!string.IsNullOrWhiteSpace(searchTerm)) queryParams["searchTerm"] = searchTerm;
            if (!string.IsNullOrWhiteSpace(role)) queryParams["role"] = role;

            var queryString = string.Join("&", queryParams
                .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                .Select(kvp => $"{UrlEncoder.Default.Encode(kvp.Key)}={UrlEncoder.Default.Encode(kvp.Value!)}"));

            if (!string.IsNullOrWhiteSpace(queryString))
            {
                endpoint += $"?{queryString}";
            }

            return await _apiService.SendRequestAsync<List<User>>(HttpMethod.Get, endpoint) ?? new List<User>();
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _apiService.SendRequestAsync<User>(HttpMethod.Get, $"UserManagement/user/{userId}");
        }

        public async Task<List<string>> GetAllRolesAsync()
        {
            return await _apiService.SendRequestAsync<List<string>>(HttpMethod.Get, "UserManagement/roles")
                ?? new List<string>();
        }

        public async Task<(bool Success, string Message)> UpdateUserRolesAsync(string userId, List<string> roles)
        {
            var payload = new { UserId = userId, Roles = roles };
            try
            {
                var response = await _apiService.SendRequestAsync<ApiResponseWithMessage>(HttpMethod.Put, "UserManagement/update-user-roles", payload);
                return (_apiService.LastResponseWasSuccessful, response?.Message ?? (_apiService.LastResponseWasSuccessful ? "Roles updated successfully." : "Failed to update roles."));
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex));
            }
        }

        public async Task<(bool Success, string Message)> UpdateUserDetailsAsync(EditUserDetails model)
        {
            var apiPayload = new
            {
                model.Id,
                model.FullName,
                model.IsActive
            };

            try
            {
                var response = await _apiService.SendRequestAsync<ApiResponseWithMessage>(HttpMethod.Put, "UserManagement/user", apiPayload);
                return (_apiService.LastResponseWasSuccessful, response?.Message ?? (_apiService.LastResponseWasSuccessful ? "User details updated successfully." : "Failed to update user details."));
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex));
            }
        }

        public async Task<(bool Success, string Message)> LockUserAsync(string userId)
        {
            try
            {
                var response = await _apiService.SendRequestAsync<ApiResponseWithMessage>(HttpMethod.Post, $"UserManagement/lock-user/{userId}");
                return (_apiService.LastResponseWasSuccessful, response?.Message ?? (_apiService.LastResponseWasSuccessful ? "User locked successfully." : "Operation failed."));
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex));
            }
        }

        public async Task<(bool Success, string Message)> UnlockUserAsync(string userId)
        {
            try
            {
                var response = await _apiService.SendRequestAsync<ApiResponseWithMessage>(HttpMethod.Post, $"UserManagement/unlock-user/{userId}");
                return (_apiService.LastResponseWasSuccessful, response?.Message ?? (_apiService.LastResponseWasSuccessful ? "User unlocked successfully." : "Operation failed."));
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex));
            }
        }

        public async Task<(bool Success, string Message, User? CreatedUser)> CreateUserByAdminAsync(AdminCreate model)
        {
            var apiPayload = new
            {
                model.UserName,
                model.Email,
                model.Password,
                model.FullName,
                InitialRoles = model.SelectedRoles,
                model.IsActive
            };
            try
            {
                var responseString = await _apiService.SendRequestAndGetResponseStringAsync(HttpMethod.Post, "UserManagement/create-user", apiPayload);

                if (_apiService.LastResponseWasSuccessful && !string.IsNullOrEmpty(responseString))
                {
                    using (JsonDocument doc = JsonDocument.Parse(responseString))
                    {
                        JsonElement root = doc.RootElement;
                        string message = "User created successfully.";

                        if (root.TryGetProperty("message", out JsonElement messageElement))
                        {
                            message = messageElement.GetString() ?? message;
                        }

                        User? createdUser = null;
                        if (root.TryGetProperty("user", out JsonElement userElement) && userElement.ValueKind == JsonValueKind.Object)
                        {
                            createdUser = JsonSerializer.Deserialize<User>(
                                userElement.GetRawText(),
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                            );
                        }
                        return (true, message, createdUser);
                    }
                }
                else if (!string.IsNullOrEmpty(responseString))
                {
                    string extractedErrorMessage = "Failed to create user.";
                    try
                    {
                        using (JsonDocument errorDoc = JsonDocument.Parse(responseString))
                        {
                            JsonElement errorRoot = errorDoc.RootElement;
                            if (errorRoot.TryGetProperty("message", out JsonElement msgEl))
                            {
                                extractedErrorMessage = msgEl.GetString() ?? extractedErrorMessage;
                            }

                            else if (errorRoot.ValueKind == JsonValueKind.Array)
                            {
                                var identityErrors = new List<string>();
                                foreach (JsonElement item in errorRoot.EnumerateArray())
                                {
                                    if (item.TryGetProperty("description", out JsonElement descElement) && descElement.ValueKind == JsonValueKind.String)
                                    {
                                        var description = descElement.GetString();
                                        if (!string.IsNullOrWhiteSpace(description))
                                        {
                                            identityErrors.Add(description);
                                        }
                                    }
                                }
                                if (identityErrors.Any()) extractedErrorMessage = string.Join(" ", identityErrors);
                            }
                        }
                    }
                    catch (JsonException) { }

                    return (false, extractedErrorMessage, null);
                }
                return (false, "Failed to create user. No response from server or an unknown error occurred.", null);
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex), null);
            }
            catch (JsonException jsonEx)
            {
                return (false, $"Error parsing server response: {jsonEx.Message}", null);
            }
            catch (Exception ex)
            {
                return (false, $"An unexpected error occurred: {ex.Message}", null);
            }
        }


        public async Task<(bool Success, string Message)> DeleteUserAsync(string userId)
        {
            try
            {
                var response = await _apiService.SendRequestAsync<ApiResponseWithMessage>(HttpMethod.Delete, $"UserManagement/user/{userId}");
                return (_apiService.LastResponseWasSuccessful,
                        response?.Message ?? (_apiService.LastResponseWasSuccessful ? "User deleted successfully." : "Failed to delete user."));
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex));
            }
            catch (Exception ex)
            {
                return (false, $"An unexpected error occurred: {ex.Message}");
            }
        }


        public async Task<(bool Success, string Message, DeleteMultipleUsersApiResponse? Details)> DeleteMultipleUsersAsync(List<string> userIds)
        {
            if (userIds == null || !userIds.Any())
            {
                return (false, "No user IDs provided for deletion.", null);
            }

            var payload = new { Ids = userIds };
            try
            {
                var apiResponse = await _apiService.SendRequestAsync<DeleteMultipleUsersApiResponse>(HttpMethod.Post, "UserManagement/delete-multiple-users", payload);

                if (_apiService.LastResponseWasSuccessful && apiResponse != null)
                {
                    return (true, apiResponse.Message ?? "Processed multiple user deletion request.", apiResponse);
                }
                else if (apiResponse != null && !string.IsNullOrWhiteSpace(apiResponse.Message))
                {
                    return (false, apiResponse.Message, apiResponse);
                }
                return (false, "Failed to delete users. API did not respond as expected.", null);
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex), null);
            }
            catch (Exception ex)
            {
                return (false, $"An unexpected error occurred: {ex.Message}", null);
            }
        }
    }
}