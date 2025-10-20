using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Features.WorkSpace.Queries; 

namespace WorkSpace.WebApi.Controllers.v1
{

    [Route("api/v1/workspacerooms")]
    public class WorkSpaceRoomsController : BaseApiController
    {
        /// Get workspace room detail by ID
        [HttpGet("{id}/detail")] 
        public async Task<IActionResult> GetRoomDetailById(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var query = new GetWorkSpaceRoomDetailQuery(id);

            var result = await Mediator.Send(query, cancellationToken);

            if (result.Data == null)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(result.Data);
        }
    }
}