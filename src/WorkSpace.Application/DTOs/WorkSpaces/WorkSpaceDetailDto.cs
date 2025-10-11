namespace WorkSpace.Application.DTOs.WorkSpaces;

public class WorkSpaceDetailDto: WorkSpaceListItemDto
{
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public IEnumerable<string> Images { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<string> Amenities { get; set; } = Enumerable.Empty<string>();
    public double Rating { get; set; }
    public int RatingCount { get; set; }
}