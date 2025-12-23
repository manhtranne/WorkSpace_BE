using MediatR;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Application.Exceptions;

namespace WorkSpace.Application.Features.WorkSpace.Commands.ToggleActive
{
    public class ToggleWorkSpaceActiveCommandHandler : IRequestHandler<ToggleWorkSpaceActiveCommand, Response<bool>>
    {
        private readonly IWorkSpaceRepository _workSpaceRepository;
        // Giả sử bạn sử dụng UnitOfWork hoặc gọi SaveChanges từ Repository
        // private readonly IUnitOfWork _unitOfWork; 

        public ToggleWorkSpaceActiveCommandHandler(IWorkSpaceRepository workSpaceRepository)
        {
            _workSpaceRepository = workSpaceRepository;
        }

        public async Task<Response<bool>> Handle(ToggleWorkSpaceActiveCommand request, CancellationToken cancellationToken)
        {
            var workSpace = await _workSpaceRepository.GetByIdAsync(request.Id);

            if (workSpace == null)
            {
                throw new ApiException($"Workspace with id {request.Id} not found.");
            }

            // Toggle trạng thái (True -> False, False -> True)
            workSpace.IsActive = !workSpace.IsActive;

            await _workSpaceRepository.UpdateAsync(workSpace);

            // Nếu pattern repository của bạn cần gọi SaveChanges riêng thì thêm dòng này:
            // await _unitOfWork.Commit(cancellationToken);

            var message = workSpace.IsActive ? "Workspace has been activated." : "Workspace has been blocked.";

            return new Response<bool>(workSpace.IsActive, message);
        }
    }
}