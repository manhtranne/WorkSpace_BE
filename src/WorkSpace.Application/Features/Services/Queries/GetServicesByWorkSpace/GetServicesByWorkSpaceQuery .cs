using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.Services;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Services.Queries.GetServicesByWorkSpace
{
    public class GetServicesByWorkSpaceQuery : IRequest<Response<List<WorkSpaceServiceDto>>>
    {
        public int WorkSpaceId { get; set; }
        public int OwnerUserId { get; set; } 
    }

    public class GetServicesByWorkSpaceQueryHandler : IRequestHandler<GetServicesByWorkSpaceQuery, Response<List<WorkSpaceServiceDto>>>
    {
        private readonly IGenericRepositoryAsync<WorkSpaceService> _serviceRepository;
        private readonly IMapper _mapper;

        public GetServicesByWorkSpaceQueryHandler(IGenericRepositoryAsync<WorkSpaceService> serviceRepository, IMapper mapper)
        {
            _serviceRepository = serviceRepository;
            _mapper = mapper;
        }

        public async Task<Response<List<WorkSpaceServiceDto>>> Handle(GetServicesByWorkSpaceQuery request, CancellationToken cancellationToken)
        {
            var allServices = await _serviceRepository.GetPagedResponseAsync(1, 100); 

 

            var services = (await _serviceRepository.GetAllAsync())
                            .Where(x => x.WorkSpaceId == request.WorkSpaceId && x.IsActive)
                            .ToList();

            var dtos = _mapper.Map<List<WorkSpaceServiceDto>>(services);
            return new Response<List<WorkSpaceServiceDto>>(dtos);
        }
    }
}