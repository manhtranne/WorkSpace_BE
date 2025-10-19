namespace WorkSpace.Application.DTOs.WorkSpaces;

public class WorkSpaceDetailResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int HostId { get; set; }
    public string? HostName { get; set; }
    public string? HostCompanyName { get; set; }
    public string? HostContactPhone { get; set; }
    public bool IsHostVerified { get; set; }
    
    // Address information
    public string? AddressLine { get; set; }
    public string? Ward { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    
    // Workspace info
    public string? WorkSpaceType { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    
    // Rooms
    public List<RoomWithAmenitiesDto> Rooms { get; set; } = new();
    
    // Statistics
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public decimal MinPricePerDay { get; set; }
    public decimal MaxPricePerDay { get; set; }
}

public class RoomWithAmenitiesDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RoomType { get; set; }
    
    // Pricing
    public decimal PricePerHour { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal PricePerMonth { get; set; }
    
    // Room details
    public int Capacity { get; set; }
    public double Area { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    
    // Images
    public List<string> Images { get; set; } = new();
    
    // Amenities
    public List<RoomAmenityDto> Amenities { get; set; } = new();
    
    // Reviews
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    
    // Availability (optional - có thể check theo time range)
    public bool IsAvailable { get; set; } = true;
}

public class RoomAmenityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IconClass { get; set; }
}

