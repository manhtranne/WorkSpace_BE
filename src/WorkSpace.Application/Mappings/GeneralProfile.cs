using AutoMapper;
using WorkSpace.Application.Features.HostProfile.Commands.CreateHostProfile;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Mappings;

public class GeneralProfile : Profile
{
    public GeneralProfile()
    {
        CreateMap<CreateHostProfileCommand, HostProfile>();
    }
}