

using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories
{
    public class WorkSpaceRepository : GenericRepositoryAsync<Domain.Entities.WorkSpace>, IWorkSpaceRepository
    {
        private readonly WorkSpaceContext _context;

        public WorkSpaceRepository(WorkSpaceContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<Domain.Entities.WorkSpace?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Workspaces
                .Include(w => w.Address)
                .Include(w => w.Host)
                    .ThenInclude(h => h.User)
                .Include(w => w.WorkSpaceType)
                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(wr => wr.WorkSpaceRoomType)
                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(wr => wr.WorkSpaceRoomImages)
                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(wr => wr.WorkSpaceRoomAmenities)
                        .ThenInclude(wra => wra.Amenity)
                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(wr => wr.Reviews)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }

        public async Task<WorkSpaceRoom?> GetRoomByIdWithDetailsAsync(int roomId, CancellationToken cancellationToken = default)
        {
            return await _context.WorkSpaceRooms
                .Include(wr => wr.WorkSpace)
                .ThenInclude(w => w.Address)
                .Include(wr => wr.WorkSpace)
                .ThenInclude(w => w.Host)
                .Include(wr => wr.WorkSpaceRoomType)
                .Include(wr => wr.WorkSpaceRoomImages)
                .Include(wr => wr.WorkSpaceRoomAmenities)
                .ThenInclude(wra => wra.Amenity)
                .Include(wr => wr.Reviews)
                .AsNoTracking()
                .FirstOrDefaultAsync(wr => wr.Id == roomId, cancellationToken);
        }

        public async Task<(IReadOnlyList<WorkSpaceRoom> Rooms, int TotalCount)> GetRoomsPagedAsync(WorkSpaceFilter filter, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.WorkSpaceRooms
                .Include(wr => wr.WorkSpace)
                .ThenInclude(w => w.Address)
                .Include(wr => wr.WorkSpaceRoomImages)
  
                .Include(wr => wr.BlockedTimeSlots)
                .AsNoTracking()
                .AsQueryable();

            if (filter.WorkSpaceRoomTypeId is int typeId)
            {
                query = query.Where(x => x.WorkSpaceRoomTypeId == typeId);
            }

         
            if (!string.IsNullOrWhiteSpace(filter.City)) 
            {
                query = query.Where(x => x.WorkSpace.Address!.Ward == filter.City);
            }

            if (filter.MinPricePerDay.HasValue)
            {
                query = query.Where(x => x.PricePerDay >= filter.MinPricePerDay.Value);
            }
            if (filter.MaxPricePerDay.HasValue)
            {
                query = query.Where(x => x.PricePerDay <= filter.MaxPricePerDay.Value);
            }

            if (filter.OnlyActived == true)
            {
                query = query.Where(x => x.IsActive && x.WorkSpace.IsActive);
            }
            if (filter.OnlyVerified == true)
            {
                query = query.Where(x => x.IsVerified && x.WorkSpace.IsVerified);
            }

   
            if (filter.DesiredStartUtc.HasValue && filter.DesiredEndUtc.HasValue)
            {
             
                var start = filter.DesiredStartUtc.Value;
                var end = filter.DesiredEndUtc.Value;


                query = query.Where(x => !x.BlockedTimeSlots.Any(b =>
                    b.StartTime < end.UtcDateTime && b.EndTime > start.UtcDateTime

                   )
                );
            }
     

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(x => x.WorkSpace.IsVerified)
                .ThenBy(x => x.PricePerDay)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            return (items, total);
        }

        public async Task<bool> ExistsTitleForHostAsync(int hostId, string title, CancellationToken cancellationToken = default)
        {
            return await _context.Workspaces.AnyAsync(x =>
                x.HostId == hostId && x.Title == title, cancellationToken);
        }

        public async Task<IReadOnlyList<WorkSpaceRoom>> GetFeaturedRoomsAsync(int count = 5, CancellationToken cancellationToken = default)
        {
            return await _context.WorkSpaceRooms
                .Include(wr => wr.WorkSpace)
                    .ThenInclude(w => w.Address)
                .Include(wr => wr.WorkSpaceRoomImages)
                .Include(wr => wr.Reviews)
                .AsNoTracking()
                .Where(wr => wr.IsActive && wr.IsVerified 
                            && wr.WorkSpace.IsActive && wr.WorkSpace.IsVerified)
                .Select(wr => new 
                {
                    Room = wr,
                    AverageRating = wr.Reviews.Any() ? wr.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = wr.Reviews.Count
                })
                .OrderByDescending(x => x.AverageRating)
                .ThenByDescending(x => x.ReviewCount)
                .Take(count)
                .Select(x => x.Room)
                .ToListAsync(cancellationToken);
        }
    }
}