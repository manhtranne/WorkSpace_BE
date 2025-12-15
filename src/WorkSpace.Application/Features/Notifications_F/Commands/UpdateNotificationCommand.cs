using MediatR;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.Application.Features.Notifications_F.Commands
{
    public class UpdateNotificationCommand : IRequest<int>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class UpdateNotificationCommandHandler : IRequestHandler<UpdateNotificationCommand, int>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public UpdateNotificationCommandHandler(INotificationRepository notificationRepository, IAuthenticatedUserService authenticatedUserService)
        {
            _notificationRepository = notificationRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<int> Handle(UpdateNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = await _notificationRepository.GetByIdAsync(request.Id);
            if (notification == null)
                throw new ApiException($"Không tìm thấy thông báo với ID {request.Id}");

            var currentUserId = int.Parse(_authenticatedUserService.UserId);

            if (notification.SenderRole == "Owner" && notification.SenderId != currentUserId)
            {
                throw new ApiException("Bạn không có quyền sửa thông báo này.");
            }

            notification.Title = request.Title;
            notification.Content = request.Content;

            await _notificationRepository.UpdateAsync(notification);

            return notification.Id;
        }
    }
}