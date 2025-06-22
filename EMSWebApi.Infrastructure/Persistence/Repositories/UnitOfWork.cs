using EMSWebApi.Application.Interfaces.Persistence;
using EMSWebApi.Infrastructure.Persistence.Data;

namespace EMSWebApi.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IEmployeeRepository Employees { get; private set; }
        public IAttendanceRepository Attendances { get; private set; }
        public ILeaveTypeRepository LeaveTypes { get; private set; }
        public ILeaveApplicationRepository LeaveApplications { get; private set; }
        public INotificationRepository Notifications { get; private set; }
        public INoticeRepository Notices { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Employees = new EmployeeRepository(_context);
            Attendances = new AttendanceRepository(_context);
            LeaveTypes = new LeaveTypeRepository(_context);
            LeaveApplications = new LeaveApplicationRepository(_context);
            Notifications = new NotificationRepository(_context);
            Notices = new NoticeRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
