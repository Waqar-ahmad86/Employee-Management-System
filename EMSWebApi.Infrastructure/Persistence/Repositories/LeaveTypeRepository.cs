using EMSWebApi.Application.Interfaces.Persistence;
using EMSWebApi.Domain.Entities;
using EMSWebApi.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace EMSWebApi.Infrastructure.Persistence.Repositories
{
    public class LeaveTypeRepository : BaseRepository<LeaveType>, ILeaveTypeRepository
    {
        public LeaveTypeRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<LeaveType>> GetActiveLeaveTypesAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(lt => lt.IsActive)
                .OrderBy(lt => lt.Name)
                .ToListAsync();
        }

        public async Task<LeaveType?> GetByNameAsync(string name)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(lt => lt.Name.ToLower() == name.ToLower());
        }
    }
}
