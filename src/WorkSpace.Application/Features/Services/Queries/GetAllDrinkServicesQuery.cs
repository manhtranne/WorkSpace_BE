using MediatR;
using WorkSpace.Application.DTOs.Services;
using System.Collections.Generic;

namespace WorkSpace.Application.Features.Services.Queries.GetAllDrinkServices
{
    public class GetAllDrinkServicesQuery : IRequest<List<WorkSpaceServiceDto>>
    {
        public int OwnerUserId { get; set; }
    }
}