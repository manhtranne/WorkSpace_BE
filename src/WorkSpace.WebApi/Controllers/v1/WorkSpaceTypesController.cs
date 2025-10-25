using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Features.WorkSpaceTypes.Queries;

namespace WorkSpace.WebApi.Controllers.v1;

[Route("api/v1/workspacetypes")]
public class WorkSpaceTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var workSpaceTypes = await Mediator.Send(new GetAllWorkSpaceTypesQuery(), cancellationToken);
        return Ok(workSpaceTypes);
    }
}

