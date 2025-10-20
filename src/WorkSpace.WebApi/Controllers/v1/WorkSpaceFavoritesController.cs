using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Features.Favorites.Commands;
using System.Threading.Tasks;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/favorites")]
    public class WorkSpaceFavoritesController : BaseApiController
    {
      
        [HttpPost]
        [Authorize] 
        public async Task<IActionResult> Create(
            [FromBody] CreateFavoriteRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateFavoriteCommand
            {
                WorkSpaceId = request.WorkSpaceId
            };

            var result = await Mediator.Send(command, cancellationToken);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}