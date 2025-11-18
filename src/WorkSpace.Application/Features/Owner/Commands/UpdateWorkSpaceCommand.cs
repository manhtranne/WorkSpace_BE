using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Owner;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Owner.Commands
{
    public class UpdateWorkSpaceCommand : IRequest<Response<int>>
    {
        public int WorkSpaceId { get; set; }
        public int OwnerUserId { get; set; }
        public UpdateWorkSpaceDto Dto { get; set; }
    }

    public class UpdateWorkSpaceCommandHandler : IRequestHandler<UpdateWorkSpaceCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHostProfileAsyncRepository _hostRepo;

        public UpdateWorkSpaceCommandHandler(IApplicationDbContext context, IHostProfileAsyncRepository hostRepo)
        {
            _context = context;
            _hostRepo = hostRepo;
        }

        public async Task<Response<int>> Handle(UpdateWorkSpaceCommand request, CancellationToken cancellationToken)
        {

            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);
            if (hostProfile == null)
            {
                throw new ApiException("Không tìm thấy hồ sơ Host.");
            }

            var workspace = await _context.Workspaces
                .FirstOrDefaultAsync(w => w.Id == request.WorkSpaceId, cancellationToken);

            if (workspace == null)
            {
                throw new ApiException($"Không tìm thấy Workspace ID {request.WorkSpaceId}");
            }

            if (workspace.HostId != hostProfile.Id)
            {
                throw new ApiException("Bạn không có quyền chỉnh sửa Workspace này.");
            }

            if (!string.IsNullOrEmpty(request.Dto.Title))
                workspace.Title = request.Dto.Title;

            if (!string.IsNullOrEmpty(request.Dto.Description))
                workspace.Description = request.Dto.Description;

            if (request.Dto.AddressId.HasValue)
                workspace.AddressId = request.Dto.AddressId.Value;

            if (request.Dto.WorkSpaceTypeId.HasValue)
                workspace.WorkSpaceTypeId = request.Dto.WorkSpaceTypeId.Value;

            if (request.Dto.IsActive.HasValue)
                workspace.IsActive = request.Dto.IsActive.Value;

            workspace.LastModifiedUtc = DateTime.UtcNow;
            workspace.LastModifiedById = request.OwnerUserId;

            await _context.SaveChangesAsync(cancellationToken);

            return new Response<int>(workspace.Id, "Cập nhật Workspace thành công.");
        }
    }
}