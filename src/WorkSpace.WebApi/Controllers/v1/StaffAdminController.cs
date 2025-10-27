using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Features.Bookings.Queries; 
using WorkSpace.Application.Features.Reviews.Commands;
using WorkSpace.Application.Features.Reviews.Queries;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Wrappers; 
namespace WorkSpace.WebApi.Controllers.v1;

[Route("api/v1/staff")]
[Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.Staff)}")]
[ApiController]
public class StaffAdminController : BaseApiController
{


    [HttpGet("reviews")]
    public async Task<IActionResult> GetAllReviewsForModeration(
        [FromQuery] GetAllReviewsForModerationQuery query,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("reviews/{reviewId}/moderate")]
    public async Task<IActionResult> ModerateReview(
        [FromRoute] int reviewId,
        [FromBody] ModerateReviewCommand command,
        CancellationToken cancellationToken)
    {
        
        command.ReviewId = reviewId;

        var result = await Mediator.Send(command, cancellationToken);
   
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }


    [HttpGet("bookings")]
    public async Task<IActionResult> GetAllBookings(
        [FromQuery] GetAllBookingsQuery query, 
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result); 
    }
}