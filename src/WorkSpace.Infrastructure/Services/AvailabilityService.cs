using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.Infrastructure.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly WorkSpaceContext _context;
    public AvailabilityService(WorkSpaceContext context) => _context = context;

    public async Task<bool> IsAvailableAsync(int workspaceId, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct)
    {
        var overlap = await _context.Bookings
            .Where(b => b.WorkSpaceRoomId == workspaceId) 
            .AnyAsync(b => !(b.EndTimeUtc <= startUtc || b.StartTimeUtc >= endUtc), ct);

        return !overlap;
    }
}