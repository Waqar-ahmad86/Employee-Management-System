using EMSMvc.Core.Application.Interfaces;
using System.Text.Encodings.Web;
using System.Text.Json;
using EMSMvc.Core.Application.DTOs;

namespace EMSMvc.Core.Application.Services
{
    public class AttendanceService
    {
        private readonly IApiService _apiService;

        public AttendanceService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<AttendanceRecord?> GetMyTodaysAttendanceAsync()
        {
            return await _apiService.SendRequestAsync<AttendanceRecord>(HttpMethod.Get, "Attendance/my-today");
        }

        public async Task<(bool Success, string? Message, AttendanceRecord? Record)> CheckInAsync(string? remarks)
        {
            var payload = new { Remarks = remarks };
            try
            {
                var response = await _apiService.SendRequestAsync<AttendanceRecord>(HttpMethod.Post, "Attendance/check-in", payload);

                if (_apiService.LastResponseWasSuccessful)
                {
                    return (true, null, response);
                }
                else
                {
                    return (false, "Check-in failed: Could not connect to the service or an unknown error occurred.", response);
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex), null);
            }
            catch (JsonException ex)
            {
                return (false, $"Check-in succeeded but failed to process the response: {ex.Message}", null);
            }
            catch (Exception)
            {
                return (false, "An unexpected error occurred during check-in.", null);
            }
        }

        public async Task<(bool Success, string? Message, AttendanceRecord? Record)> CheckOutAsync(string? remarks)
        {
            var payload = new { Remarks = remarks };
            try
            {
                var response = await _apiService.SendRequestAsync<AttendanceRecord>(HttpMethod.Post, "Attendance/check-out", payload);

                if (_apiService.LastResponseWasSuccessful)
                {
                    return (true, null, response);
                }
                else
                {
                    return (false, "Check-out failed: Could not connect to the service or an unknown error occurred.", response);
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex), null);
            }
            catch (JsonException ex)
            {
                return (false, $"Check-out succeeded but failed to process the response: {ex.Message}", null);
            }
            catch (Exception)
            {
                return (false, "An unexpected error occurred during check-out.", null);
            }
        }

        public async Task<List<AttendanceRecord>> GetMyAttendanceHistoryAsync(DateTime startDate, DateTime endDate)
        {
            string start = startDate.ToString("yyyy-MM-dd");
            string end = endDate.ToString("yyyy-MM-dd");
            var endpoint = $"Attendance/my-history?startDate={UrlEncoder.Default.Encode(start)}&endDate={UrlEncoder.Default.Encode(end)}";
            return await _apiService.SendRequestAsync<List<AttendanceRecord>>(HttpMethod.Get, endpoint) ??
                new List<AttendanceRecord>();
        }

        public async Task<List<AttendanceRecord>> GetAllAttendanceAsync(
            DateTime startDate,
            DateTime endDate,
            string? userName = null,
            string? roleName = null)
        {
            string start = startDate.ToString("yyyy-MM-dd");
            string end = endDate.ToString("yyyy-MM-dd");
            var endpoint = $"Attendance/all?startDate={UrlEncoder.Default.Encode(start)}&endDate={UrlEncoder.Default.Encode(end)}";

            if (!string.IsNullOrEmpty(userName))
            {
                endpoint += $"&userName={UrlEncoder.Default.Encode(userName)}";
            }

            if (!string.IsNullOrEmpty(roleName))
            {
                endpoint += $"&roleName={UrlEncoder.Default.Encode(roleName)}";
            }

            return await _apiService.SendRequestAsync<List<AttendanceRecord>>(HttpMethod.Get, endpoint) ??
                new List<AttendanceRecord>();
        }

        public async Task<List<MonthlyAttendanceReportItem>> GetMonthlyAttendanceReportAsync(
            int year,
            int month,
            string? userName,
            string? roleName)
        {
            var apiPayload = new
            {
                Year = year,
                Month = month,
                UserName = userName,
                RoleName = roleName
            };
            var response = await _apiService.SendRequestAsync<List<MonthlyAttendanceReportItem>>(
                HttpMethod.Post,
                "Attendance/monthly-report",
                apiPayload);
            return response ?? new List<MonthlyAttendanceReportItem>();
        }
    }
}
