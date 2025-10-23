using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Features.WorkSpace.Queries; 

namespace WorkSpace.WebApi.Controllers.v1
{

    [Route("api/v1/workspacerooms")]
    public class WorkSpaceRoomsController : BaseApiController
    {
        /// Get workspace room detail by ID
        [HttpGet("{id}/detail")] 
        public async Task<IActionResult> GetRoomDetailById(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var query = new GetWorkSpaceRoomDetailQuery(id);

            var result = await Mediator.Send(query, cancellationToken);

            if (result.Data == null)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Check available rooms based on start time and end time
        /// </summary>
        /// <param name="request">Request containing start time and end time</param>
        /// <param name="workSpaceRoomTypeId">Filter by room type (optional)</param>
        /// <param name="ward">Filter by ward/district (optional)</param>
        /// <param name="minPricePerDay">Minimum price per day (optional)</param>
        /// <param name="maxPricePerDay">Maximum price per day (optional)</param>
        /// <param name="minCapacity">Minimum capacity (optional)</param>
        /// <param name="onlyVerified">Show only verified rooms (default: true)</param>
        /// <param name="onlyActive">Show only active rooms (default: true)</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of available rooms</returns>
        [HttpPost("check-availability")]
        public async Task<IActionResult> CheckAvailableRooms(
            [FromBody] CheckAvailableRoomsRequest request,
            [FromQuery] int? workSpaceRoomTypeId = null,
            [FromQuery] string? ward = null,
            [FromQuery] decimal? minPricePerDay = null,
            [FromQuery] decimal? maxPricePerDay = null,
            [FromQuery] int? minCapacity = null,
            [FromQuery] bool onlyVerified = true,
            [FromQuery] bool onlyActive = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            // Validate request
            if (request.StartTime >= request.EndTime)
            {
                return BadRequest(new { message = "Start time must be before end time." });
            }

            if (request.StartTime < DateTimeOffset.UtcNow)
            {
                return BadRequest(new { message = "Start time cannot be in the past." });
            }

            // Build full request with filters from query params
            var fullRequest = new CheckAvailableRoomsRequestInternal
            {
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                WorkSpaceRoomTypeId = workSpaceRoomTypeId,
                Ward = ward,
                MinPricePerDay = minPricePerDay,
                MaxPricePerDay = maxPricePerDay,
                MinCapacity = minCapacity,
                OnlyVerified = onlyVerified,
                OnlyActive = onlyActive
            };

            var query = new GetAvailableRoomsQuery(fullRequest, pageNumber, pageSize);
            var result = await Mediator.Send(query, cancellationToken);

            return Ok(result.Data);
        }
    }
}