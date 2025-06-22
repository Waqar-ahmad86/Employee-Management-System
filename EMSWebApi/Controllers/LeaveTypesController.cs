using EMSWebApi.Application.DTOs.Leave;
using EMSWebApi.Application.Interfaces.Persistence;
using EMSWebApi.Application.Mappings;
using EMSWebApi.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMSWebApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveTypesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LeaveTypesController> _logger;
        private readonly ApplicationDbContext _context;

        public LeaveTypesController(IUnitOfWork unitOfWork, ILogger<LeaveTypesController> logger, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeaveTypeDto>>> GetAllLeaveTypes()
        {
            var leaveTypes = await _unitOfWork.LeaveTypes.GetAllAsync();
            return Ok(leaveTypes.Select(lt => lt.ToDto()).ToList());
        }

        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<LeaveTypeDto>>> GetActiveLeaveTypes()
        {
            var leaveTypes = await _unitOfWork.LeaveTypes.GetActiveLeaveTypesAsync();
            return Ok(leaveTypes.Select(lt => lt.ToDto()).ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveTypeDto>> GetLeaveType(int id)
        {
            var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(id);
            if (leaveType == null)
                return NotFound(new { Message = "Leave Type not found." });

            return Ok(leaveType.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<LeaveTypeDto>> CreateLeaveType([FromBody] CreateLeaveTypeDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _unitOfWork.LeaveTypes.GetByNameAsync(dto.Name);
            if (existing != null)
                return Conflict(new { Message = $"Leave Type with name '{dto.Name}' already exists." });

            var leaveType = dto.ToEntity();
            await _unitOfWork.LeaveTypes.AddAsync(leaveType);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Leave Type '{LeaveTypeName}' created with ID {LeaveTypeId}", leaveType.Name, leaveType.Id);

            return CreatedAtAction(nameof(GetLeaveType), new { id = leaveType.Id }, leaveType.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLeaveType(int id, [FromBody] LeaveTypeDto dto)
        {
            if (id != dto.Id) return BadRequest(new { Message = "ID mismatch." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(id);
            if (leaveType == null) return NotFound(new { Message = "Leave Type not found." });

            if (!string.Equals(leaveType.Name, dto.Name, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _unitOfWork.LeaveTypes.GetByNameAsync(dto.Name);
                if (existing != null && existing.Id != id)
                    return Conflict(new { Message = $"Another Leave Type with name '{dto.Name}' already exists." });
            }

            leaveType.UpdateEntity(dto);
            _unitOfWork.LeaveTypes.Update(leaveType);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Leave Type ID {LeaveTypeId} updated.", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLeaveType(int id)
        {
            var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(id);
            if (leaveType == null) return NotFound(new { Message = "Leave Type not found." });

            bool isInUse = await _context.LeaveApplications.AnyAsync(la => la.LeaveTypeId == id);
            if (isInUse)
            {
                return BadRequest(new
                {
                    Message = "Cannot delete Leave Type. It is currently used by leave applications. Consider deactivating it instead."
                });
            }

            _unitOfWork.LeaveTypes.Delete(leaveType);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Leave Type ID {LeaveTypeId} deleted.", id);
            return NoContent();
        }
    }
}

