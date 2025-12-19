using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Services;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Services.Queries.GetServicesByRoomId
{
    public class GetServicesByRoomIdQueryHandler : IRequestHandler<GetServicesByRoomIdQuery, Response<IEnumerable<WorkSpaceServiceDto>>>
    {
        private readonly IGenericRepositoryAsync<WorkSpaceService> _serviceRepository;
        private readonly IGenericRepositoryAsync<WorkSpaceRoom> _roomRepository;
        private readonly IMapper _mapper;

        public GetServicesByRoomIdQueryHandler(
            IGenericRepositoryAsync<WorkSpaceService> serviceRepository,
            IGenericRepositoryAsync<WorkSpaceRoom> roomRepository,
            IMapper mapper)
        {
            _serviceRepository = serviceRepository;
            _roomRepository = roomRepository;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<WorkSpaceServiceDto>>> Handle(GetServicesByRoomIdQuery request, CancellationToken cancellationToken)
        {
            var room = await _roomRepository.GetByIdAsync(request.WorkSpaceRoomId);

            if (room == null)
            {
                throw new ApiException($"Không tìm thấy phòng với ID: {request.WorkSpaceRoomId}");
            }

            var workSpaceId = room.WorkSpaceId;


            var allServices = await _serviceRepository.GetAllAsync();

            var relevantServices = new List<WorkSpaceService>();
            foreach (var service in allServices)
            {
                if (service.WorkSpaceId == workSpaceId && service.IsActive)
                {
                    relevantServices.Add(service);
                }
            }

            var serviceDtos = _mapper.Map<IEnumerable<WorkSpaceServiceDto>>(relevantServices);

            return new Response<IEnumerable<WorkSpaceServiceDto>>(serviceDtos);
        }
    }
}