using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkSpacesController : ControllerBase
{
    private readonly IWorkSpaceService _wsService;
    public WorkSpacesController(IWorkSpaceService wsService) { _wsService = wsService; }


    [HttpGet("featured")]
    // API Request URL: GET /api/WorkSpaces/featured
    public async Task<IActionResult> GetFeatured([FromQuery] int take = 4)
        => Ok(await _wsService.GetFeaturedAsync(take));
}