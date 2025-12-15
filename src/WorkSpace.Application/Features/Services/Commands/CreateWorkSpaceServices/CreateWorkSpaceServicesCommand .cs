using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.Services;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Services.Commands.CreateWorkSpaceServices
{
    public class CreateWorkSpaceServicesCommand : IRequest<Response<List<WorkSpaceServiceDto>>>
    {
        public int WorkSpaceId { get; set; }
        public int OwnerUserId { get; set; }
        public List<CreateWorkSpaceServiceDto> Services { get; set; } = new();
    }

    public class CreateWorkSpaceServicesCommandHandler : IRequestHandler<CreateWorkSpaceServicesCommand, Response<List<WorkSpaceServiceDto>>>
    {
        private readonly IWorkSpaceRepository _workSpaceRepository;
        private readonly IGenericRepositoryAsync<WorkSpaceService> _serviceRepository;
        private readonly IMapper _mapper;

        public CreateWorkSpaceServicesCommandHandler(
            IWorkSpaceRepository workSpaceRepository,
            IGenericRepositoryAsync<WorkSpaceService> serviceRepository,
            IMapper mapper)
        {
            _workSpaceRepository = workSpaceRepository;
            _serviceRepository = serviceRepository;
            _mapper = mapper;
        }

        public async Task<Response<List<WorkSpaceServiceDto>>> Handle(CreateWorkSpaceServicesCommand request, CancellationToken cancellationToken)
        {
            var workspace = await _workSpaceRepository.GetByIdAsync(request.WorkSpaceId);
            if (workspace == null)
            {
                throw new ApiException($"Workspace with id {request.WorkSpaceId} not found.");
            }

    
            var hostProfile = workspace.Host;

            var entities = _mapper.Map<List<WorkSpaceService>>(request.Services);

            foreach (var entity in entities)
            {
                entity.WorkSpaceId = request.WorkSpaceId;
                entity.IsActive = true;
                await _serviceRepository.AddAsync(entity);
            }


            var resultDtos = _mapper.Map<List<WorkSpaceServiceDto>>(entities);
            return new Response<List<WorkSpaceServiceDto>>(resultDtos, "Services added successfully.");
        }
    }
}