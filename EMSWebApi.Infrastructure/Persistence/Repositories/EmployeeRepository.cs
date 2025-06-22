using EMSWebApi.Application.Interfaces.Persistence;
using EMSWebApi.Domain.Entities;
using EMSWebApi.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace EMSWebApi.Infrastructure.Persistence.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Employee>> SearchAsync(string? name, string? department, string? jobTitle)
        {
            var query = _dbSet.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(e => e.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(department))
            {
                query = query.Where(e => e.Department != null && e.Department.Contains(department));
            }

            if (!string.IsNullOrWhiteSpace(jobTitle))
            {
                query = query.Where(e => e.JobTitle != null && e.JobTitle.Contains(jobTitle));
            }

            query = query.OrderBy(e => e.Name);

            return await query.ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }
    }
}
