using EMS.Common.Enums;
using EMSWebApi.Domain.Entities;

namespace EMSWebApi.Application.Interfaces.Persistence
{
    public interface ILeaveApplicationRepository : IBaseRepository<LeaveApplication>
    {
        Task<IEnumerable<LeaveApplication>> GetLeaveApplicationsForUserAsync(string applicantId, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<LeaveApplication>> GetAllPendingAsync();
        Task<IEnumerable<LeaveApplication>> GetByStatusAsync(LeaveApplicationStatus status);
        Task<IEnumerable<LeaveApplication>> GetForDateRangeAndUserAsync(string applicantId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<LeaveApplication>> GetApprovedLeaveForUserInDateRangeAsync(string employeeId, DateTime startDate, DateTime endDate);
    }
}
