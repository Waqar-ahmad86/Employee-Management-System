using Microsoft.AspNetCore.Mvc;
using EMSMvc.Core.Application.Services;
using EMSMvc.Core.Application.DTOs;

namespace EMSMvc.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeService _employeeService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmployeeController(EmployeeService employeeService, IHttpContextAccessor httpContextAccessor)
        {
            _employeeService = employeeService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index(string? nameSearch, string? deptSearch, string? titleSearch)
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.UserRole = _httpContextAccessor.HttpContext?.Session.GetString("UserRole");
            ViewBag.IsAdmin = ViewBag.UserRole == "Admin";

            var employees = await _employeeService.GetEmployeesAsync(nameSearch, deptSearch, titleSearch);

            ViewBag.CurrentNameSearch = nameSearch;
            ViewBag.CurrentDeptSearch = deptSearch;
            ViewBag.CurrentTitleSearch = titleSearch;

            return View(employees);
        }

        public async Task<IActionResult> EmployeeDetails(int id)
        {
            var employee = await _employeeService.GetEmployeeAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        public IActionResult CreateEmployee()
        {
            var userRole = _httpContextAccessor.HttpContext?.Session.GetString("UserRole");

            if (userRole != "Admin")
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(Employee employee)
        {
            var userRole = _httpContextAccessor.HttpContext?.Session.GetString("UserRole");

            if (userRole != "Admin")
            {
                return RedirectToAction("Index");
            }
            if (!ModelState.IsValid)
            {
                return View(employee);
            }

            if (await _employeeService.CreateEmployeeAsync(employee))
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Failed to create employee. Please try again.");
            return View(employee);
        }

        public async Task<IActionResult> EditEmployee(int id)
        {
            var userRole = _httpContextAccessor.HttpContext?.Session.GetString("UserRole");

            if (userRole != "Admin")
            {
                return RedirectToAction("Index");
            }

            var employee = await _employeeService.GetEmployeeAsync(id);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return NotFound();
            }

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmployee(int id, Employee employee)
        {
            var userRole = _httpContextAccessor.HttpContext?.Session.GetString("UserRole");

            if (userRole != "Admin")
            {
                return RedirectToAction("Index");
            }

            if (id != employee.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(employee);
            }

            if (await _employeeService.UpdateEmployeeAsync(id, employee))
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Failed to update employee. Please try again.");
            return View(employee);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userRole = _httpContextAccessor.HttpContext?.Session.GetString("UserRole");

            if (userRole != "Admin")
            {
                return Json(new { success = false, message = "Unauthorized action." });
            }

            if (await _employeeService.DeleteEmployeeAsync(id))
            {
                return Json(new { success = true, message = "Employee Deleted Successfully." });
            }

            return Json(new { success = false, message = "Failed to delete employee." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelectedConfirmed(List<int> ids)
        {
            var userRole = _httpContextAccessor.HttpContext?.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return Json(new { success = false, message = "Unauthorized action." });
            }

            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "No employees selected for deletion." });
            }

            var (success, message) = await _employeeService.DeleteEmployeesAsync(ids);

            if (success)
            {
                return Json(new { success = true, message = message ?? "Selected employees deleted successfully." });
            }

            return Json(new { success = false, message = message ?? "Failed to delete selected employees." });
        }
    }
}
