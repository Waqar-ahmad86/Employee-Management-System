using EMSWebApi.Domain.Entities;

namespace EMSWebApi.Application.Interfaces.Persistence
{
    public interface IEmployeeRepository : IBaseRepository<Employee>
    {
        Task<IEnumerable<Employee>> SearchAsync(string? name, string? department, string? jobTitle);
        Task<int> CountAsync();
    }
}
