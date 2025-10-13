using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Search;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _search;
    public SearchController(ISearchService search) { _search = search; }


    [HttpPost("workspaces")]
    public async Task<IActionResult> SearchWorkspaces([FromBody] SearchWorkspacesRequest request)
        => Ok(await _search.SearchWorkspacesAsync(request));

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        => Ok(await _search.GetSearchHistoryAsync(pageNumber, pageSize));
}
