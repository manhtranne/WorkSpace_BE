using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("locations/suggest")]
        public async Task<IActionResult> GetLocationSuggestions([FromQuery] string query)
        {
            var suggestions = await _searchService.GetLocationSuggestionsAsync(query);
            return Ok(suggestions);
        }

        [HttpGet("wards")]
        public async Task<IActionResult> GetAllWards()
        {
            var wards = await _searchService.GetAllWardsAsync();
            return Ok(wards);
        }

        [HttpGet("workspaces")]
        public async Task<IActionResult> SearchWorkSpaces([FromQuery] SearchRequestDto request)
        {
            var result = await _searchService.SearchWorkSpaceRoomsAsync(request);
            return Ok(result);
        }
    }
}