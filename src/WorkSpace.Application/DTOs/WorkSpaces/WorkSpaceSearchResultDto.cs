
namespace WorkSpace.Application.DTOs.WorkSpaces
{
    public class WorkSpaceSearchResultDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Ward { get; set; }
        public string? Street { get; set; }
        public string? HostName { get; set; } 
    }
}