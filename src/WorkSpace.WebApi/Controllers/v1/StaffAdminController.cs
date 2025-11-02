using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WorkSpace.Application.Features.Reviews.Commands;
using WorkSpace.Application.Features.Reviews.Queries;

using WorkSpace.Application.Features.Bookings.Queries; 

using WorkSpace.Application.Features.WorkSpace.Commands;
using WorkSpace.Application.Features.WorkSpace.Queries;
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


    /// <summary>
    /// Lấy danh sách workspace chờ duyệt (IsVerified = false)
    /// </summary>
    [HttpGet("workspaces/pending")]
    public async Task<IActionResult> GetPendingWorkSpaces(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPendingWorkSpacesQuery(pageNumber, pageSize);
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lấy tất cả workspace với filter theo trạng thái verified
    /// </summary>
    [HttpGet("workspaces")]
    public async Task<IActionResult> GetAllWorkSpaces(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isVerified = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllWorkSpacesQuery(pageNumber, pageSize, isVerified);
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Duyệt hoặc từ chối workspace
    /// </summary>
    [HttpPut("workspaces/{workSpaceId}/approve")]
    public async Task<IActionResult> ApproveWorkSpace(
        [FromRoute] int workSpaceId,
        [FromBody] Application.DTOs.WorkSpaces.ApproveWorkSpaceDto dto,
        CancellationToken cancellationToken = default)
    {
        dto.WorkSpaceId = workSpaceId;
        var command = new ApproveWorkSpaceCommand(dto);
        var result = await Mediator.Send(command, cancellationToken);
        
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}