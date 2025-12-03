using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.GuestChat.Commands.OwnerReplyToGuest;

public class OwnerReplyToGuestCommand : IRequest<Response<GuestChatMessageDto>>
{
    public string SessionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int OwnerUserId { get; set; }
}

