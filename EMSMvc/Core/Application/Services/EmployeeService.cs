using EMSMvc.Core.Application.DTOs;
using EMSMvc.Core.Application.Interfaces;
using System.Text.Encodings.Web;

namespace EMSMvc.Core.Application.Services
{
    public class EmployeeService
    {
        private readonly IApiService _apiService;

        public EmployeeService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<Employee>> GetEmployeesAsync(
            string? name = null,
            string? department = null,
            string? jobTitle = null)
        {
            var endpoint = "Employee/GetAll";
            var queryParams = new Dictionary<string, string?>();

            if (!string.IsNullOrWhiteSpace(name))
            {
                queryParams["name"] = name;
            }
            if (!string.IsNullOrWhiteSpace(department))
            {
                queryParams["department"] = department;
            }
            if (!string.IsNullOrWhiteSpace(jobTitle))
            {
                queryParams["jobTitle"] = jobTitle;
            }

            if (queryParams.Any())
            {
                var queryString = string.Join("&", queryParams
                    .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                    .Select(kvp => $"{UrlEncoder.Default.Encode(kvp.Key)}={UrlEncoder.Default.Encode(kvp.Value!)}"));
                endpoint += $"?{queryString}";
            }

            return await _apiService.SendRequestAsync<List<Employee>>(HttpMethod.Get, endpoint) ?? new List<Employee>();
        }

        public async Task<Employee?> GetEmployeeAsync(int id)
        {
            return await _apiService.SendRequestAsync<Employee>(HttpMethod.Get, $"Employee/GetById/{id}");
        }


        public async Task<bool> CreateEmployeeAsync(Employee employee)
        {
            return await _apiService.SendRequestAsync(HttpMethod.Post, "Employee/Add", employee);
        }

        public async Task<bool> UpdateEmployeeAsync(int id, Employee employee)
        {
            return await _apiService.SendRequestAsync(HttpMethod.Put, $"Employee/Update/{id}", employee);
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            return await _apiService.SendRequestAsync(HttpMethod.Delete, $"Employee/Delete/{id}");
        }

        public async Task<(bool Success, string? Message)> DeleteEmployeesAsync(List<int> employeeIds)
        {
            if (employeeIds == null || !employeeIds.Any())
            {
                return (false, "No employee IDs provided for deletion.");
            }

            var payload = new { Ids = employeeIds };
            try
            {
                var apiResponse = await _apiService.SendRequestAsync<ApiResponseWithMessage>(HttpMethod.Post, "Employee/DeleteMultiple", payload);

                if (_apiService.LastResponseWasSuccessful && apiResponse != null)
                {
                    return (true, apiResponse.Message ?? "Selected employees deleted successfully.");
                }
                else if (apiResponse != null && !string.IsNullOrWhiteSpace(apiResponse.Message))
                {
                    return (false, apiResponse.Message);
                }
                return (false, "Failed to delete employees. API did not respond successfully.");
            }
            catch (HttpRequestException ex)
            {
                return (false, _apiService.ExtractErrorMessageFromHttpException(ex));
            }
            catch (Exception)
            {
                return (false, "An unexpected error occurred while trying to delete employees.");
            }
        }
    }
}