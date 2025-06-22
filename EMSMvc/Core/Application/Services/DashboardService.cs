using EMSMvc.Core.Application.DTOs;
using EMSMvc.Core.Application.Interfaces;

namespace EMSMvc.Core.Application.Services
{
    public class DashboardService
    {
        private readonly IApiService _apiService;

        public DashboardService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<AdminDashboardOverview?> GetAdminDashboardOverviewAsync()
        {
            return await _apiService.SendRequestAsync<AdminDashboardOverview>(HttpMethod.Get, "Dashboard/admin-overview");
        }

        public async Task<SimpleChart?> GetDepartmentEmployeeCountsAsync()
        {
            return await _apiService.SendRequestAsync<SimpleChart>(HttpMethod.Get, "Dashboard/department-employee-counts");
        }

        public async Task<SimpleChart?> GetUserRoleDistributionAsync()
        {
            return await _apiService.SendRequestAsync<SimpleChart>(HttpMethod.Get, "Dashboard/user-role-distribution");
        }
    }
}
