using MediatR;
using System.Collections.Generic;
using WorkSpace.Application.DTOs.Chat;
// using WorkSpace.Application.Wrappers; // Bỏ wrapper

namespace WorkSpace.Application.Features.GuestChat.Queries.GetGuestChatMessages;

// Thay đổi kiểu trả về
public class GetGuestChatMessagesQuery : IRequest<IEnumerable<GuestChatMessageDto>>
{
    public string SessionId { get; set; } = string.Empty;
}