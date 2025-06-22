using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EMSWebApi.Domain.Entities;
using EMSWebApi.Application.DTOs.Employees;
using EMSWebApi.Application.Interfaces.Persistence;

namespace EMSWebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IUnitOfWork unitOfWork, ILogger<EmployeeController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }


        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees(
            [FromQuery] string? name,
            [FromQuery] string? department,
            [FromQuery] string? jobTitle)
        {
            _logger.LogInformation("Fetching employees with filters - Name: {Name}, Dept: {Department}, Title: {JobTitle}",
                name, department, jobTitle);

            var employees = await _unitOfWork.Employees.SearchAsync(name, department, jobTitle);
            return Ok(employees);
        }


        [HttpGet("GetById/{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            _logger.LogInformation("Fetching employee with ID: {EmployeeId} using UnitOfWork.", id);
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
            {
                _logger.LogWarning("Employee with ID: {EmployeeId} not found.", id);
                return NotFound(new { message = "Employee Not Found." });
            }
            return Ok(employee);
        }

        [HttpPost("Add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddEmployee(CreateEmployeeDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var employee = new Employee
            {
                Name = request.Name,
                Department = request.Department,
                Salary = request.Salary,
                DateOfJoining = request.DateOfJoining,
                ContactNumber = request.ContactNumber,
                JobTitle = request.JobTitle
            };

            await _unitOfWork.Employees.AddAsync(employee);
            try
            {
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Employee created successfully with ID: {EmployeeId}", employee.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error creating employee in database via UnitOfWork.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error saving employee to database." });
            }

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id },
                new { message = "Employee Created Successfully.", data = employee });
        }

        [HttpPut("Update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEmployee(int id, Employee employeeUpdateRequest)
        {
            if (id != employeeUpdateRequest.Id)
            {
                _logger.LogWarning("UpdateEmployee: ID mismatch. Route ID: {RouteId}, Body ID: {BodyId}", id, employeeUpdateRequest.Id);
                return BadRequest(new { message = "Employee ID mismatch." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingEmployee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (existingEmployee == null)
            {
                _logger.LogWarning("UpdateEmployee: Employee with ID {EmployeeId} not found for update.", id);
                return NotFound(new { message = "Employee Not Found to update." });
            }

            existingEmployee.Name = employeeUpdateRequest.Name;
            existingEmployee.Department = employeeUpdateRequest.Department;
            existingEmployee.Salary = employeeUpdateRequest.Salary;
            existingEmployee.DateOfJoining = employeeUpdateRequest.DateOfJoining;
            existingEmployee.ContactNumber = employeeUpdateRequest.ContactNumber;
            existingEmployee.JobTitle = employeeUpdateRequest.JobTitle;

            _unitOfWork.Employees.Update(existingEmployee);

            try
            {
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Employee with ID: {EmployeeId} updated successfully via UnitOfWork.", id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await EmployeeExists(id))
                {
                    _logger.LogWarning(ex, "UpdateEmployee: Concurrency conflict, Employee with ID {EmployeeId} not found.", id);
                    return NotFound(new { message = "Employee Not Found (concurrency)." });
                }
                else
                {
                    _logger.LogError(ex, "UpdateEmployee: Concurrency error for employee ID {EmployeeId} via UnitOfWork.", id);
                    return Conflict(new { message = "The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled." });
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating employee ID {EmployeeId} in database via UnitOfWork.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error saving employee update to database." });
            }

            return Ok(new { message = "Employee Updated Successfully." });
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            _logger.LogInformation("Attempting to delete employee with ID: {EmployeeId} via UnitOfWork.", id);
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
            {
                _logger.LogWarning("DeleteEmployee: Employee with ID {EmployeeId} not found.", id);
                return NotFound(new { message = "Employee Not Found." });
            }

            _unitOfWork.Employees.Delete(employee);
            try
            {
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Employee with ID: {EmployeeId} deleted successfully via UnitOfWork.", id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting employee ID {EmployeeId} from database via UnitOfWork. It might be referenced by other entities.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error deleting employee. It might be in use or a database error occurred." });
            }

            return Ok(new { message = "Employee Deleted Successfully." });
        }

        [HttpPost("DeleteMultiple")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMultipleEmployees([FromBody] DeleteMultipleEmployeesDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.Ids == null || !request.Ids.Any())
            {
                _logger.LogWarning("DeleteMultipleEmployees: No IDs provided for deletion.");
                return BadRequest(new { message = "No employee IDs provided for deletion." });
            }

            _logger.LogInformation("Attempting to delete multiple employees with IDs: {EmployeeIds} via UnitOfWork.", string.Join(", ", request.Ids));

            var employeesToDelete = new List<Employee>();
            var notFoundIds = new List<int>();

            foreach (var id in request.Ids.Distinct())
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(id);
                if (employee != null)
                {
                    employeesToDelete.Add(employee);
                }
                else
                {
                    notFoundIds.Add(id);
                    _logger.LogWarning("DeleteMultipleEmployees: Employee with ID {EmployeeId} not found during bulk delete.", id);
                }
            }

            if (!employeesToDelete.Any())
            {
                if (notFoundIds.Any())
                {
                    return NotFound(new { message = $"None of the specified employees found. Not found IDs: {string.Join(", ", notFoundIds)}" });
                }
                return BadRequest(new { message = "No valid employees found to delete based on provided IDs." });
            }

            _unitOfWork.Employees.DeleteRange(employeesToDelete);

            try
            {
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Successfully deleted {Count} employees via UnitOfWork. Deleted IDs: {DeletedIds}",
                    employeesToDelete.Count, string.Join(", ", employeesToDelete.Select(e => e.Id)));

                if (notFoundIds.Any())
                {
                    return Ok(new
                    {
                        message = $"Successfully deleted {employeesToDelete.Count} employee(s). Some IDs were not found: {string.Join(", ", notFoundIds)}",
                        deletedCount = employeesToDelete.Count,
                        notFoundIds = notFoundIds
                    });
                }
                return Ok(new { message = $"Successfully deleted {employeesToDelete.Count} employee(s).", deletedCount = employeesToDelete.Count });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting multiple employees from database via UnitOfWork. Some might be referenced by other entities. IDs attempted: {AttemptedIds}", string.Join(", ", request.Ids));
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error deleting one or more employees. They might be in use or a database error occurred." });
            }
        }

        private async Task<bool> EmployeeExists(int id)
        {
            return await _unitOfWork.Employees.ExistsAsync(id);
        }
    }
}

