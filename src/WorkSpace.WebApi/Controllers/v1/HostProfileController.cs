using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Features.HostProfile.Commands.CreateHostProfile;

namespace WorkSpace.WebApi.Controllers.v1;
[Route("host-profile")]
public class HostProfileController : BaseApiController
{
    [HttpPost]
    // API Request URL: POST /api/v1/host-profile
    public async Task<IActionResult> Post([FromBody] CreateHostProfileCommand command)
    {
        return Ok(await Mediator.Send(command));
    }
}