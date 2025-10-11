namespace WorkSpace.Application.DTOs.WorkSpaces;

public class WorkSpaceListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public int HostId { get; set; }
    public int AddressId { get; set; }
    public int WorkspaceTypeId { get; set; }
    public decimal PricePerHour { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal PricePerMonth { get; set; }
    public int Capacity { get; set; }
    public double Area { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
}