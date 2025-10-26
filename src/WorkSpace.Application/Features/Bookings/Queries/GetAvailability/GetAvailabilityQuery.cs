using MediatR;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Bookings.Commands;

public class GetAvailabilityQuery : IRequest<Response<bool>>
{
    public int WorkspaceId { get; set; }
    public DateTimeOffset FromUtc { get; set; }
    public DateTimeOffset ToUtc { get; set; }
}