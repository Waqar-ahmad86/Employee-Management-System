using EMS.Common.Enums;
using EMSWebApi.Domain.Entities;

namespace EMSWebApi.Application.Interfaces.Persistence
{
    public interface INoticeRepository
    {
        Task<Notice?> GetByIdAsync(Guid id);
        Task<IEnumerable<Notice>> GetAllAsync(bool activeOnly = true);
        Task<IEnumerable<Notice>> GetNoticesForAudienceAsync(List<NoticeAudience> audiences, bool activeOnly = true);
        Task AddAsync(Notice notice);
        void Update(Notice notice);
        void Delete(Notice notice);
    }
}
