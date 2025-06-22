using EMSMvc.Core.Application.DTOs;
using EMSMvc.Core.Application.Interfaces;
using EMSMvc.ViewModels;

namespace EMSMvc.Core.Application.Services
{
    public class NoticeService
    {
        private readonly IApiService _apiService;

        public NoticeService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<Notice>> GetActiveNoticesForCurrentUserAsync()
        {
            return await _apiService.SendRequestAsync<List<Notice>>(HttpMethod.Get, "Notices") ?? new List<Notice>();
        }

        public async Task<List<Notice>> GetAllNoticesAdminAsync(bool activeOnly = false)
        {
            return await _apiService.SendRequestAsync<List<Notice>>(HttpMethod.Get, $"Notices/all-admin?activeOnly={activeOnly}") ?? new List<Notice>();
        }

        public async Task<Notice?> GetNoticeByIdAdminAsync(Guid id)
        {
            return await _apiService.SendRequestAsync<Notice>(HttpMethod.Get, $"Notices/{id}");
        }

        public async Task<(bool Success, string Message, Notice? Data)> CreateNoticeAsync(CreateNoticeVM model)
        {
            var payload = new
            {
                model.Title,
                model.Content,
                model.ExpiresAt,
                model.Audience,
                model.IsActive
            };
            try
            {
                var response = await _apiService.SendRequestAsync<Notice>(HttpMethod.Post, "Notices", payload);
                return (_apiService.LastResponseWasSuccessful,
                        _apiService.LastResponseWasSuccessful ? "Notice created successfully." : "Failed to create notice.",
                        response);
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex), null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateNoticeAsync(EditNoticeVM model)
        {
            var payload = new
            {
                model.Id,
                model.Title,
                model.Content,
                model.ExpiresAt,
                model.Audience,
                model.IsActive
            };
            try
            {
                await _apiService.SendRequestAsync(HttpMethod.Put, $"Notices/{model.Id}", payload);
                return (_apiService.LastResponseWasSuccessful,
                       _apiService.LastResponseWasSuccessful ? "Notice updated successfully." : "Failed to update notice.");
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex));
            }
        }

        public async Task<(bool Success, string Message)> DeleteNoticeAsync(Guid id)
        {
            try
            {
                await _apiService.SendRequestAsync(HttpMethod.Delete, $"Notices/{id}");
                return (_apiService.LastResponseWasSuccessful,
                        _apiService.LastResponseWasSuccessful ? "Notice deleted successfully." : "Failed to delete notice.");
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex));
            }
        }
    }
}
