﻿
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Features.WorkSpace.Commands;
using WorkSpace.Application.Features.WorkSpace.Queries;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/workspaces")]
    public class WorkSpaceController : BaseApiController
    {
       
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetWorkSpaceByIdQuery(id), cancellationToken);
            return Ok(result.Data);
        }

        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetDetailById(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var result = await Mediator.Send(new GetWorkSpaceDetailQuery(id), cancellationToken);
            
            if (result == null)
                return NotFound(new { message = $"Workspace with ID {id} not found" });
            
            return Ok(result);
        }

  
        [HttpGet("rooms")]
        public async Task<IActionResult> GetPagedRooms(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? workSpaceRoomTypeId = null,
            [FromQuery] string? city = null,
            [FromQuery] decimal? minPricePerDay = null,
            [FromQuery] decimal? maxPricePerDay = null,
            [FromQuery] int? minCapacity = null,
            [FromQuery] bool? onlyVerified = null,
            [FromQuery] bool? onlyActive = true,
            [FromQuery] DateTimeOffset? desiredStartUtc = null,
            [FromQuery] DateTimeOffset? desiredEndUtc = null,
            CancellationToken cancellationToken = default)
        {
            var filter = new WorkSpaceFilter
            (
                workSpaceRoomTypeId, city, minPricePerDay, maxPricePerDay, minCapacity, onlyVerified, onlyActive,
                desiredStartUtc, desiredEndUtc);

            var result = await Mediator.Send(new GetWorkSpaceRoomsPagedQuery(filter, pageNumber, pageSize), cancellationToken);
            return Ok(result.Data);
        }


        [HttpGet("rooms/{roomId}")]
        public async Task<IActionResult> GetRoomById([FromRoute] int roomId, CancellationToken cancellationToken)
        {
        
            return Ok("This endpoint is ready. Please implement GetWorkSpaceRoomByIdQuery and its handler.");
        }



        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWorkSpaceRequest request, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new CreateWorkSpaceCommand(request), cancellationToken);
            return Ok(result.Data);
        }
    }
}