using EMSWebApi.Application.DTOs.Dashboard;
using EMSWebApi.Application.Interfaces.Persistence;
using EMSWebApi.Domain.Identity;
using EMSWebApi.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMSWebApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public DashboardController(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _context = context;
        }

        [HttpGet("admin-overview")]
        public async Task<ActionResult<AdminDashboardOverviewDto>> GetAdminOverview()
        {
            var today = DateTime.UtcNow.Date;

            var overview = new AdminDashboardOverviewDto
            {
                TotalActiveUsers = await _userManager.Users.CountAsync(u => u.IsActive),
                TotalEmployees = await _unitOfWork.Employees.CountAsync(),
                UsersCheckedInToday = await _context.AttendanceRecords.CountAsync(ar => ar.Date == today && ar.CheckInTime.HasValue)
            };
            return Ok(overview);
        }

        [HttpGet("department-employee-counts")]
        public async Task<ActionResult<SimpleChartDto>> GetDepartmentEmployeeCounts()
        {
            var departmentCounts = await _context.Employees
                .Where(e => !string.IsNullOrEmpty(e.Department))
                .GroupBy(e => e.Department)
                .Select(g => new { DepartmentName = g.Key, Count = g.Count() })
                .OrderBy(x => x.DepartmentName)
                .ToListAsync();

            var chartData = new SimpleChartDto();
            foreach (var item in departmentCounts)
            {
                chartData.Labels.Add(item.DepartmentName ?? "Undefined");
                chartData.Values.Add(item.Count);
            }

            return Ok(chartData);
        }

        [HttpGet("user-role-distribution")]
        public async Task<ActionResult<SimpleChartDto>> GetUserRoleDistribution()
        {
            var roles = await _context.Roles.Select(r => r.Name).ToListAsync();
            var userRolesCounts = new Dictionary<string, int>();

            foreach (var roleName in roles)
            {
                if (!string.IsNullOrEmpty(roleName))
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
                    userRolesCounts[roleName] = usersInRole.Count;
                }
            }

            var chartData = new SimpleChartDto();
            foreach (var kvp in userRolesCounts.OrderBy(x => x.Key))
            {
                chartData.Labels.Add(kvp.Key);
                chartData.Values.Add(kvp.Value);
            }

            return Ok(chartData);
        }
    }
}
