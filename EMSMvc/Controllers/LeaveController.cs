using EMS.Common.Enums;
using EMSMvc.Core.Application.Services;
using EMSMvc.Helpers;
using EMSMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EMSMvc.Controllers
{
    public class LeaveController : Controller
    {
        private readonly LeaveService _leaveService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LeaveController(LeaveService leaveService, IHttpContextAccessor httpContextAccessor)
        {
            _leaveService = leaveService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<IActionResult> Apply()
        {
            if (!SessionHelper.IsUserLoggedIn(HttpContext)) return RedirectToAction("Login", "Auth");

            var activeLeaveTypes = await _leaveService.GetActiveLeaveTypesAsync();
            var viewModel = new ApplyLeaveVM
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                AvailableLeaveTypes = activeLeaveTypes.Select(lt => new SelectListItem
                {
                    Value = lt.Id.ToString(),
                    Text = lt.Name + (lt.DefaultDaysAllowed.HasValue ? $" (Max: {lt.DefaultDaysAllowed} days)" : "")
                }).ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(ApplyLeaveVM model)
        {
            if (!SessionHelper.IsUserLoggedIn(HttpContext)) return RedirectToAction("Login", "Auth");

            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date cannot be before start date.");
            }

            if (model.StartDate < DateTime.Today)
            {
                ModelState.AddModelError("StartDate", "Start date cannot be in the past.");
            }

            if (ModelState.IsValid)
            {
                var (success, message, data) = await _leaveService.ApplyForLeaveAsync(model);
                if (success)
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                        TempData["SuccessMessage"] = message;
                    }
                    return RedirectToAction(nameof(MyApplications));
                }
                TempData["ErrorMessage"] = message ?? "Failed to submit leave application. Please try again.";
            }

            var activeLeaveTypes = await _leaveService.GetActiveLeaveTypesAsync();
            model.AvailableLeaveTypes = activeLeaveTypes.Select(lt => new SelectListItem
            {
                Value = lt.Id.ToString(),
                Text = lt.Name + (lt.DefaultDaysAllowed.HasValue ? $" (Max: {lt.DefaultDaysAllowed} days)" : ""),
                Selected = lt.Id == model.SelectedLeaveTypeId
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> MyApplications(DateTime? startDate, DateTime? endDate)
        {
            if (!SessionHelper.IsUserLoggedIn(HttpContext)) return RedirectToAction("Login", "Auth");

            ViewBag.FilterStartDate = (startDate ?? DateTime.Today.AddMonths(-3)).ToString("yyyy-MM-dd");
            ViewBag.FilterEndDate = (endDate ?? DateTime.Today.AddMonths(3)).ToString("yyyy-MM-dd");

            var applications = await _leaveService.GetMyLeaveApplicationsAsync(startDate, endDate);
            return View(applications);
        }


        [HttpGet]
        public async Task<IActionResult> Manage(
            string? applicantName, int? leaveTypeId, LeaveApplicationStatus? status,
            DateTime? filterStartDate, DateTime? filterEndDate)
        {
            if (!SessionHelper.IsAdmin(HttpContext)) return Forbid();

            ViewBag.ApplicantName = applicantName;
            ViewBag.SelectedLeaveTypeId = leaveTypeId;
            ViewBag.SelectedStatus = status;
            ViewBag.FilterStartDate = filterStartDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.AddMonths(-1).ToString("yyyy-MM-dd");
            ViewBag.FilterEndDate = filterEndDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.AddMonths(1).ToString("yyyy-MM-dd");


            var activeLeaveTypes = await _leaveService.GetActiveLeaveTypesAsync();
            ViewBag.LeaveTypeList = activeLeaveTypes.Select(lt => new SelectListItem
            {
                Value = lt.Id.ToString(),
                Text = lt.Name,
                Selected = lt.Id == leaveTypeId
            }).ToList();

            ViewBag.StatusList = System.Enum.GetValues(typeof(LeaveApplicationStatus))
                .Cast<LeaveApplicationStatus>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = s.ToString(),
                    Selected = s == status
                }).ToList();


            var applications = await _leaveService.GetAllLeaveApplicationsAdminAsync(
                applicantName, leaveTypeId, status,
                filterStartDate ?? DateTime.Today.AddMonths(-1),
                filterEndDate ?? DateTime.Today.AddMonths(1)
            );
            return View(applications);
        }

        [HttpGet]
        public async Task<IActionResult> Review(int id)
        {
            if (!SessionHelper.IsAdmin(HttpContext)) return Forbid();

            var application = await _leaveService.GetLeaveApplicationByIdAsync(id);
            if (application == null)
            {
                TempData["ErrorMessage"] = "Leave application not found.";
                return RedirectToAction(nameof(Manage));
            }

            if (application.Status != LeaveApplicationStatus.Pending)
            {
                TempData["InfoMessage"] = $"This application is already '{application.Status}'. No further action needed.";
            }

            var viewModel = new LeaveApprovalVM
            {
                LeaveApplicationId = application.Id,
                ApplicationDetails = application,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(LeaveApprovalVM model)
        {
            if (!SessionHelper.IsAdmin(HttpContext)) return Forbid();

            if (model.NewStatus == LeaveApplicationStatus.Rejected && string.IsNullOrWhiteSpace(model.AdminRemarks))
            {
                ModelState.AddModelError("AdminRemarks", "Remarks are required when rejecting a leave application.");
            }

            if (!ModelState.IsValid)
            {
                if (model.ApplicationDetails == null && model.LeaveApplicationId > 0)
                {
                    model.ApplicationDetails = await _leaveService.GetLeaveApplicationByIdAsync(model.LeaveApplicationId);
                    if (model.ApplicationDetails == null)
                    {
                        TempData["ErrorMessage"] = "Error retrieving application details. Please try again.";
                        return RedirectToAction(nameof(Manage));
                    }
                }
                return View(model);
            }

            var (success, message) = await _leaveService.UpdateLeaveApplicationStatusAsync(
                model.LeaveApplicationId, model.NewStatus, model.AdminRemarks);

            if (success)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    TempData["SuccessMessage"] = message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = message ?? "Failed to update leave application status.";
            }
            return RedirectToAction(nameof(Manage));
        }
    }
}
