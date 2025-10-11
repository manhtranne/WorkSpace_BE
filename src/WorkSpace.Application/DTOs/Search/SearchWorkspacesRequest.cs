using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSpace.Application.DTOs.Search;

public class SearchWorkspacesRequest
{
    // Trường search chính
    public string? Ward { get; set; }
    public DateOnly? Date { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public int? Participants { get; set; }
    public string? QueryText { get; set; }

    // Bộ lọc (filter)
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public List<int>? AmenityIds { get; set; }

    // Phân trang + sắp xếp
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "featured"; 
    public bool SortDesc { get; set; } = false;

}