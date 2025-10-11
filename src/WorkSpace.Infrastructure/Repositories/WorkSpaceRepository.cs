using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories;

public class WorkSpaceRepository : GenericRepositoryAsync<Domain.Entities.WorkSpace>, IWorkSpaceRepository
{
    private readonly DbSet<WorkSpace.Domain.Entities.WorkSpace> _workSpaces;
    public WorkSpaceRepository(WorkSpaceContext dbContext) : base(dbContext)
    {
        _workSpaces = dbContext.Set<WorkSpace.Domain.Entities.WorkSpace>();
    }

    public async Task<Domain.Entities.WorkSpace?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _workSpaces
            .Include(x => x.Address)
            .Include(x => x.WorkspaceType)
            .Include(x => x.WorkspaceImages)
            .Include(x =>x.WorkspaceAmenities)
            .ThenInclude(x => x.Amenity)
            .FirstOrDefaultAsync(x =>x .Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Domain.Entities.WorkSpace>, int TotalCount)> GetPagedAsync(WorkSpaceFilter filter, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _workSpaces.Include(x => x.Address)
            .AsNoTracking()
            .AsQueryable();
        
        // Apply filter 
        if (filter.WorkSpaceTypeId is int typeId)
        {
            query = query.Where(x => x.WorkspaceTypeId == typeId); 
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            query = query.Where(x => x.Address!.City == filter.City);
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
            query = query.Where(x => x.IsActive);
        }
        if (filter.OnlyVerified == true)
        {
            query = query.Where(x => x.IsVerified);
        }

        if (filter.DesiredStartUtc.HasValue && filter.DesiredEndUtc.HasValue)
        {
            var start = filter.DesiredStartUtc.Value.UtcDateTime;
            var end = filter.DesiredEndUtc.Value.UtcDateTime;

            query = query.Where(x => !x.BlockedTimeSlots.Any(b => 
            !(b.EndTime <= start || b.StartTime >= end))
            && x.AvailabilitySchedules.Any(a => 
                a.StartTime <= start.TimeOfDay && a.EndTime >= end.TimeOfDay
                )
            );
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.IsVerified)
            .ThenBy(x => x.PricePerDay)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<bool> ExistsTitleForHostAsync(int hostId, string title, CancellationToken cancellationToken = default)
    {
        return await _workSpaces.AnyAsync(x =>
            x.HostId == hostId && x.Title == title, cancellationToken);
    }
}