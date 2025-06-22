using EMSMvc.Core.Application.DTOs;
using EMSMvc.Core.Application.Interfaces;
using EMSMvc.Infrastructure.Config;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EMSMvc.Infrastructure.Implementations
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _baseUrl;
        public bool LastResponseWasSuccessful { get; private set; }

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _baseUrl = apiSettings.Value.BaseUrl;
        }

        private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string endpoint, object? body = null)
        {
            LastResponseWasSuccessful = false;
            var requestUri = $"{_baseUrl}{endpoint}";
            var request = new HttpRequestMessage(method, requestUri);

            var publicApiEndpoints = new List<string>
            {
                "Auth/register",
                "Auth/login",
                "Auth/forgot-password",
                "Auth/reset-password",
                "Auth/external-login"
            };

            var normalizedEndpoint = endpoint.Trim('/');
            bool isPublic = publicApiEndpoints.Any(publicEp =>
                normalizedEndpoint.Equals(publicEp.Trim('/'), StringComparison.OrdinalIgnoreCase));

            if (!isPublic)
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException($"User is not authenticated or session expired for API endpoint: '{endpoint}'.");
                }
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (body != null)
            {
                try
                {
                    request.Content = new StringContent(JsonSerializer.Serialize(
                        body,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                        Encoding.UTF8, "application/json");
                }
                catch (JsonException jsonEx)
                {
                    throw new ApplicationException($"Failed to serialize request body for endpoint {endpoint}.", jsonEx);
                }
            }
            return request;
        }

        public async Task<T?> SendRequestAsync<T>(HttpMethod method, string endpoint, object? body = null)
        {
            var request = await CreateRequestAsync(method, endpoint, body);
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException httpEx)
            {
                LastResponseWasSuccessful = false;
                throw new HttpRequestException($"Network error while calling API endpoint {endpoint}: {httpEx.Message}", httpEx);
            }

            LastResponseWasSuccessful = response.IsSuccessStatusCode;
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"API request to {endpoint} failed with status {response.StatusCode}. Response: {responseContent}");
            }

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Failed to deserialize API response from {endpoint} to type {typeof(T).Name}. Content: {responseContent}", ex);
            }
        }

        public async Task<string?> SendRequestAndGetResponseStringAsync(HttpMethod method, string endpoint, object? body = null)
        {
            var request = await CreateRequestAsync(method, endpoint, body);
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException httpEx)
            {
                LastResponseWasSuccessful = false;
                throw new HttpRequestException($"Network error while calling API endpoint {endpoint}: {httpEx.Message}", httpEx);
            }

            LastResponseWasSuccessful = response.IsSuccessStatusCode;
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"API request to {endpoint} failed with status {response.StatusCode}. Response: {responseContent}");
            }
            return responseContent;
        }

        public async Task<bool> SendRequestAsync(HttpMethod method, string endpoint, object? body = null)
        {
            var request = await CreateRequestAsync(method, endpoint, body);
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request);
                LastResponseWasSuccessful = response.IsSuccessStatusCode;

                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"API request to {endpoint} failed with status {response.StatusCode}. Response: {responseContent}");
                }
                return true;
            }
            catch (HttpRequestException)
            {
                LastResponseWasSuccessful = false;
                throw;
            }
        }

        public Task LogoutAsync()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _ = SendRequestAsync(HttpMethod.Post, "Auth/logout");
            }
            _httpContextAccessor.HttpContext?.Session.Clear();
            return Task.CompletedTask;
        }

        public ApiErrorResponse? TryParseApiError(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody)) return null;

            try
            {
                return JsonSerializer.Deserialize<ApiErrorResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public string ExtractErrorMessageFromHttpException(HttpRequestException ex)
        {
            string responseBody = GetRawResponseBodyFromHttpException(ex);

            if (string.IsNullOrWhiteSpace(responseBody))
                return "An API error occurred, and no detailed message was available.";

            var apiError = TryParseApiError(responseBody);
            if (apiError?.Message is string msg && !string.IsNullOrWhiteSpace(msg))
            {
                return msg;
            }

            try
            {
                JsonDocument doc = JsonDocument.Parse(responseBody);
                JsonElement root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    var identityErrors = new List<string>();
                    foreach (JsonElement item in root.EnumerateArray())
                    {
                        if (item.TryGetProperty("description", out JsonElement descElement) &&
                            descElement.ValueKind == JsonValueKind.String &&
                            !string.IsNullOrEmpty(descElement.GetString()))
                        {
                            identityErrors.Add(descElement.GetString()!);
                        }
                    }
                    if (identityErrors.Any()) return string.Join(" ", identityErrors);
                }

                if (root.TryGetProperty("title", out JsonElement titleElement) &&
                    titleElement.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrWhiteSpace(titleElement.GetString()))
                {
                    return titleElement.GetString()!;
                }

                if (root.TryGetProperty("message", out JsonElement messageElement) &&
                    messageElement.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrWhiteSpace(messageElement.GetString()))
                {
                    return messageElement.GetString()!;
                }

                if (root.TryGetProperty("errors", out JsonElement errorsElement) &&
                    errorsElement.ValueKind == JsonValueKind.Object)
                {
                    var errorMessages = new List<string>();
                    foreach (JsonProperty property in errorsElement.EnumerateObject())
                    {
                        foreach (JsonElement error in property.Value.EnumerateArray())
                        {
                            if (error.ValueKind == JsonValueKind.String &&
                                !string.IsNullOrWhiteSpace(error.GetString()))
                            {
                                errorMessages.Add(error.GetString()!);
                            }
                        }
                    }
                    if (errorMessages.Any()) return string.Join(" ", errorMessages);
                }
            }
            catch (JsonException)
            {
                return responseBody.Length > 200 ? "An API error occurred (Non-JSON response)." : responseBody;
            }
            catch
            {
                return "Could not parse the error response from the API.";
            }
            return responseBody.Length > 200 ? "An API error occurred." : responseBody;
        }

        public string GetRawResponseBodyFromHttpException(HttpRequestException ex)
        {
            string responsePrefix = "Response: ";
            int responseStartIndex = ex.Message.IndexOf(responsePrefix);
            if (responseStartIndex != -1)
            {
                return ex.Message.Substring(responseStartIndex + responsePrefix.Length);
            }
            return string.Empty;
        }
    }
}