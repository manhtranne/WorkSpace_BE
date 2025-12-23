namespace WorkSpace.Application.DTOs.Recommendations;

public class UserPreferenceDto
{
    public int UserId { get; set; }
    
    public List<string> FrequentWards { get; set; } = new();
    public string? MostFrequentWard { get; set; }
    
    public decimal AveragePricePerHour { get; set; }
    public decimal MinPriceBooked { get; set; }
    public decimal MaxPriceBooked { get; set; }
    
    public int AverageCapacity { get; set; }
    public int MaxCapacity { get; set; }
    
    public int TotalBookings { get; set; }
    public List<string> PreferredAmenities { get; set; } = new();
    public List<int> PreferredWorkSpaceTypes { get; set; } = new();
    
    public int AverageBookingDurationHours { get; set; }
    public List<DayOfWeek> PreferredDaysOfWeek { get; set; } = new();
}