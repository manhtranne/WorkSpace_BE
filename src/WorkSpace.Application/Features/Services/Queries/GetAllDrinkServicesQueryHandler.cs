using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Services;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

using WorkSpaceEntity = WorkSpace.Domain.Entities.WorkSpace;

namespace WorkSpace.Application.Features.Services.Queries.GetAllDrinkServices
{
    public class GetAllDrinkServicesQueryHandler : IRequestHandler<GetAllDrinkServicesQuery, List<WorkSpaceServiceDto>>
    {
        private readonly IGenericRepositoryAsync<WorkSpaceService> _serviceRepository;

        private readonly IGenericRepositoryAsync<WorkSpaceEntity> _workSpaceRepository;
        private readonly IMapper _mapper;

        public GetAllDrinkServicesQueryHandler(
            IGenericRepositoryAsync<WorkSpaceService> serviceRepository,
            IGenericRepositoryAsync<WorkSpaceEntity> workSpaceRepository,
            IMapper mapper)
        {
            _serviceRepository = serviceRepository;
            _workSpaceRepository = workSpaceRepository;
            _mapper = mapper;
        }

        public async Task<List<WorkSpaceServiceDto>> Handle(GetAllDrinkServicesQuery request, CancellationToken cancellationToken)
        {
            var allWorkSpaces = await _workSpaceRepository.GetAllAsync(cancellationToken);

            var ownerWorkSpaceIds = allWorkSpaces
                                    .Where(w => w.HostId == request.OwnerUserId)
                                    .Select(w => w.Id)
                                    .ToList();

            var allServices = await _serviceRepository.GetAllAsync(cancellationToken);

            var ownerServices = allServices
                                .Where(s => ownerWorkSpaceIds.Contains(s.WorkSpaceId) && s.IsActive)
                                .ToList();

            var dtos = _mapper.Map<List<WorkSpaceServiceDto>>(ownerServices);
            return dtos;
        }
    }
}