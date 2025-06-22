using EMSWebApi.Domain.Entities;

namespace EMSWebApi.Application.Interfaces.Persistence
{
    public interface IAttendanceRepository : IBaseRepository<AttendanceRecord>
    {
        Task<AttendanceRecord?> GetByUserIdAndDateAsync(string userId, DateTime date);
        Task<IEnumerable<AttendanceRecord>> GetAttendanceForUserAsync(string userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<AttendanceRecord>> GetAttendanceForDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<AttendanceRecord>> GetAttendanceForMonthAsync(int year, int month, string? UserName = null);
        IQueryable<AttendanceRecord> GetAllQueryable();
    }
}
