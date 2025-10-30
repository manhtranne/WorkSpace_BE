using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories;

public class BlockedTimeSlotRepository : GenericRepositoryAsync<BlockedTimeSlot>, IBlockedTimeSlotRepository
{
    private readonly WorkSpaceContext _context;

    public BlockedTimeSlotRepository(WorkSpaceContext dbContext) : base(dbContext)
    {
        _context = dbContext;
    }

    public async Task<IReadOnlyList<BlockedTimeSlot>> GetBlockedTimeSlotsForRoomAsync(
        int workSpaceRoomId, 
        DateTimeOffset startTime, 
        DateTimeOffset endTime, 
        CancellationToken cancellationToken = default)
    {
        return await _context.BlockedTimeSlots
            .Where(b => b.WorkSpaceRoomId == workSpaceRoomId
                        && b.StartTime < endTime.DateTime
                        && b.EndTime > startTime.DateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsTimeSlotBlockedAsync(
        int workSpaceRoomId, 
        DateTimeOffset startTime, 
        DateTimeOffset endTime, 
        CancellationToken cancellationToken = default)
    {
        return await _context.BlockedTimeSlots
            .AnyAsync(b => b.WorkSpaceRoomId == workSpaceRoomId
                           && b.StartTime < endTime.DateTime
                           && b.EndTime > startTime.DateTime, 
                cancellationToken);
    }

    public async Task CreateBlockedTimeSlotForBookingAsync(
        int workSpaceRoomId, 
        int bookingId, 
        DateTimeOffset startTime, 
        DateTimeOffset endTime, 
        CancellationToken cancellationToken = default)
    {
        var blockedSlot = new BlockedTimeSlot
        {
            WorkSpaceRoomId = workSpaceRoomId,
            StartTime = startTime.DateTime,
            EndTime = endTime.DateTime,
            Reason = $"Blocked for booking ID: {bookingId}",
            CreatedAt = DateTime.UtcNow
        };

        await _context.BlockedTimeSlots.AddAsync(blockedSlot, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveBlockedTimeSlotForBookingAsync(
        int bookingId, 
        CancellationToken cancellationToken = default)
    {
        var blockedSlots = await _context.BlockedTimeSlots
            .Where(b => b.Reason == $"Blocked for booking ID: {bookingId}")
            .ToListAsync(cancellationToken);

        if (blockedSlots.Any())
        {
            _context.BlockedTimeSlots.RemoveRange(blockedSlots);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

