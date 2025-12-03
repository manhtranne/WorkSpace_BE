using MediatR;
using System.Collections.Generic;
using WorkSpace.Application.DTOs.Chat;


namespace WorkSpace.Application.Features.GuestChat.Queries.GetActiveGuestSessions;


public class GetActiveGuestSessionsQuery : IRequest<IEnumerable<GuestChatSessionDto>>
{
    public int? OwnerId { get; set; }
}