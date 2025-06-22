using EMSMvc.Core.Application.DTOs;
using EMSMvc.Core.Application.Services;
using EMSMvc.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace EMSMvc.Controllers
{
    public class LeaveTypesController : Controller
    {
        private readonly LeaveService _leaveService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LeaveTypesController(LeaveService leaveService, IHttpContextAccessor httpContextAccessor)
        {
            _leaveService = leaveService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();
            var leaveTypes = await _leaveService.GetAllLeaveTypesAdminAsync();
            return View(leaveTypes ?? new List<LeaveType>());
        }

        public IActionResult Create()
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();
            return View(new LeaveType { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaveType leaveType)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();

            if (ModelState.IsValid)
            {
                var (success, message, data) = await _leaveService.CreateLeaveTypeAsync(leaveType);
                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = message ?? "Failed to create leave type.";
            }
            return View(leaveType);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();
            if (id == 0) return NotFound();

            var leaveType = await _leaveService.GetLeaveTypeByIdAdminAsync(id);
            if (leaveType == null)
            {
                TempData["ErrorMessage"] = "Leave Type not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(leaveType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LeaveType leaveType)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();
            if (id != leaveType.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var (success, message) = await _leaveService.UpdateLeaveTypeAsync(leaveType);
                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = message ?? "Failed to update leave type.";
            }
            return View(leaveType);
        }


        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext))
                return Json(new { success = false, message = "Unauthorized action." });

            var (success, message) = await _leaveService.DeleteLeaveTypeAsync(id);
            if (success)
            {
                return Json(new { success = true, message = message ?? "Leave Type deleted successfully." });
            }
            return Json(new { success = false, message = message ?? "Failed to delete leave type." });
        }
    }
}
