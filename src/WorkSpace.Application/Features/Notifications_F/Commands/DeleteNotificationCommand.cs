using MediatR;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.Application.Features.Notifications_F.Commands
{
    public class DeleteNotificationCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }

    public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, bool>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public DeleteNotificationCommandHandler(INotificationRepository notificationRepository, IAuthenticatedUserService authenticatedUserService)
        {
            _notificationRepository = notificationRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<bool> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = await _notificationRepository.GetByIdAsync(request.Id);
            if (notification == null)
                throw new ApiException($"Không tìm thấy thông báo với ID {request.Id}");

            var currentUserId = int.Parse(_authenticatedUserService.UserId);

            if (notification.SenderRole == "Owner" && notification.SenderId != currentUserId)
            {
                throw new ApiException("Bạn không có quyền xóa thông báo này.");
            }

            await _notificationRepository.DeleteAsync(notification);
            return true;
        }
    }
}