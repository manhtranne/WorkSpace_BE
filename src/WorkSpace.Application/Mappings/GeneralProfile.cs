using AutoMapper;
using System.Linq;
using WorkSpace.Application.DTOs.Amenities;
using WorkSpace.Application.DTOs.Promotions;
using WorkSpace.Application.DTOs.Users;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Features.HostProfile.Commands.CreateHostProfile;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Mappings
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            CreateMap<AppUser, UserDto>();

            // Amenity mappings
            CreateMap<Amenity, AmenityDto>();

            // Promotion mappings
            CreateMap<Promotion, PromotionDto>()
                .ForMember(d => d.RemainingUsage, o => o.MapFrom(s => s.UsageLimit == 0 ? int.MaxValue : s.UsageLimit - s.UsedCount));

            // HostProfile mappings
            CreateMap<CreateHostProfileCommand, HostProfile>();
            CreateMap<HostProfile, HostProfileDto>()
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.User!.UserName))
                .ForMember(d => d.UserEmail, o => o.MapFrom(s => s.User!.Email))
                .ForMember(d => d.TotalWorkspaces, o => o.MapFrom(s => s.Workspaces.Count))
                .ForMember(d => d.ActiveWorkspaces, o => o.MapFrom(s => s.Workspaces.Count(w => w.IsActive)));

            // WorkSpace Mappings
            CreateMap<CreateWorkSpaceRequest, WorkSpace.Domain.Entities.WorkSpace>();
            CreateMap<WorkSpace.Domain.Entities.WorkSpace, WorkSpaceDetailDto>()
                .ForMember(d => d.AddressLine, o => o.MapFrom(s => $"{s.Address!.Street}, {s.Address.Ward}"))
                .ForMember(d => d.Country, o => o.MapFrom(s => s.Address!.Country))
                .ForMember(d => d.HostName, o => o.MapFrom(s => s.Host.User.GetFullName()))
                .ForMember(d => d.Rooms, o => o.MapFrom(s => s.WorkSpaceRooms));

            // WorkSpaceRoom Mappings (Chi con map cho ListItem)
            CreateMap<WorkSpaceRoom, WorkSpaceRoomListItemDto>()
                .ForMember(d => d.WorkSpaceTitle, o => o.MapFrom(s => s.WorkSpace.Title))
                .ForMember(d => d.City, o => o.MapFrom(s => s.WorkSpace.Address != null ? s.WorkSpace.Address.Ward : null))
                .ForMember(d => d.ThumbnailUrl, o => o.MapFrom(s => s.WorkSpaceRoomImages.FirstOrDefault().ImageUrl))
                .ForMember(d => d.AverageRating, o => o.MapFrom(s => s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : 0))
                .ForMember(d => d.RatingCount, o => o.MapFrom(s => s.Reviews.Count));

            // DA XOA: CreateMap<WorkSpaceRoom, WorkSpaceRoomDetailDto>()
            // Ly do: Da map thu cong trong GetWorkSpaceRoomDetailQueryHandler
        }
    }

    public class HostProfileDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? CompanyName { get; set; }
        public string? Description { get; set; }
        public string? ContactPhone { get; set; }
        public string? LogoUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreateUtc { get; set; }
        public DateTime? LastModifiedUtc { get; set; }

        // User info
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }

        // Statistics
        public int TotalWorkspaces { get; set; }
        public int ActiveWorkspaces { get; set; }
    }
}