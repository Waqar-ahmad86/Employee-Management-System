using EMSMvc.Core.Application.DTOs;
using EMSMvc.Core.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EMSMvc.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DashboardService _dashboardService;

        public DashboardController(IHttpContextAccessor httpContextAccessor, DashboardService dashboardService)
        {
            _httpContextAccessor = httpContextAccessor;
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(_httpContextAccessor.HttpContext?.Session.GetString("JwtToken")))
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.FullName = _httpContextAccessor.HttpContext?.Session.GetString("FullName");
            ViewBag.UserRole = _httpContextAccessor.HttpContext?.Session.GetString("UserRole");

            AdminDashboardOverview? overviewData = null;
            SimpleChart? departmentChartData = null;
            SimpleChart? userRoleChartData = null;

            if (ViewBag.UserRole == "Admin")
            {
                overviewData = await _dashboardService.GetAdminDashboardOverviewAsync();
                departmentChartData = await _dashboardService.GetDepartmentEmployeeCountsAsync();
                userRoleChartData = await _dashboardService.GetUserRoleDistributionAsync();
            }

            ViewBag.OverviewData = overviewData;
            ViewBag.DepartmentChartData = departmentChartData;
            ViewBag.UserRoleChartData = userRoleChartData;

            return View();
        }
    }
}
