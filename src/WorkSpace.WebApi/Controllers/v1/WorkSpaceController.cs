using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Features.WorkSpace.Commands;
using WorkSpace.Application.Features.WorkSpace.Queries;

namespace WorkSpace.WebApi.Controllers.v1;
[Route("workspace")]
public class WorkSpaceController : BaseApiController
{
    //Get by Id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        return Ok(await Mediator.Send(new GetWorkSpaceByIdQuery(id), cancellationToken));
    }
    
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? workspaceTypeId = null,
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
            workspaceTypeId, city, minPricePerDay, maxPricePerDay, minCapacity, onlyVerified, onlyActive,
            desiredStartUtc, desiredEndUtc);

        var result = await Mediator.Send(new GetWorkSpacesPagedQuery(filter, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkSpaceRequest request,CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new CreateWorkSpaceCommand(request), cancellationToken);
        return Ok(result);
    }
    
}