using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Features.HostProfile.Commands.CreateHostProfile;

namespace WorkSpace.WebApi.Controllers.v1;
[Route("host-profile")]
public class HostProfileController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateHostProfileCommand command)
    {
        return Ok(await Mediator.Send(command));
    }
}