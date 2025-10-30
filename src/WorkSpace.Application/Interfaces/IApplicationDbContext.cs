
using Microsoft.EntityFrameworkCore;
using WorkSpace.Domain.Entities; 

namespace WorkSpace.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Booking> Bookings { get; }
        DbSet<Review> Reviews { get; }
        DbSet<AppUser> Users { get; } 
        DbSet<WorkSpaceRoom> WorkSpaceRooms { get; } 
        DbSet<BookingStatus> BookingStatuses { get; }
        DbSet<Payment> Payments { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }
}