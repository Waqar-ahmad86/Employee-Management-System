using EMSWebApi.Domain.Entities;

namespace EMSWebApi.Application.Interfaces.Persistence
{
    public interface ILeaveTypeRepository : IBaseRepository<LeaveType>
    {
        Task<IEnumerable<LeaveType>> GetActiveLeaveTypesAsync();
        Task<LeaveType?> GetByNameAsync(string name);
    }
}
