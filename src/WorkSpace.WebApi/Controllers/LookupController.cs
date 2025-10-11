using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LookupController : ControllerBase
{
    private readonly ILookupService _lookup;
    public LookupController(ILookupService lookup) { _lookup = lookup; }


    [HttpGet("wards")]
    public async Task<IActionResult> GetWards([FromQuery] string? q, [FromQuery] int? take)
        => Ok(await _lookup.GetAllWardsAsync(q, take));
}
