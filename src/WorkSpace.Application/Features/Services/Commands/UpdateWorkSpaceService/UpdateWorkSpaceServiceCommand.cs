using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.Services;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Services.Commands.UpdateWorkSpaceService
{
    public class UpdateWorkSpaceServiceCommand : IRequest<Response<int>>
    {
        public UpdateWorkSpaceServiceDto Dto { get; set; } = default!;
        public int OwnerUserId { get; set; }
    }

    public class UpdateWorkSpaceServiceCommandHandler : IRequestHandler<UpdateWorkSpaceServiceCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<WorkSpaceService> _serviceRepository;
        private readonly IWorkSpaceRepository _workSpaceRepository;
        private readonly IHostProfileAsyncRepository _hostRepository;

        public UpdateWorkSpaceServiceCommandHandler(
            IGenericRepositoryAsync<WorkSpaceService> serviceRepository,
            IWorkSpaceRepository workSpaceRepository,
             IHostProfileAsyncRepository hostRepository)
        {
            _serviceRepository = serviceRepository;
            _workSpaceRepository = workSpaceRepository;
            _hostRepository = hostRepository;
        }

        public async Task<Response<int>> Handle(UpdateWorkSpaceServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await _serviceRepository.GetByIdAsync(request.Dto.Id);
            if (service == null) throw new ApiException($"Service not found.");

            // Verify Owner
            var workspace = await _workSpaceRepository.GetByIdAsync(service.WorkSpaceId);

            // SỬA LẠI DÒNG NÀY: Bỏ chữ Async và truyền thêm cancellationToken
            var host = await _hostRepository.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);

            if (host == null || workspace.HostId != host.Id)
            {
                throw new ApiException("You do not own this workspace.");
            }

            // Update fields
            service.Name = request.Dto.Name;
            service.Description = request.Dto.Description;
            service.Price = request.Dto.Price;
            service.ImageUrl = request.Dto.ImageUrl;
            service.IsActive = request.Dto.IsActive;

            await _serviceRepository.UpdateAsync(service);
            return new Response<int>(service.Id, "Service updated successfully.");
        }
    }
}