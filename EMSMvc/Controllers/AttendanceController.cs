using EMSMvc.Core.Application.Interfaces;
using EMSMvc.Core.Application.Services;
using EMSMvc.Helpers;
using EMSMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EMSMvc.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly AttendanceService _attendanceService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPdfService _pdfService;
        private readonly UserManagementService _userManagementService;

        public AttendanceController(
            AttendanceService attendanceService,
            IHttpContextAccessor httpContextAccessor,
            UserManagementService userManagementService,
            IPdfService pdfService)
        {
            _attendanceService = attendanceService;
            _httpContextAccessor = httpContextAccessor;
            _userManagementService = userManagementService;
            _pdfService = pdfService;
        }

        [HttpGet]
        public async Task<IActionResult> MyAttendance()
        {
            if (!SessionHelper.IsUserLoggedIn(_httpContextAccessor.HttpContext))
                return RedirectToAction("Login", "Auth");

            var todaysRecord = await _attendanceService.GetMyTodaysAttendanceAsync();
            var viewModel = new AttendanceActionVM
            {
                TodaysRecord = todaysRecord
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(AttendanceActionVM model)
        {
            if (!SessionHelper.IsUserLoggedIn(_httpContextAccessor.HttpContext))
                return RedirectToAction("Login", "Auth");

            var (success, message, record) = await _attendanceService.CheckInAsync(model.Remarks);

            if (success)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    TempData["SuccessMessage"] = message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = message ?? "An unknown error occurred during check-in.";
            }
            return RedirectToAction(nameof(MyAttendance));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(AttendanceActionVM model)
        {
            if (!SessionHelper.IsUserLoggedIn(_httpContextAccessor.HttpContext))
                return RedirectToAction("Login", "Auth");

            var (success, message, record) = await _attendanceService.CheckOutAsync(model.Remarks);

            if (success)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    TempData["SuccessMessage"] = message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = message ?? "An unknown error occurred during check-out.";
            }
            return RedirectToAction(nameof(MyAttendance));
        }

        [HttpGet]
        public async Task<IActionResult> MyHistory(DateTime? startDate, DateTime? endDate)
        {
            if (!SessionHelper.IsUserLoggedIn(_httpContextAccessor.HttpContext))
                return RedirectToAction("Login", "Auth");

            var sDate = startDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var eDate = endDate ?? DateTime.Today;

            ViewBag.StartDate = sDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = eDate.ToString("yyyy-MM-dd");

            var history = await _attendanceService.GetMyAttendanceHistoryAsync(sDate, eDate);
            return View(history);
        }

        [HttpGet]
        public async Task<IActionResult> AllAttendanceReport(
            DateTime? startDate, 
            DateTime? endDate, 
            string? Name,
            string? roleName)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext))
                return Forbid();

            var sDate = startDate ?? DateTime.Today.AddDays(-7);
            var eDate = endDate ?? DateTime.Today;

            ViewBag.StartDate = sDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = eDate.ToString("yyyy-MM-dd");
            ViewBag.SelectedName = Name;
            ViewBag.SelectedRoleName = roleName;
            ViewBag.RoleList = await DropdownHelper.GetRoleDropdownItemsAsync(_userManagementService, roleName);

            var reportData = await _attendanceService.GetAllAttendanceAsync(sDate, eDate, Name, roleName);
            return View(reportData);
        }

        [HttpGet]
        public async Task<IActionResult> MonthlyReport()
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext))
                return Forbid();

            var viewModel = new MonthlyReportRequestVM();
            DropdownHelper.PopulateYearMonthDropdowns(viewModel);

            var allRolesFromService = await _userManagementService.GetAllRolesAsync() ?? new List<string>();
            viewModel.Roles = allRolesFromService.Select(r => new SelectListItem
            {
                Value = r,
                Text = r
            }).ToList();


            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MonthlyReport(MonthlyReportRequestVM model)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext))
                return Forbid();

            if (!ModelState.IsValid)
            {
                DropdownHelper.PopulateYearMonthDropdowns(model);
                var allRolesFromService = await _userManagementService.GetAllRolesAsync() ?? new List<string>();
                model.Roles = allRolesFromService.Select(r => new SelectListItem
                {
                    Value = r,
                    Text = r,
                    Selected = r == model.RoleName
                }).ToList();
                return View(model);
            }

            model.ReportData = await _attendanceService.GetMonthlyAttendanceReportAsync(
                model.Year, model.Month, model.Name, model.RoleName);

            DropdownHelper.PopulateYearMonthDropdowns(model);

            var rolesAfterProcess = await _userManagementService.GetAllRolesAsync() ?? new List<string>();
            model.Roles = rolesAfterProcess.Select(r => new SelectListItem
            {
                Value = r,
                Text = r,
                Selected = r == model.RoleName
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadMonthlyReportPdf(int year, int month, string? employeeName, string? roleName)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext))
                return Forbid();

            if (year == 0 || month == 0)
            {
                TempData["ErrorMessage"] = "Year and Month are required for PDF report.";
                return RedirectToAction(nameof(MonthlyReport));
            }

            var reportData = await _attendanceService.GetMonthlyAttendanceReportAsync(year, month, employeeName, roleName);
            var viewModel = new MonthlyReportRequestVM
            {
                Year = year,
                Month = month,
                Name = employeeName,
                RoleName = roleName,
                ReportData = reportData
            };

            byte[] pdfBytes = _pdfService.GenerateMonthlyAttendanceReportPdf(viewModel);

            string fileName = FileNameHelper.GenerateMonthlyAttendanceReportFileName(year, month, employeeName, roleName);

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
