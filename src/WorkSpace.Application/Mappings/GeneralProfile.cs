using AutoMapper;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Features.HostProfile.Commands.CreateHostProfile;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Mappings;

public class GeneralProfile : Profile
{
    public GeneralProfile()
    {
        CreateMap<CreateHostProfileCommand, HostProfile>();
        
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