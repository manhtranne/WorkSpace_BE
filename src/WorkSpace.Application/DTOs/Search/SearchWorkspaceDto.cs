using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSpace.Application.DTOs.Search;

public class SearchWorkspaceDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string ShortDescription { get; set; }
    public decimal PricePerHour { get; set; }
    public int Capacity { get; set; }
    public double Area { get; set; }
    public string AddressText { get; set; }
    public string Ward { get; set; }
    public string District { get; set; }
    public string City { get; set; }

    public string ThumbnailUrl { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public List<string> AmenityNames { get; set; }
}