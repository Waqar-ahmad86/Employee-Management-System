using EMSWebApi.Application.Interfaces.Persistence;
using EMSWebApi.Domain.Entities;
using EMSWebApi.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace EMSWebApi.Infrastructure.Persistence.Repositories
{
    public class AttendanceRepository : BaseRepository<AttendanceRecord>, IAttendanceRepository
    {
        public AttendanceRepository(ApplicationDbContext context) : base(context) { }

        public async Task<AttendanceRecord?> GetByUserIdAndDateAsync(string userId, DateTime date)
        {
            return await _dbSet
                .Include(ar => ar.User)
                .FirstOrDefaultAsync(ar => ar.UserId == userId && ar.Date.Date == date.Date);
        }

        public async Task<IEnumerable<AttendanceRecord>> GetAttendanceForUserAsync(
            string userId,
            DateTime startDate,
            DateTime endDate)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(ar => ar.User)
                .Where(ar => ar.UserId == userId && ar.Date >= startDate.Date && ar.Date <= endDate.Date)
                .OrderBy(ar => ar.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<AttendanceRecord>> GetAttendanceForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
               .AsNoTracking()
               .Include(ar => ar.User)
               .Where(ar => ar.Date >= startDate.Date && ar.Date <= endDate.Date)
               .OrderBy(ar => ar.Date).ThenBy(ar => ar.User.FullName)
               .ToListAsync();
        }

        public async Task<IEnumerable<AttendanceRecord>> GetAttendanceForMonthAsync(
            int year,
            int month,
            string? UserName = null)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var query = _dbSet
                .AsNoTracking()
                .Include(ar => ar.User)
                .Where(ar => ar.Date >= startDate.Date && ar.Date <= endDate.Date);

            if (!string.IsNullOrEmpty(UserName))
            {
                string searchTerm = UserName.ToLower();
                query = query.Where(ar =>
                    ar.User.FullName != null && ar.User.FullName.ToLower().Contains(searchTerm) ||
                    ar.User.UserName != null && ar.User.UserName.ToLower().Contains(searchTerm)
                );
            }

            return await query
                .OrderBy(ar => ar.User.FullName)
                .ThenBy(ar => ar.Date)
                .ToListAsync();
        }

        public IQueryable<AttendanceRecord> GetAllQueryable()
        {
            return _dbSet.AsNoTracking().AsQueryable();
        }
    }
}
