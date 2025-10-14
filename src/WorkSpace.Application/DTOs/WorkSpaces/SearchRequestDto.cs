namespace WorkSpace.Application.DTOs.WorkSpaces
{
    public class SearchRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? LocationQuery { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Capacity { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<string>? Amenities { get; set; }
        public string? Keyword { get; set; } 
    }
}