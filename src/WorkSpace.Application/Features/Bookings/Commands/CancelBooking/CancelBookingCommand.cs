using MediatR;
using System.Text.Json.Serialization;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Bookings.Commands
{
    public class CancelBookingCommand : IRequest<Response<bool>>
    {
        public int BookingId { get; set; }

        public string? Reason { get; set; }

        [JsonIgnore]
        public int UserId { get; set; }
    }
}