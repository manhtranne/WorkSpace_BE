using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Features.Bookings.Commands;

namespace WorkSpace.WebApi.Controllers.v1;
[Route("api/v1/bookings")]
public class BookingController : BaseApiController
{
    /// <summary>
    /// Create a new booking
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == 0)
        {
            return Unauthorized("Invalid user token");
        }

        // Override customerId from token for security
        request.CustomerId = userId;

        var command = new CreateBookingCommand 
        { 
            Model = request 
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}