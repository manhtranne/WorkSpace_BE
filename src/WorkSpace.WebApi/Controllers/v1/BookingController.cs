using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Features.Bookings.Commands;

namespace WorkSpace.WebApi.Controllers.v1;
[Route("api/v1/bookings")]
public class BookingController : BaseApiController
{
    /// <summary>
    /// Create a new booking status
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}