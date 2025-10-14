using AutoMapper;
<<<<<<< Updated upstream
=======
using System.Linq;
using WorkSpace.Application.DTOs.WorkSpaces;
>>>>>>> Stashed changes
using WorkSpace.Application.Features.HostProfile.Commands.CreateHostProfile;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Mappings
{
    public class GeneralProfile : Profile
    {
<<<<<<< Updated upstream
        CreateMap<CreateHostProfileCommand, HostProfile>();
=======
        public GeneralProfile()
        {
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
                .ForMember(d => d.AddressLine, o => o.MapFrom(s => $"{s.Address!.Street}, {s.Address.Ward}, {s.Address.District}")) // Gộp địa chỉ cho đầy đủ hơn
                .ForMember(d => d.City, o => o.MapFrom(s => s.Address!.District)) // << SỬA LỖI TẠI ĐÂY
                .ForMember(d => d.Country, o => o.MapFrom(s => s.Address!.Country))
                .ForMember(d => d.HostName, o => o.MapFrom(s => s.Host.User.GetFullName()))
                .ForMember(d => d.Rooms, o => o.MapFrom(s => s.WorkSpaceRooms));

            // WorkSpaceRoom Mappings
            CreateMap<WorkSpaceRoom, WorkSpaceRoomListItemDto>()
                .ForMember(d => d.WorkSpaceTitle, o => o.MapFrom(s => s.WorkSpace.Title))
                .ForMember(d => d.City, o => o.MapFrom(s => s.WorkSpace.Address.District)) // << SỬA LỖI TẠI ĐÂY
                .ForMember(d => d.ThumbnailUrl, o => o.MapFrom(s => s.WorkSpaceRoomImages.FirstOrDefault().ImageUrl))
                .ForMember(d => d.AverageRating, o => o.MapFrom(s => s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : 0))
                .ForMember(d => d.RatingCount, o => o.MapFrom(s => s.Reviews.Count));

            CreateMap<WorkSpaceRoom, WorkSpaceRoomDetailDto>()
                .IncludeBase<WorkSpaceRoom, WorkSpaceRoomListItemDto>()
                .ForMember(d => d.AddressLine, o => o.MapFrom(s => $"{s.WorkSpace.Address.Street}, {s.WorkSpace.Address.Ward}, {s.WorkSpace.Address.District}"))
                .ForMember(d => d.Country, o => o.MapFrom(s => s.WorkSpace.Address.Country))
                .ForMember(d => d.Images, o => o.MapFrom(s => s.WorkSpaceRoomImages.Select(i => i.ImageUrl)))
                .ForMember(d => d.Amenities, o => o.MapFrom(s => s.WorkSpaceRoomAmenities.Select(a => a.Amenity!.Name)));
        }
    }

    // HostProfileDto vẫn giữ nguyên như cũ
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
>>>>>>> Stashed changes
    }
}