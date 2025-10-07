using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.HostProfile.Commands.CreateHostProfile;

public partial class CreateHostProfileCommand : IRequest<Response<int>>
{
    public int UserId { get; set; }

    public string? CompanyName { get; set; }
    
    public string? Description { get; set; }
    
    public string? ContactPhone { get; set; }
    
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }

}

public class CreateHostProfileCommandHandler : IRequestHandler<CreateHostProfileCommand, Response<int>>
{
    private readonly IHostProfileAsyncRepository _hostProfileRepository;
    private readonly IMapper _mapper;
    public CreateHostProfileCommandHandler(IHostProfileAsyncRepository hostProfileRepository, IMapper mapper)
    {
        _hostProfileRepository = hostProfileRepository;
        _mapper = mapper;
    }
    public async Task<Response<int>> Handle(CreateHostProfileCommand request,CancellationToken cancellationToken)
    {
        var hostProfile = _mapper.Map<Domain.Entities.HostProfile>(request);
        // check if user already has a host profile
        var existingProfile = await _hostProfileRepository.GetHostProfileByUserId(hostProfile.UserId);
        if (existingProfile != null)
        {
            return new Response<int>($"User with id {hostProfile.UserId} already has a host profile.");
        }
        hostProfile.CreateUtc = DateTime.UtcNow;
        hostProfile.IsVerified = false;
        try
        {
            await _hostProfileRepository.AddAsync(hostProfile);
            return new Response<int>(hostProfile.Id, "Host profile created successfully.");
        }
        catch (DbUpdateException ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return new Response<int>($"Save Host profile failed. {msg}");
        }
    }

}

