using EMS.Common.Enums;
using EMSWebApi.Application.Interfaces.Persistence;
using EMSWebApi.Domain.Entities;
using EMSWebApi.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace EMSWebApi.Infrastructure.Persistence.Repositories
{
    public class NoticeRepository : INoticeRepository
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<Notice> _dbSet;

        public NoticeRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Notice>();
        }

        public async Task<Notice?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Notice>> GetAllAsync(bool activeOnly = true)
        {
            var query = _dbSet.AsQueryable();
            if (activeOnly)
            {
                query = query.Where(n => n.IsActive && (n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow));
            }
            return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<Notice>> GetNoticesForAudienceAsync(List<NoticeAudience> audiences, bool activeOnly = true)
        {
            var query = _dbSet.Where(n => audiences.Contains(n.Audience));
            if (activeOnly)
            {
                query = query.Where(n => n.IsActive && (n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow));
            }
            return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task AddAsync(Notice notice)
        {
            await _dbSet.AddAsync(notice);
        }

        public void Update(Notice notice)
        {
            _dbSet.Attach(notice);
            _context.Entry(notice).State = EntityState.Modified;
        }

        public void Delete(Notice notice)
        {
            if (_context.Entry(notice).State == EntityState.Detached)
            {
                _dbSet.Attach(notice);
            }
            _dbSet.Remove(notice);
        }
    }
}
