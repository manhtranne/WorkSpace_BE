using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Extensions;
using Microsoft.AspNetCore.Authorization;
using WorkSpace.Application.Features.HostProfile.Commands.CreateHostProfile;
using WorkSpace.Application.Features.HostProfile.Commands.UpdateHostProfile;
using WorkSpace.Application.Features.HostProfile.Commands.DeleteHostProfile;
using WorkSpace.Application.Features.HostProfile.Queries.GetHostProfileById;
using WorkSpace.Application.Features.HostProfile.Queries.GetAllHostProfiles;

namespace WorkSpace.WebApi.Controllers.v1;

[Route("api/v1/host-profile")]
[ApiController]
public class HostProfileController : BaseApiController
{

    [HttpPost]
    [Authorize] 
    public async Task<IActionResult> Create([FromBody] CreateHostProfileCommand command)
    {
       
        command.UserId = User.GetUserId();

        var result = await Mediator.Send(command);
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await Mediator.Send(new GetHostProfileByIdQuery(id));
        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
      
            [FromQuery] bool? isVerified = null,
            [FromQuery] string? companyName = null,
            [FromQuery] string? city = null)
    {
        var query = new GetAllHostProfilesQuery
        {
        
            PageNumber = 1,
            PageSize = int.MaxValue, 

            IsVerified = isVerified,
            CompanyName = companyName,
            City = city
        };

        var result = await Mediator.Send(query);
        return Ok(result.Data);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateHostProfileCommand command)
    {
        command.Id = id;
        var result = await Mediator.Send(command);
        return Ok(result.Data);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await Mediator.Send(new DeleteHostProfileCommand(id));
        return Ok(result.Data);
    }


    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
      
        var query = new GetAllHostProfilesQuery
        {
            PageNumber = 1,
            PageSize = 1
        };
        
        var result = await Mediator.Send(query);
        return Ok(result.Data);
    }
}