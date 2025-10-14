using AutoMapper;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Features.HostProfile.Commands.CreateHostProfile;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Mappings;

public class GeneralProfile : Profile
{
    public GeneralProfile()
    {
        // HostProfile mappings
        CreateMap<CreateHostProfileCommand, HostProfile>();
        CreateMap<HostProfile, HostProfileDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User!.UserName))
            .ForMember(d => d.UserEmail, o => o.MapFrom(s => s.User!.Email))
            .ForMember(d => d.TotalWorkspaces, o => o.MapFrom(s => s.Workspaces.Count))
            .ForMember(d => d.ActiveWorkspaces, o => o.MapFrom(s => s.Workspaces.Count(w => w.IsActive)));
        
        // WorkSpace
        CreateMap<WorkSpace.Domain.Entities.WorkSpace, CreateWorkSpaceRequest>().ReverseMap();
        CreateMap<UpdateWorkSpaceRequest, WorkSpace.Domain.Entities.WorkSpace>()
            .ForMember(d => d.HostId, o => o.Ignore())
            .ForMember(d => d.AddressId, o => o.Ignore());
        CreateMap<WorkSpace.Domain.Entities.WorkSpace, WorkSpaceListItemDto>();
        CreateMap<WorkSpace.Domain.Entities.WorkSpace, WorkSpaceDetailDto>()
            .ForMember(d => d.AddressLine, o => o.MapFrom(s => s.Address!.Street))
            .ForMember(d => d.City, o => o.MapFrom(s => s.Address!.City))
            .ForMember(d => d.Country, o => o.MapFrom(s => s.Address!.Country))
            .ForMember(d => d.Images, o => o.MapFrom(s => s.WorkspaceImages.Select(i => i.ImageUrl)))
            .ForMember(d => d.Amenities, o => o.MapFrom(s => s.WorkspaceAmenities.Select(a => a.Amenity!.Name)))
            .ForMember(d => d.Rating, o => o.MapFrom(s => s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : 0))
            .ForMember(d => d.RatingCount, o => o.MapFrom(s => s.Reviews.Count));
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