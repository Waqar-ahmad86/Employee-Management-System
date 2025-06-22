using EMS.Common.Enums;
using EMSMvc.Core.Application.DTOs;
using EMSMvc.Core.Application.Interfaces;
using EMSMvc.ViewModels;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace EMSMvc.Core.Application.Services
{
    public class LeaveService
    {
        private readonly IApiService _apiService;

        public LeaveService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<LeaveType>> GetActiveLeaveTypesAsync()
        {
            return await _apiService.SendRequestAsync<List<LeaveType>>(HttpMethod.Get, "LeaveTypes/active") ?? new List<LeaveType>();
        }

        public async Task<List<LeaveType>> GetAllLeaveTypesAdminAsync()
        {
            return await _apiService.SendRequestAsync<List<LeaveType>>(HttpMethod.Get, "LeaveTypes") ?? new List<LeaveType>();
        }

        public async Task<(bool Success, string Message, LeaveType? Data)> CreateLeaveTypeAsync(LeaveType model)
        {
            try
            {
                var payload = new { model.Name, model.Description, model.DefaultDaysAllowed, model.IsActive };
                var response = await _apiService.SendRequestAsync<LeaveType>(HttpMethod.Post, "LeaveTypes", payload);
                return (_apiService.LastResponseWasSuccessful, _apiService.LastResponseWasSuccessful ? "Leave Type created." : "Failed.", response);
            }
            catch (HttpRequestException ex) { return (false, _apiService.ExtractErrorMessageFromHttpException(ex), null); }
        }

        public async Task<(bool Success, string? Message, LeaveApplication? Data)> ApplyForLeaveAsync(ApplyLeaveVM model)
        {
            var payload = new
            {
                LeaveTypeId = model.SelectedLeaveTypeId,
                model.StartDate,
                model.EndDate,
                model.Reason
            };
            try
            {
                var response = await _apiService.SendRequestAsync<LeaveApplication>(HttpMethod.Post, "LeaveApplications", payload);

                if (_apiService.LastResponseWasSuccessful)
                {
                    return (true, null, response);
                }
                else
                {
                    return (false, "Leave application submission failed at the API.", response);
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex), null);
            }
            catch (JsonException ex)
            {
                return (false, $"Submission succeeded but response processing failed: {ex.Message}", null);
            }
            catch (Exception)
            {
                return (false, "An unexpected error occurred while applying for leave.", null);
            }
        }
        public async Task<List<LeaveApplication>> GetMyLeaveApplicationsAsync(DateTime? startDate, DateTime? endDate)
        {
            var endpoint = "LeaveApplications/my-applications";
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={UrlEncoder.Default.Encode(startDate.Value.ToString("yyyy-MM-dd"))}");
            if (endDate.HasValue) queryParams.Add($"endDate={UrlEncoder.Default.Encode(endDate.Value.ToString("yyyy-MM-dd"))}");

            if (queryParams.Any()) endpoint += "?" + string.Join("&", queryParams);

            return await _apiService.SendRequestAsync<List<LeaveApplication>>(HttpMethod.Get, endpoint) ?? new List<LeaveApplication>();
        }

        public async Task<LeaveApplication?> GetLeaveApplicationByIdAsync(int id)
        {
            return await _apiService.SendRequestAsync<LeaveApplication>(HttpMethod.Get, $"LeaveApplications/{id}");
        }

        public async Task<List<LeaveApplication>> GetAllPendingLeaveApplicationsAdminAsync()
        {
            return await _apiService.SendRequestAsync<List<LeaveApplication>>(HttpMethod.Get, "LeaveApplications/pending")
                ?? new List<LeaveApplication>();
        }

        public async Task<List<LeaveApplication>> GetAllLeaveApplicationsAdminAsync(
            string? applicantName,
            int? leaveTypeId,
            LeaveApplicationStatus? status,
            DateTime? SDate,
            DateTime? EDate)
        {
            var endpoint = "LeaveApplications/all";
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(applicantName)) queryParams.Add($"applicantName={UrlEncoder.Default.Encode(applicantName)}");
            if (leaveTypeId.HasValue) queryParams.Add($"leaveTypeId={leaveTypeId.Value}");
            if (status.HasValue) queryParams.Add($"status={(int)status.Value}");
            if (SDate.HasValue) queryParams.Add($"SDate={UrlEncoder.Default.Encode(SDate.Value.ToString("yyyy-MM-dd"))}");
            if (EDate.HasValue) queryParams.Add($"EDate={UrlEncoder.Default.Encode(EDate.Value.ToString("yyyy-MM-dd"))}");

            if (queryParams.Any()) endpoint += "?" + string.Join("&", queryParams);

            return await _apiService.SendRequestAsync<List<LeaveApplication>>(HttpMethod.Get, endpoint)
                ?? new List<LeaveApplication>();
        }


        public async Task<(bool Success, string? Message)> UpdateLeaveApplicationStatusAsync(
            int leaveApplicationId,
            LeaveApplicationStatus newStatus,
            string? adminRemarks)
        {
            var payload = new
            {
                NewStatus = newStatus,
                AdminRemarks = adminRemarks
            };
            try
            {
                bool apiCallSucceeded = await _apiService.SendRequestAsync(
                    HttpMethod.Put,
                    $"LeaveApplications/{leaveApplicationId}/status",
                    payload);

                if (apiCallSucceeded)
                {
                    return (true, null);
                }
                else
                {
                    return (false, "Failed to update leave status: API call was not successful.");
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex));
            }
            catch (Exception)
            {
                return (false, "An unexpected error occurred while updating leave status.");
            }
        }


        public async Task<LeaveType?> GetLeaveTypeByIdAdminAsync(int id)
        {
            return await _apiService.SendRequestAsync<LeaveType>(HttpMethod.Get, $"LeaveTypes/{id}");
        }

        public async Task<(bool Success, string Message)> UpdateLeaveTypeAsync(LeaveType model)
        {
            var payload = new
            {
                model.Id,
                model.Name,
                model.Description,
                model.DefaultDaysAllowed,
                model.IsActive
            };
            try
            {
                await _apiService.SendRequestAsync(HttpMethod.Put, $"LeaveTypes/{model.Id}", payload);
                return (_apiService.LastResponseWasSuccessful,
                       _apiService.LastResponseWasSuccessful ? "Leave Type updated successfully." : "Failed to update Leave Type.");
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex));
            }
        }

        public async Task<(bool Success, string Message)> DeleteLeaveTypeAsync(int id)
        {
            try
            {
                await _apiService.SendRequestAsync(HttpMethod.Delete, $"LeaveTypes/{id}");
                return (_apiService.LastResponseWasSuccessful,
                        _apiService.LastResponseWasSuccessful ? "Leave Type deleted successfully." : "Failed to delete Leave Type.");
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex));
            }
        }
    }
}
