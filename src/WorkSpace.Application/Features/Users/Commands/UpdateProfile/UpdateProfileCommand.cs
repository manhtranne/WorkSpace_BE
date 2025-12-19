using MediatR;
using System.Text.Json.Serialization;
using WorkSpace.Application.DTOs.Account;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Users.Commands.UpdateProfile
{
    public class UpdateProfileCommand : IRequest<Response<string>>
    {
        [JsonIgnore]
        public int UserId { get; set; }
        public UpdateProfileRequest UpdateRequest { get; set; }
    }
}