using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Notifications_F.Commands
{
    // Đổi return type từ Response<int> thành int (trả về ID)
    public class CreateAdminNotificationCommand : IRequest<int>
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class CreateAdminNotificationCommandHandler : IRequestHandler<CreateAdminNotificationCommand, int>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public CreateAdminNotificationCommandHandler(INotificationRepository notificationRepository, IAuthenticatedUserService authenticatedUserService)
        {
            _notificationRepository = notificationRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<int> Handle(CreateAdminNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = new Notification
            {
                Title = request.Title,
                Content = request.Content,
                SenderId = int.Parse(_authenticatedUserService.UserId),
                SenderRole = "Admin"
            };

            await _notificationRepository.AddAsync(notification);

            return notification.Id;
        }
    }
}