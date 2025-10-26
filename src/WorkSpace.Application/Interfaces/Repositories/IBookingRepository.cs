using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories;

public interface IBookingRepository : IGenericRepositoryAsync<Booking>
{
    Task<bool> HasOverlapAsync(int workspaceId, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct);
    Task<Booking?> GetByCodeAsync(string bookingCode, CancellationToken ct);
}