
namespace WorkSpace.Application.DTOs.Bookings;

public class BookingAdminDto
{
    public int Id { get; set; }
    public string BookingCode { get; set; } = default!;
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public int WorkSpaceRoomId { get; set; }
    public string? WorkSpaceRoomTitle { get; set; }
    public DateTimeOffset StartTimeUtc { get; set; }
    public DateTimeOffset EndTimeUtc { get; set; }
    public int NumberOfParticipants { get; set; }
    public decimal FinalAmount { get; set; }
    public string? Currency { get; set; }
    public int BookingStatusId { get; set; }
    public string? BookingStatusName { get; set; }
    public DateTimeOffset CreateUtc { get; set; }
    public DateTimeOffset? CheckedInAt { get; set; }
    public DateTimeOffset? CheckedOutAt { get; set; }
    public bool IsReviewed { get; set; }
}