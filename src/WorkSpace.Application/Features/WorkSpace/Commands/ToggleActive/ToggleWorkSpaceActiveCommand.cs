using MediatR;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.WorkSpace.Commands.ToggleActive
{
    public class ToggleWorkSpaceActiveCommand : IRequest<Response<bool>>
    {
        public int Id { get; set; }

        public ToggleWorkSpaceActiveCommand(int id)
        {
            Id = id;
        }
    }
}