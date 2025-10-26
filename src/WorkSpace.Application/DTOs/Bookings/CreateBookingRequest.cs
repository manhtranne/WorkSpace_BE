namespace WorkSpace.Application.DTOs.Bookings;

public class CreateBookingRequest
{
    public int CustomerId { get; set; }
    public int WorkspaceId { get; set; }
    public DateTimeOffset StartTimeUtc { get; set; }
    public DateTimeOffset EndTimeUtc { get; set; }
    public int NumberOfParticipants { get; set; } = 1;
    public string? SpecialRequests { get; set; }
    public string Currency { get; set; } = "VND";
}