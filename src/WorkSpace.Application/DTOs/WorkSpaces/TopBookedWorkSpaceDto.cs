namespace WorkSpace.Application.DTOs.WorkSpaces
{
    public class TopBookedWorkSpaceDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public string Address { get; set; } = default!; 
        public decimal MinPrice { get; set; } 
        public string? ImageUrl { get; set; }

        public int HostId { get; set; }
        public string HostName { get; set; } = default!;
        public string? HostAvatar { get; set; }
        public string? HostEmail { get; set; }
    }
}