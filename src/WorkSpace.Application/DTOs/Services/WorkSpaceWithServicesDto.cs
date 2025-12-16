
using System.Collections.Generic;

namespace WorkSpace.Application.DTOs.Services
{
    public class WorkSpaceWithServicesDto
    {
        public int WorkSpaceId { get; set; }
        public string WorkSpaceTitle { get; set; } = default!;
        public List<WorkSpaceServiceDto> Services { get; set; } = new();
    }
}