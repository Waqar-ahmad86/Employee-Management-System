using EMS.Common.Enums;
using EMSWebApi.Application.Interfaces.Persistence;
using EMSWebApi.Domain.Entities;
using EMSWebApi.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace EMSWebApi.Infrastructure.Persistence.Repositories
{
    public class LeaveApplicationRepository : BaseRepository<LeaveApplication>, ILeaveApplicationRepository
    {
        public LeaveApplicationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<LeaveApplication>> GetLeaveApplicationsForUserAsync(
            string applicantId,
            DateTime? filterStartDate,
            DateTime? filterEndDate)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(la => la.Applicant)
                .Include(la => la.LeaveType)
                .Include(la => la.ApprovedByUser)
                .Where(la => la.ApplicantId == applicantId);

            if (filterStartDate.HasValue)
            {
                query = query.Where(la => la.EndDate >= filterStartDate.Value);
            }
            if (filterEndDate.HasValue)
            {
                query = query.Where(la => la.StartDate <= filterEndDate.Value);
            }

            return await query.OrderByDescending(la => la.AppliedDate).ToListAsync();
        }

        public async Task<IEnumerable<LeaveApplication>> GetAllPendingAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(la => la.Applicant)
                .Include(la => la.LeaveType)
                .Where(la => la.Status == LeaveApplicationStatus.Pending)
                .OrderBy(la => la.AppliedDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<LeaveApplication>> GetByStatusAsync(LeaveApplicationStatus status)
        {
            return await _dbSet
               .AsNoTracking()
               .Include(la => la.Applicant)
               .Include(la => la.LeaveType)
               .Where(la => la.Status == status)
               .OrderByDescending(la => la.ActionDate ?? la.AppliedDate)
               .ToListAsync();
        }


        public async Task<IEnumerable<LeaveApplication>> GetForDateRangeAndUserAsync(
            string applicantId,
            DateTime startDate,
            DateTime endDate)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(la => la.ApplicantId == applicantId &&
                             la.Status != LeaveApplicationStatus.Rejected &&
                             la.Status != LeaveApplicationStatus.Cancelled &&
                             la.StartDate <= endDate &&
                             la.EndDate >= startDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<LeaveApplication>> GetApprovedLeaveForUserInDateRangeAsync(
            string employeeId,
            DateTime startDate,
            DateTime endDate)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(la => la.ApplicantId == employeeId &&
                             la.Status == LeaveApplicationStatus.Approved &&
                             la.StartDate <= endDate &&
                             la.EndDate >= startDate)
                .ToListAsync();
        }
    }
}
