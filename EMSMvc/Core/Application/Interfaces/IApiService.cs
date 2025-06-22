using EMSMvc.Core.Application.DTOs;

namespace EMSMvc.Core.Application.Interfaces
{
    public interface IApiService
    {
        bool LastResponseWasSuccessful { get; }

        Task<T?> SendRequestAsync<T>(HttpMethod method, string endpoint, object? body = null);
        Task<string?> SendRequestAndGetResponseStringAsync(HttpMethod method, string endpoint, object? body = null);
        Task<bool> SendRequestAsync(HttpMethod method, string endpoint, object? body = null);
        Task LogoutAsync();
        ApiErrorResponse? TryParseApiError(string responseBody);
        string ExtractErrorMessageFromHttpException(HttpRequestException ex);
        string GetRawResponseBodyFromHttpException(HttpRequestException ex);
    }
}
