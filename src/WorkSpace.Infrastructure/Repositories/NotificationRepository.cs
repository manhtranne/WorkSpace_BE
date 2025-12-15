using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories
{
    public class NotificationRepository : GenericRepositoryAsync<Notification>, INotificationRepository
    {
        private readonly DbSet<Notification> _notifications;
        private readonly DbSet<WorkSpaceFavorite> _favorites;
        private readonly DbSet<Booking> _bookings;

        public NotificationRepository(WorkSpaceContext dbContext) : base(dbContext)
        {
            _notifications = dbContext.Set<Notification>();
            _favorites = dbContext.Set<WorkSpaceFavorite>();
            _bookings = dbContext.Set<Booking>();
        }

        public async Task<IEnumerable<Notification>> GetSystemNotificationsAsync(int pageNumber, int pageSize)
        {
            return await _notifications
                .Where(n => n.SenderRole == "Admin")
                .OrderByDescending(n => n.CreateUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetRelevantOwnerNotificationsAsync(int userId, int pageNumber, int pageSize)
        {
            var ownerIdsFromFavorites = _favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.Workspace.Host.UserId);

            var ownerIdsFromBookings = _bookings
                .Where(b => b.CustomerId == userId)
                .Select(b => b.WorkSpaceRoom.WorkSpace.Host.UserId);

            var interestedOwnerIds = ownerIdsFromFavorites.Union(ownerIdsFromBookings);

            var query = _notifications
                .Where(n => n.SenderRole == "Owner" && interestedOwnerIds.Contains(n.SenderId))
                .OrderByDescending(n => n.CreateUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return await query.ToListAsync();
        }
    }
}