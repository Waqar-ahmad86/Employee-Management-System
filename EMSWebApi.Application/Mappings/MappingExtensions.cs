using EMS.Common.Enums;
using EMSWebApi.Application.DTOs.Attendance;
using EMSWebApi.Application.DTOs.Leave;
using EMSWebApi.Application.DTOs.Notices;
using EMSWebApi.Application.DTOs.Notifications;
using EMSWebApi.Application.DTOs.Users;
using EMSWebApi.Application.Helpers;
using EMSWebApi.Domain.Entities;
using EMSWebApi.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace EMSWebApi.Application.Mappings
{
    public static partial class MappingExtensions
    {
        public static AttendanceRecordDto ToDto(this AttendanceRecord record, ApplicationUser? user)
        {
            if (record == null) return null;

            return new AttendanceRecordDto
            {
                Id = record.Id,
                UserId = record.UserId,
                UserName = user?.FullName ?? user?.UserName ?? record.User?.FullName ?? record.User?.UserName ?? "N/A",
                Date = record.Date,
                CheckInTime = record.CheckInTime,
                CheckOutTime = record.CheckOutTime,
                Status = record.Status,
                WorkHours = record.WorkHours,
                Remarks = record.Remarks
            };
        }

        public static AttendanceRecordDto ToDto(this AttendanceRecord record)
        {
            if (record == null) return null;

            return new AttendanceRecordDto
            {
                Id = record.Id,
                UserId = record.UserId,
                UserName = record.User?.FullName ?? record.User?.UserName ?? "N/A",
                Date = record.Date,
                CheckInTime = record.CheckInTime,
                CheckOutTime = record.CheckOutTime,
                Status = record.Status,
                WorkHours = record.WorkHours,
                Remarks = record.Remarks
            };
        }

        public static List<AttendanceRecordDto> ToDtoList(this IEnumerable<AttendanceRecord> records)
        {
            return records?.Select(r => r.ToDto()).ToList() ?? new List<AttendanceRecordDto>();
        }

        public static LeaveApplicationDto ToDto(this LeaveApplication la)
        {
            if (la == null) return null;

            return new LeaveApplicationDto
            {
                Id = la.Id,
                ApplicantId = la.ApplicantId,
                ApplicantName = la.Applicant?.FullName ?? la.Applicant?.UserName ?? "N/A",
                LeaveTypeId = la.LeaveTypeId,
                LeaveTypeName = la.LeaveType?.Name ?? "N/A",
                StartDate = la.StartDate,
                EndDate = la.EndDate,
                NumberOfDays = la.NumberOfDays,
                Reason = la.Reason,
                Status = la.Status,
                AppliedDate = la.AppliedDate,
                ApprovedByUserId = la.ApprovedByUserId,
                ApprovedByUserName = la.ApprovedByUser?.FullName ?? la.ApprovedByUser?.UserName,
                ActionDate = la.ActionDate,
                AdminRemarks = la.AdminRemarks
            };
        }


        public static MonthlyAttendanceReportItemDto ToMonthlyReportItem(
            this IGrouping<(string UserId, string? FullName, string? UserName), AttendanceRecord> g, int year, int month)
        {
            int totalDays = DateTime.DaysInMonth(year, month);
            int workingDays = AttendanceHelper.GetWorkingDaysInMonth(year, month);

            var present = g.Where(r =>
                r.CheckInTime.HasValue &&
                r.CheckOutTime.HasValue &&
                r.Status == AttendanceStatus.Present);

            int presentCount = present.Count();
            double workHours = present.Sum(r => r.WorkHours ?? 0);
            int leaveCount = g.Count(r => r.Status == AttendanceStatus.OnLeave);
            int absentCount = Math.Max(0, workingDays - (presentCount + leaveCount));

            return new MonthlyAttendanceReportItemDto
            {
                UserId = g.Key.UserId,
                UserName = g.Key.FullName ?? g.Key.UserName ?? "N/A",
                TotalDaysInMonth = totalDays,
                WorkingDaysInMonth = workingDays,
                PresentDays = presentCount,
                AbsentDays = absentCount,
                LeaveDays = leaveCount,
                TotalWorkHours = workHours
            };
        }

        public static LeaveTypeDto ToDto(this LeaveType leaveType)
        {
            if (leaveType == null) return null;

            return new LeaveTypeDto
            {
                Id = leaveType.Id,
                Name = leaveType.Name,
                Description = leaveType.Description,
                DefaultDaysAllowed = leaveType.DefaultDaysAllowed,
                IsActive = leaveType.IsActive
            };
        }

        public static LeaveType ToEntity(this CreateLeaveTypeDto dto)
        {
            if (dto == null) return null;

            return new LeaveType
            {
                Name = dto.Name,
                Description = dto.Description,
                DefaultDaysAllowed = dto.DefaultDaysAllowed,
                IsActive = dto.IsActive
            };
        }

        public static void UpdateEntity(this LeaveType entity, LeaveTypeDto dto)
        {
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.DefaultDaysAllowed = dto.DefaultDaysAllowed;
            entity.IsActive = dto.IsActive;
        }

        public static NoticeDto ToDto(this Notice notice)
        {
            if (notice == null) return null;

            return new NoticeDto
            {
                Id = notice.Id,
                Title = notice.Title,
                Content = notice.Content,
                CreatedAt = notice.CreatedAt,
                ExpiresAt = notice.ExpiresAt,
                Audience = notice.Audience,
                IsActive = notice.IsActive
            };
        }

        public static List<NoticeDto> ToDtoList(this IEnumerable<Notice> notices)
        {
            return notices?.Select(n => n.ToDto()).ToList() ?? new List<NoticeDto>();
        }

        public static Notice ToEntity(this CreateNoticeDto dto, string userId)
        {
            if (dto == null) return null;

            return new Notice
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = dto.ExpiresAt,
                CreatedByUserId = userId,
                Audience = dto.Audience,
                IsActive = dto.IsActive
            };
        }

        public static void UpdateEntity(this Notice notice, UpdateNoticeDto dto)
        {
            if (notice == null || dto == null) return;

            notice.Title = dto.Title;
            notice.Content = dto.Content;
            notice.ExpiresAt = dto.ExpiresAt;
            notice.Audience = dto.Audience;
            notice.IsActive = dto.IsActive;
        }

        public static NotificationDto ToDto(this Notification notification)
        {
            if (notification == null) return null;

            return new NotificationDto
            {
                Id = notification.Id,
                Message = notification.Message,
                Link = notification.Link,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }

        public static List<NotificationDto> ToDtoList(this IEnumerable<Notification> notifications)
        {
            return notifications?.Select(n => n.ToDto()).ToList() ?? new List<NotificationDto>();
        }

        public static async Task<UserDto> ToUserDtoAsync(this ApplicationUser user, UserManager<ApplicationUser> userManager)
        {
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                IsActive = user.IsActive,
                Roles = await userManager.GetRolesAsync(user)
            };
        }

    }
}
