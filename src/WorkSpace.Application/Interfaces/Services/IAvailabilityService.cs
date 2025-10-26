namespace WorkSpace.Application.Interfaces.Services;

public interface IAvailabilityService
{
    Task<bool> IsAvailableAsync(int workspaceId, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct);
}