namespace WorkSpace.Application.DTOs.Services
{
    public class WorkSpaceServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int WorkSpaceId { get; set; }
    }
}