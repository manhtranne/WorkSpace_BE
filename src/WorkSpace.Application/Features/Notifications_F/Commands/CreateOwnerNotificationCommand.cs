using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Notifications_F.Commands
{
    public class CreateOwnerNotificationCommand : IRequest<Response<int>>
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class CreateOwnerNotificationCommandHandler : IRequestHandler<CreateOwnerNotificationCommand, Response<int>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public CreateOwnerNotificationCommandHandler(INotificationRepository notificationRepository, IAuthenticatedUserService authenticatedUserService)
        {
            _notificationRepository = notificationRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<int>> Handle(CreateOwnerNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = new Notification
            {
                Title = request.Title,
                Content = request.Content,
                SenderId = int.Parse(_authenticatedUserService.UserId),
                SenderRole = "Owner"
            };

            await _notificationRepository.AddAsync(notification);
            return new Response<int>(notification.Id, "Tạo thông báo Owner thành công");
        }
    }
}