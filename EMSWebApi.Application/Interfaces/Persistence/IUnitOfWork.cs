namespace EMSWebApi.Application.Interfaces.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IEmployeeRepository Employees { get; }
        IAttendanceRepository Attendances { get; }
        ILeaveTypeRepository LeaveTypes { get; }
        ILeaveApplicationRepository LeaveApplications { get; }
        INotificationRepository Notifications { get; }
        INoticeRepository Notices { get; }
        Task<int> CompleteAsync();
    }
}
