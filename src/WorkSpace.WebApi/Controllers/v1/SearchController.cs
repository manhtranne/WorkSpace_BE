// File: src/WorkSpace.WebApi/Controllers/v1/SearchController.cs
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization; // Thêm cái này để parse TimeSpan

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

        // Action này có thể không cần nữa nếu SearchWorkSpaces đã đủ
        // [HttpGet("workspacerooms")]
        // public async Task<IActionResult> SearchWorkSpaces([FromQuery] SearchRequestDto request)
        // {
        //     // Nên gọi hàm search chính
        //     var result = await _searchService.SearchWorkSpaceRoomsAsync(request);
        //     return Ok(result.Data); // Trả về data hoặc cả response tùy ý
        // }

        [HttpGet("workspaces")]
        public async Task<IActionResult> SearchWorkSpaceRooms(
             [FromQuery] string? location,
             [FromQuery] DateTime? startDate, // Nhận giá trị nullable từ query
             [FromQuery] DateTime? endDate,
             [FromQuery] string? startTime,
             [FromQuery] string? endTime,
             [FromQuery] int? capacity,
             [FromQuery] decimal? minPrice = null,
             [FromQuery] decimal? maxPrice = null,
             [FromQuery] List<string>? amenities = null,
             [FromQuery] string? keyword = null)
        {
            // --- XỬ LÝ NGÀY MẶC ĐỊNH ---
            // Nếu startDate không được cung cấp, gán ngày hôm nay (chỉ phần Date)
            DateTime effectiveStartDate = startDate?.Date ?? DateTime.Today;
            // Nếu endDate không được cung cấp, gán bằng effectiveStartDate
            DateTime effectiveEndDate = endDate?.Date ?? effectiveStartDate;
            // --- KẾT THÚC XỬ LÝ NGÀY MẶC ĐỊNH ---


            TimeSpan? startTimeOfDay = null;
            if (!string.IsNullOrEmpty(startTime) && TimeSpan.TryParse(startTime, CultureInfo.InvariantCulture, out var parsedStartTime))
            {
                startTimeOfDay = parsedStartTime;
            }

            TimeSpan? endTimeOfDay = null;
            if (!string.IsNullOrEmpty(endTime) && TimeSpan.TryParse(endTime, CultureInfo.InvariantCulture, out var parsedEndTime))
            {
                endTimeOfDay = parsedEndTime;
            }

            // Tạo DTO request với ngày đã được xử lý
            var request = new SearchRequestDto
            {
                LocationQuery = location,
                StartDate = effectiveStartDate, // Luôn có giá trị (hoặc từ user hoặc là Today)
                EndDate = effectiveEndDate,     // Luôn có giá trị
                StartTimeOfDay = startTimeOfDay,
                EndTimeOfDay = endTimeOfDay,
                Capacity = capacity,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Amenities = amenities ?? new List<string>(),
                Keyword = keyword
            };

            // Bây giờ SearchRequestDto luôn có StartDate và EndDate hợp lệ
            // Không cần các hàm GetSearchStartDateOnly/GetSearchEndDateOnly nữa
            // Hoặc có thể giữ lại nếu bạn vẫn muốn dùng chúng ở nơi khác

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