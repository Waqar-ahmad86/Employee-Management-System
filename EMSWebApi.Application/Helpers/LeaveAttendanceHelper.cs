using EMS.Common.Enums;
using EMSWebApi.Application.Interfaces.Persistence;
using EMSWebApi.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EMSWebApi.Application.Helpers
{
    public class LeaveAttendanceHelper
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LeaveAttendanceHelper> _logger;

        public LeaveAttendanceHelper(IUnitOfWork unitOfWork, ILogger<LeaveAttendanceHelper> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task HandleAttendanceUpdateForApprovedLeave(LeaveApplication leaveApp)
        {
            if (leaveApp.LeaveType == null)
            {
                var fetchedLeaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(leaveApp.LeaveTypeId);
                if (fetchedLeaveType != null)
                {
                    leaveApp.LeaveType = fetchedLeaveType;
                }
                else
                {
                    _logger.LogWarning("LeaveType could not be loaded for LeaveApplication ID {LeaveAppId}", leaveApp.Id);
                }
            }

            for (DateTime date = leaveApp.StartDate.Date; date <= leaveApp.EndDate.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                var attendanceRecord = await _unitOfWork.Attendances.GetByUserIdAndDateAsync(leaveApp.ApplicantId, date);
                string remarks = $"Approved Leave: {leaveApp.LeaveType?.Name ?? "N/A"} (ID: {leaveApp.Id})";

                if (attendanceRecord == null)
                {
                    attendanceRecord = new AttendanceRecord
                    {
                        UserId = leaveApp.ApplicantId,
                        Date = date,
                        Status = AttendanceStatus.OnLeave,
                        Remarks = remarks
                    };
                    await _unitOfWork.Attendances.AddAsync(attendanceRecord);
                }
                else if (attendanceRecord.Status != AttendanceStatus.OnLeave)
                {
                    _logger.LogInformation("Overriding attendance for User {UserId} on {Date} due to approved Leave ID {LeaveAppId}",
                        leaveApp.ApplicantId, date, leaveApp.Id);
                    attendanceRecord.Status = AttendanceStatus.OnLeave;
                    attendanceRecord.CheckInTime = null;
                    attendanceRecord.CheckOutTime = null;
                    attendanceRecord.Remarks = $"{remarks}. Original: {attendanceRecord.Remarks}";
                    _unitOfWork.Attendances.Update(attendanceRecord);
                }
            }

            await _unitOfWork.CompleteAsync();
        }
    }
}
