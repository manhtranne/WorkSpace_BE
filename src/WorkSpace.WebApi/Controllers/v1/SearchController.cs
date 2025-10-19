
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


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
        public async Task<IActionResult> SearchWorkSpaceRooms(
             [FromQuery] string? ward,
       
             [FromQuery] DateTime? startTime,
             [FromQuery] DateTime? endTime,
       
             [FromQuery] int? capacity,
             [FromQuery] decimal? minPrice = null,
             [FromQuery] decimal? maxPrice = null,
             [FromQuery] List<string>? amenities = null,
             [FromQuery] string? keyword = null)
        {
    
            var request = new SearchRequestDto
            {
                Ward = ward,
          
                StartTime = startTime,
                EndTime = endTime,
           
                Capacity = capacity,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Amenities = amenities ?? new List<string>(),
                Keyword = keyword
            };

            var result = await _searchService.SearchWorkSpaceRoomsAsync(request);

            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(new { Message = result.Message ?? "Search failed." });
            }
        }

    }
}