using EMSMvc.Core.Application.Services;
using EMSMvc.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace EMSMvc.Controllers
{
    public class NoticesController : Controller
    {
        private readonly NoticeService _noticeService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NoticesController(NoticeService noticeService, IHttpContextAccessor httpContextAccessor)
        {
            _noticeService = noticeService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            if (!SessionHelper.IsUserLoggedIn(_httpContextAccessor.HttpContext))
                return RedirectToAction("Login", "Auth");

            var notices = await _noticeService.GetActiveNoticesForCurrentUserAsync();
            return View(notices);
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();
            var notices = await _noticeService.GetAllNoticesAdminAsync(activeOnly: false);
            return View("Admin/Manage", notices);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();
            return View("Admin/Create", new ViewModels.CreateNoticeVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ViewModels.CreateNoticeVM model)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();
            if (!ModelState.IsValid)
            {
                return View("Admin/Create", model);
            }
            var (success, message, data) = await _noticeService.CreateNoticeAsync(model);
            if (success)
            {
                //TempData["SuccessMessage"] = message;
                return RedirectToAction(nameof(Manage));
            }
            TempData["ErrorMessage"] = message;
            return View("Admin/Create", model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();
            var notice = await _noticeService.GetNoticeByIdAdminAsync(id);
            if (notice == null)
            {
                TempData["ErrorMessage"] = "Notice not found.";
                return RedirectToAction(nameof(Manage));
            }
            var viewModel = new ViewModels.EditNoticeVM
            {
                Id = notice.Id,
                Title = notice.Title,
                Content = notice.Content,
                ExpiresAt = notice.ExpiresAt,
                Audience = notice.Audience,
                IsActive = notice.IsActive
            };
            return View("Admin/Edit", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ViewModels.EditNoticeVM model)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext)) return Forbid();
            if (!ModelState.IsValid)
            {
                return View("Admin/Edit", model);
            }
            var (success, message) = await _noticeService.UpdateNoticeAsync(model);
            if (success)
            {
                //TempData["SuccessMessage"] = message;
                return RedirectToAction(nameof(Manage));
            }
            TempData["ErrorMessage"] = message;
            return View("Admin/Edit", model);
        }

        [HttpPost, ActionName("DeleteNoticeConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNoticeConfirmed(Guid id)
        {
            if (!SessionHelper.IsAdmin(_httpContextAccessor.HttpContext))
                return Json(new { success = false, message = "Unauthorized." });

            var (success, message) = await _noticeService.DeleteNoticeAsync(id);
            if (success)
            {
                return Json(new { success = true, message = message });
            }
            return Json(new { success = false, message = message });
        }
    }
}