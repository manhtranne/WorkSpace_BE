using MediatR;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Services.Commands.DeleteWorkSpaceService
{
    public class DeleteWorkSpaceServiceCommand : IRequest<Response<int>>
    {
        public int ServiceId { get; set; }
        public int OwnerUserId { get; set; }
    }

    public class DeleteWorkSpaceServiceCommandHandler : IRequestHandler<DeleteWorkSpaceServiceCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<WorkSpaceService> _serviceRepository;
        private readonly IWorkSpaceRepository _workSpaceRepository;
        private readonly IHostProfileAsyncRepository _hostRepository;

        public DeleteWorkSpaceServiceCommandHandler(
            IGenericRepositoryAsync<WorkSpaceService> serviceRepository,
            IWorkSpaceRepository workSpaceRepository,
            IHostProfileAsyncRepository hostRepository)
        {
            _serviceRepository = serviceRepository;
            _workSpaceRepository = workSpaceRepository;
            _hostRepository = hostRepository;
        }

        public async Task<Response<int>> Handle(DeleteWorkSpaceServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await _serviceRepository.GetByIdAsync(request.ServiceId);
            if (service == null) throw new ApiException($"Service not found.");

            var workspace = await _workSpaceRepository.GetByIdAsync(service.WorkSpaceId);

            var host = await _hostRepository.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);

            if (host == null || workspace.HostId != host.Id)
            {
                throw new ApiException("You do not own this workspace.");
            }

            service.IsActive = false;

            await _serviceRepository.UpdateAsync(service);

            return new Response<int>(service.Id, "Service deleted (archived) successfully.");
        }
    }
}