using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.WorkSpace.Queries
{
   
    public class GetTopBookedWorkSpacesQuery : IRequest<Response<IEnumerable<TopBookedWorkSpaceDto>>>
    {
        public int Count { get; set; } = 5;
    }

    public class GetTopBookedWorkSpacesQueryHandler : IRequestHandler<GetTopBookedWorkSpacesQuery, Response<IEnumerable<TopBookedWorkSpaceDto>>>
    {
        private readonly IWorkSpaceRepository _workSpaceRepository;
        private readonly IMapper _mapper;

        public GetTopBookedWorkSpacesQueryHandler(IWorkSpaceRepository workSpaceRepository, IMapper mapper)
        {
            _workSpaceRepository = workSpaceRepository;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<TopBookedWorkSpaceDto>>> Handle(GetTopBookedWorkSpacesQuery request, CancellationToken cancellationToken)
        {
            var rawWorkspaces = await _workSpaceRepository.GetTopBookedWorkSpacesAsync(request.Count, cancellationToken);

            var dtoList = rawWorkspaces.Select(w => new TopBookedWorkSpaceDto
            {
                Id = w.Id,
                Title = w.Title,
                Description = w.Description,
                Address = $"{w.Address?.Street}, {w.Address?.Ward}, {w.Address?.State}, {w.Address?.Country}",

                ImageUrl = w.WorkSpaceImages.FirstOrDefault()?.ImageUrl,

                MinPrice = w.WorkSpaceRooms.Any() ? w.WorkSpaceRooms.Min(r => r.PricePerDay) : 0,

                HostId = w.HostId,
                HostName = w.Host?.User?.GetFullName() ?? "Unknown", 
                HostAvatar = w.Host?.User?.Avatar,          
                HostEmail = w.Host?.User?.Email
            }).ToList();

            return new Response<IEnumerable<TopBookedWorkSpaceDto>>(dtoList);
        }
    }
}