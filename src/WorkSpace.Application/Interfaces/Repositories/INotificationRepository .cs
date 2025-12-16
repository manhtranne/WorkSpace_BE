using System.Collections.Generic;
using System.Threading.Tasks;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories
{
    public interface INotificationRepository : IGenericRepositoryAsync<Notification>
    {
        Task<IEnumerable<Notification>> GetSystemNotificationsAsync(int pageNumber, int pageSize);

        Task<IEnumerable<Notification>> GetRelevantOwnerNotificationsAsync(int userId, int pageNumber, int pageSize);
    }
}